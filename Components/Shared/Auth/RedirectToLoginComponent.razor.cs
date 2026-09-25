using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LexiFlow.Components.Shared.Auth;

public partial class RedirectToLoginComponent
{
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var currentPath = Navigation.ToBaseRelativePath(Navigation.Uri);

        if (authState.User.Identity?.IsAuthenticated == true)
        {
            var destination = string.IsNullOrWhiteSpace(currentPath) || IsAuthPath(currentPath) ? "/" : $"/{currentPath}";
            Navigation.NavigateTo(destination);
            return;
        }

        if (string.IsNullOrWhiteSpace(currentPath) || IsAuthPath(currentPath))
        {
            Navigation.NavigateTo("/login");
            return;
        }

        var returnUrl = Uri.EscapeDataString(currentPath);
        Navigation.NavigateTo($"/login?returnUrl={returnUrl}");
    }

    private static bool IsAuthPath(string path)
    {
        return path.StartsWith("login", StringComparison.OrdinalIgnoreCase) || path.StartsWith("register", StringComparison.OrdinalIgnoreCase);
    }
}