using LexiFlow.Models.AuthenticationModels;

namespace LexiFlow.Services.AuthApiClient;

public interface IAuthApiClient
{
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(string token, CancellationToken cancellationToken = default);
}
