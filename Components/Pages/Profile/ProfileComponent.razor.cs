using LexiFlow.Models.SettingModels;
using LexiFlow.Services.State;
using LexiFlow.Services.UserSettings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Pages.Profile;

public partial class ProfileComponent
{
    [Inject] public required IUserSettingsService SettingsService { get; set; }
    [Inject] public required StatisticsChangeNotifier StatisticsChanges { get; set; }
    [Inject] public required FluentValidation.IValidator<UserSettings> Validator { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }

    private readonly List<string> _errors = [];

    private UserSettings _settings = new();
    private bool _saving;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _settings = await SettingsService.GetAsync();

        StateHasChanged();
    }

    private async Task SaveAsync()
    {
        if (_saving) return;

        _saving = true;
        _errors.Clear();

        try
        {
            var result = await Validator.ValidateAsync(_settings);

            if (!result.IsValid)
            {
                _errors.AddRange(result.Errors.Select(error => error.ErrorMessage));
                return;
            }

            await SettingsService.SaveAsync(_settings);

            StatisticsChanges.NotifyChanged();
            Snackbar.Add("Preferences saved.", Severity.Success);
        }
        finally
        {
            _saving = false;
        }
    }
}
