using LexiFlow.Services.LocalStorage;

namespace LexiFlow.Services.UserSettings;

public class UserSettingsService : IUserSettingsService
{
    private const string StorageKey = "lexiflow:settings";

    private readonly ILocalStorageService _storage;
    private Models.SettingModels.UserSettings? _cache;

    public UserSettingsService(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public event Action<Models.SettingModels.UserSettings>? Changed;

    public async Task<Models.SettingModels.UserSettings> GetAsync()
    {
        if (_cache is not null)
            return Clone(_cache);

        var settings = await _storage.GetAsync<Models.SettingModels.UserSettings>(StorageKey);
        _cache = Normalize(settings ?? new Models.SettingModels.UserSettings());

        return Clone(_cache);
    }

    public async Task SaveAsync(Models.SettingModels.UserSettings settings)
    {
        _cache = Normalize(Clone(settings));

        await _storage.SetAsync(StorageKey, _cache);

        Changed?.Invoke(Clone(_cache));
    }

    private static LexiFlow.Models.SettingModels.UserSettings Normalize(Models.SettingModels.UserSettings settings)
    {
        if (settings.DefaultPronunciation is not ("US" or "UK"))
            settings.DefaultPronunciation = "US";

        if (settings.DailyGoal is < 1 or > 100)
            settings.DailyGoal = 12;

        return settings;
    }

    private static Models.SettingModels.UserSettings Clone(Models.SettingModels.UserSettings settings)
    {
        return new Models.SettingModels.UserSettings
        {
            DarkMode = settings.DarkMode,
            DailyGoal = settings.DailyGoal,
            DefaultPronunciation = settings.DefaultPronunciation,
            AutoPlayPronunciation = settings.AutoPlayPronunciation
        };
    }
}
