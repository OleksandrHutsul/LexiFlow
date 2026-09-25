using System.Text;
using System.Text.Json;
using LexiFlow.Models.AuthenticationModels;
using LexiFlow.Services.LocalStorage;
using Microsoft.JSInterop;

namespace LexiFlow.Services.AuthTokenStore;

public class AuthTokenStore : IAuthTokenStore
{
    private const string TokenKey = "lexiflow:auth:token";
    private const string UserKey = "lexiflow:auth:user";
    private const string ExpiryKey = "lexiflow:auth:expires";
    private const int LoadAttempts = 6;
    private const int RetryDelayMilliseconds = 40;

    private readonly ILocalStorageService _storage;

    private string? _token;
    private AuthenticatedUser? _user;
    private DateTimeOffset? _expiresAt;
    private bool _loaded;

    public AuthTokenStore(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task SaveAsync(AuthenticationResponse response)
    {
        _token = response.AccessToken;
        _user = response.User;
        _expiresAt = response.ExpiresAt;
        _loaded = true;

        await _storage.SetAsync(TokenKey, _token);
        await _storage.SetAsync(UserKey, _user);
        await _storage.SetAsync(ExpiryKey, _expiresAt);
    }

    public async Task<string?> GetTokenAsync()
    {
        await EnsureLoadedAsync();

        if (string.IsNullOrWhiteSpace(_token) || IsExpired(_expiresAt, _token))
        {
            await ClearAsync();
            return null;
        }

        return _token;
    }

    public async Task<AuthenticatedUser?> GetUserAsync()
    {
        await EnsureLoadedAsync();
        return _user;
    }

    public async Task ClearAsync()
    {
        _token = null;
        _user = null;
        _expiresAt = null;
        _loaded = true;

        try
        {
            await _storage.RemoveAsync(TokenKey);
            await _storage.RemoveAsync(UserKey);
            await _storage.RemoveAsync(ExpiryKey);
        }
        catch (JSException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private async Task EnsureLoadedAsync()
    {
        if (_loaded) return;

        for (var attempt = 0; attempt < LoadAttempts; attempt++)
        {
            try
            {
                _token = await _storage.GetAsync<string>(TokenKey);
                _user = await _storage.GetAsync<AuthenticatedUser>(UserKey);
                _expiresAt = await _storage.GetAsync<DateTimeOffset?>(ExpiryKey) ?? ReadExpiryFromToken(_token);
                _loaded = true;
                return;
            }
            catch (JSException) when (attempt < LoadAttempts - 1)
            {
                await Task.Delay(RetryDelayMilliseconds * (attempt + 1));
            }
            catch (InvalidOperationException exception) when (attempt < LoadAttempts - 1 && IsJsInteropUnavailable(exception))
            {
                await Task.Delay(RetryDelayMilliseconds * (attempt + 1));
            }
        }
    }

    private static bool IsExpired(DateTimeOffset? expiresAt, string? token)
    {
        var expiry = expiresAt ?? ReadExpiryFromToken(token);
        return expiry is null || expiry <= DateTimeOffset.UtcNow;
    }

    private static DateTimeOffset? ReadExpiryFromToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return null;

            var payload = parts[1].Replace('-', '+').Replace('_', '/');

            if (payload.Length % 4 == 2)
                payload += "==";
            else if (payload.Length % 4 == 3)
                payload += "=";

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("exp", out var expiry) || expiry.ValueKind != JsonValueKind.Number)
                return null;

            return DateTimeOffset.FromUnixTimeSeconds(expiry.GetInt64());
        }
        catch
        {
            return null;
        }
    }

    private static bool IsJsInteropUnavailable(InvalidOperationException exception)
    {
        return exception.Message.Contains("JavaScript interop", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("JSRuntime", StringComparison.OrdinalIgnoreCase);
    }
}
