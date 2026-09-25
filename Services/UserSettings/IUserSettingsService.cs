namespace LexiFlow.Services.UserSettings;

public interface IUserSettingsService
{
    event Action<Models.SettingModels.UserSettings>? Changed;

    Task<Models.SettingModels.UserSettings> GetAsync();
    Task SaveAsync(Models.SettingModels.UserSettings settings);
}