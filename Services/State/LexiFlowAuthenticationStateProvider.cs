using System.Security.Claims;
using LexiFlow.Models.AuthenticationModels;
using LexiFlow.Services.AuthTokenStore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace LexiFlow.Services.State;

public class LexiFlowAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string AuthenticationType = "LexiFlowJwt";
    private const string AdminRole = "Admin";

    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    private readonly IAuthTokenStore _tokenStore;

    public LexiFlowAuthenticationStateProvider(IAuthTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenStore.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
                return CreateAnonymousState();

            var user = await _tokenStore.GetUserAsync();

            if (user is null)
            {
                await _tokenStore.ClearAsync();
                return CreateAnonymousState();
            }

            return new AuthenticationState(CreatePrincipal(user));
        }
        catch (JSException)
        {
            return CreateAnonymousState();
        }
        catch (InvalidOperationException)
        {
            return CreateAnonymousState();
        }
    }

    public void NotifySignedIn(AuthenticatedUser user)
    {
        var state = new AuthenticationState(CreatePrincipal(user));
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public void NotifySignedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(CreateAnonymousState()));
    }

    public async Task InvalidateAuthenticationAsync()
    {
        await _tokenStore.ClearAsync();
        NotifySignedOut();
    }

    private static AuthenticationState CreateAnonymousState()
    {
        return new AuthenticationState(Anonymous);
    }

    private static ClaimsPrincipal CreatePrincipal(AuthenticatedUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email)
        };

        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, AdminRole));
            claims.Add(new Claim("role", AdminRole));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationType));
    }
}
