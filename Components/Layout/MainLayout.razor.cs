using LexiFlow.Models.SettingModels;
using LexiFlow.Services.UserSettings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Layout;

public partial class MainLayout : IDisposable
{
    [Inject] public required IUserSettingsService SettingsService { get; set; }

    private bool _drawerOpen = true;
    private bool _darkMode = true;

    private readonly MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#6d5dfc",
            Secondary = "#22c55e",
            Background = "#f7f8fc",
            Surface = "rgba(255,255,255,0.84)",
            AppbarBackground = "rgba(255,255,255,0.74)",
            DrawerBackground = "rgba(255,255,255,0.68)",
            TextPrimary = "#111827",
            TextSecondary = "#667085"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#8b7dff",
            Secondary = "#35d39d",
            Background = "#0d1020",
            Surface = "rgba(21,25,43,0.84)",
            AppbarBackground = "rgba(13,16,32,0.72)",
            DrawerBackground = "rgba(17,21,36,0.66)",
            TextPrimary = "#f7f7fb",
            TextSecondary = "#aab2c5"
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "16px" },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Inter", "Segoe UI", "sans-serif"] },
            H1 = new H1Typography { FontWeight = "800", LetterSpacing = "0" },
            H2 = new H2Typography { FontWeight = "750", LetterSpacing = "0" }
        }
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        SettingsService.Changed += OnSettingsChanged;
        ApplySettings(await SettingsService.GetAsync());
        StateHasChanged();
    }

    private async Task ToggleTheme()
    {
        _darkMode = !_darkMode;
        var settings = await SettingsService.GetAsync();
        settings.DarkMode = _darkMode;
        await SettingsService.SaveAsync(settings);
    }

    private void OnSettingsChanged(UserSettings settings)
    {
        _ = InvokeAsync(() =>
        {
            ApplySettings(settings);
            StateHasChanged();
        });
    }

    private void ApplySettings(UserSettings settings)
    {
        _darkMode = settings.DarkMode;
    }

    public void Dispose()
    {
        SettingsService.Changed -= OnSettingsChanged;
    }
}
