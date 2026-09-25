using LexiFlow.Services.Authentication;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Shared.Auth;

public partial class LoginStatusComponent
{
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }

    private async Task LogoutAsync()
    {
        await AuthenticationService.LogoutAsync();
        Navigation.NavigateTo("/");
    }
}