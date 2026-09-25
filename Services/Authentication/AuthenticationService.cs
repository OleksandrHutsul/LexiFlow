using LexiFlow.Models.AuthenticationModels;
using LexiFlow.Services.AuthApiClient;
using LexiFlow.Services.AuthTokenStore;
using LexiFlow.Services.State;

namespace LexiFlow.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IAuthTokenStore _tokenStore;
    private readonly LexiFlowAuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationService(IAuthApiClient authApiClient, IAuthTokenStore tokenStore, LexiFlowAuthenticationStateProvider authenticationStateProvider)
    {
        _authApiClient = authApiClient;
        _tokenStore = tokenStore;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authApiClient.RegisterAsync(request, cancellationToken);

        if (result is { IsSuccess: true, Response: not null })
            await CompleteSignInAsync(result.Response);

        return result;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authApiClient.LoginAsync(request, cancellationToken);

        if (result is { IsSuccess: true, Response: not null })
            await CompleteSignInAsync(result.Response);

        return result;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var token = await _tokenStore.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
            await _authApiClient.LogoutAsync(token, cancellationToken);

        await InvalidateSessionAsync();
    }

    public async Task InvalidateSessionAsync()
    {
        await _authenticationStateProvider.InvalidateAuthenticationAsync();
    }

    private async Task CompleteSignInAsync(AuthenticationResponse response)
    {
        await _tokenStore.SaveAsync(response);
        _authenticationStateProvider.NotifySignedIn(response.User);
    }
}
