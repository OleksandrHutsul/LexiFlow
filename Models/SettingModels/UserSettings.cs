namespace LexiFlow.Models.SettingModels;

public class UserSettings
{
    public bool DarkMode { get; set; } = true;
    public int DailyGoal { get; set; } = 12;
    public string DefaultPronunciation { get; set; } = "US";
    public bool AutoPlayPronunciation { get; set; } = false;
}
