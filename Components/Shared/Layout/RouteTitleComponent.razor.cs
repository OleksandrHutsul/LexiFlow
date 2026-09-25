using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace LexiFlow.Components.Shared.Layout;

public partial class RouteTitleComponent : IDisposable
{
    [Inject] public required NavigationManager Navigation { get; set; }

    private string Title
    {
        get
        {
            var pageName = GetPageName();
            return pageName is null ? "LexiFlow" : $"{pageName} · LexiFlow";
        }
    }

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
    }

    private string? GetPageName()
    {
        var path = Navigation.ToBaseRelativePath(Navigation.Uri).Split('?', '#')[0].Trim('/');

        return path switch
        {
            "" => "Home",
            "dictionary" => "Dictionary",
            "photo-translate" => "Photo Translate",
            "favorites" => "Favorites",
            "lists" => "Vocabulary Lists",
            var value when value.StartsWith("lists/", StringComparison.OrdinalIgnoreCase) => "Vocabulary List",
            "learning" => "Learning",
            var value when value.StartsWith("learning/", StringComparison.OrdinalIgnoreCase) => "Learning Collection",
            "flashcards" => "Flashcards",
            "quiz" => "Quiz",
            "statistics" => "Statistics",
            "profile" => "Profile",
            "login" => "Log in",
            "register" => "Create account",
            "error" => "Error",
            _ => null
        };
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
    }
}
