using LexiFlow.Configuration;
using LexiFlow.Models.AuthenticationModels;
using LexiFlow.Services.Http;

namespace LexiFlow.Services.AuthApiClient;

public class AuthApiClient : IAuthApiClient
{
    private readonly IHttpService _httpService;

    public AuthApiClient(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        return await AuthenticateAsync("/api/auth/register", request, cancellationToken);
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return await AuthenticateAsync("/api/auth/login", request, cancellationToken);
    }

    public async Task LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        await _httpService.SendNoContentAsync(HttpMethod.Post, "/api/auth/logout", options: new ApiRequestOptions
        {
            BearerToken = token,
            ErrorMessage = "Logout failed."
        }, cancellationToken: cancellationToken);
    }
    
    private async Task<AuthenticationResult> AuthenticateAsync(string endpoint, object request, CancellationToken cancellationToken)
    {
        var result = await _httpService.PostAsync<AuthenticationResponse>(endpoint, request, new ApiRequestOptions
        {
            ErrorMessage = "Authentication failed. Please try again."
        }, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
            return AuthenticationResult.Failure(result.Error ?? "Authentication failed. Please try again.");

        return AuthenticationResult.Success(result.Value);
    }
}
