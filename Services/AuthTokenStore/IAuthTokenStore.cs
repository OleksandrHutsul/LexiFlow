using LexiFlow.Models.AuthenticationModels;

namespace LexiFlow.Services.AuthTokenStore;

public interface IAuthTokenStore
{
    Task SaveAsync(AuthenticationResponse response);
    Task<string?> GetTokenAsync();
    Task<AuthenticatedUser?> GetUserAsync();
    Task ClearAsync();
}
