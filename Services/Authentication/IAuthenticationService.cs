using LexiFlow.Models.AuthenticationModels;

namespace LexiFlow.Services.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task InvalidateSessionAsync();
}
