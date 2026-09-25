using LexiFlow.Services.Authentication;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Shared.Auth;

public partial class UserMenuComponent
{
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }

    private static string GetInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "U";

        return string.Concat(name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));
    }

    private async Task LogoutAsync()
    {
        await AuthenticationService.LogoutAsync();
        Navigation.NavigateTo("/");
    }
}
