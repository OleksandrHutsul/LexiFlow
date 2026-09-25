using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LexiFlow.Configuration;
using LexiFlow.Models;
using LexiFlow.Services.AuthTokenStore;
using LexiFlow.Services.State;

namespace LexiFlow.Services.Http;

public class HttpService : IHttpService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IAuthTokenStore _tokenStore;
    private readonly LexiFlowAuthenticationStateProvider _authenticationStateProvider;

    public HttpService(HttpClient httpClient, IAuthTokenStore tokenStore, LexiFlowAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url, ApiRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SendAsync<T>(HttpMethod.Get, url, null, options, cancellationToken);
    }

    public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null, ApiRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SendAsync<T>(HttpMethod.Post, url, body, options, cancellationToken);
    }

    public Task<ApiResult<T>> PutAsync<T>(string url, object? body = null, ApiRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SendAsync<T>(HttpMethod.Put, url, body, options, cancellationToken);
    }

    public Task<ApiResult<T>> PatchAsync<T>(string url, object? body = null, ApiRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SendAsync<T>(HttpMethod.Patch, url, body, options, cancellationToken);
    }

    public async Task<ApiResult<bool>> SendNoContentAsync(HttpMethod method, string url, object? body = null, ApiRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateRequest(method, url, body);
            await AddAuthorizationAsync(request, options);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            await HandleUnauthorizedAsync(response, options);

            if (response.IsSuccessStatusCode)
                return ApiResult<bool>.Success(true);

            var error = await ReadErrorAsync(response, options?.ErrorMessage, cancellationToken);
            return ApiResult<bool>.Failure(error);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return ApiResult<bool>.Failure(options?.ErrorMessage ?? "The service is temporarily unavailable.");
        }
    }

    public async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request, ApiRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await AddAuthorizationAsync(request, options);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            await HandleUnauthorizedAsync(response, options);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, options?.ErrorMessage, cancellationToken);
                return ApiResult<T>.Failure(error);
            }

            var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);

            return value is null
                ? ApiResult<T>.Failure("The service returned an empty response.")
                : ApiResult<T>.Success(value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            return ApiResult<T>.Failure($"Failed to deserialize the response: {exception.Message}");
        }
        catch (Exception exception)
        {
            return ApiResult<T>.Failure(exception.Message);
        }
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body, ApiRequestOptions? options, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, url, body);
        return await SendAsync<T>(request, options, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, object? body)
    {
        var request = new HttpRequestMessage(method, url);

        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        return request;
    }

    private async Task AddAuthorizationAsync(HttpRequestMessage request, ApiRequestOptions? options)
    {
        if (!string.IsNullOrWhiteSpace(options?.BearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.BearerToken);
            return;
        }

        if (options?.RequiresAuthentication != true) return;

        var token = await _tokenStore.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task HandleUnauthorizedAsync(HttpResponseMessage response, ApiRequestOptions? options)
    {
        if (response.StatusCode != HttpStatusCode.Unauthorized || options?.InvalidateAuthenticationOnUnauthorized != true) return;

        await _authenticationStateProvider.InvalidateAuthenticationAsync();
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, string? fallback, CancellationToken cancellationToken)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(content))
                return fallback ?? $"The request failed with status {(int)response.StatusCode}.";

            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = errors.EnumerateObject()
                    .SelectMany(property => property.Value.ValueKind == JsonValueKind.Array
                        ? property.Value.EnumerateArray().Select(value => value.GetString())
                        : [property.Value.GetString()])
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .Select(message => message!)
                    .ToList();

                if (messages.Count > 0)
                    return string.Join(" ", messages);
            }

            if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                return message.GetString() ?? fallback ?? "The request could not be completed.";

            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                return detail.GetString() ?? fallback ?? "The request could not be completed.";

            if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                return title.GetString() ?? fallback ?? "The request could not be completed.";
        }
        catch
        {
        }

        return fallback ?? $"The request failed with status {(int)response.StatusCode}.";
    }
}
