using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.LearningModels;
using LexiFlow.Services.Favorites;
using LexiFlow.Services.Learning;
using LexiFlow.Services.State;
using LexiFlow.Services.Statistics;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Pages;

public partial class HomeComponent : IDisposable
{
    [Inject] public required IStatisticsService StatsService { get; set; }
    [Inject] public required IFavoritesService FavoritesService { get; set; }
    [Inject] public required ILearningService LearningService { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required StatisticsChangeNotifier StatisticsChanges { get; set; }

    private LearningStats _stats = new(0, 0, 0, 0, 0, 0, 1, 0, [], []);
    private List<DictionaryEntry> _favorites = [];
    private List<LearningWord> _learning = [];
    private bool _loading = true;
    private bool _refreshing;
    private string? _loadError;

    private double GoalPercent => _stats.DailyGoal == 0 ? 0 : Math.Min(100, (double)_stats.DailyProgress / _stats.DailyGoal * 100);
    private string StudyTimeValue => _loading ? "–" : FormatStudyTime(_stats.StudySeconds);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        StatisticsChanges.Changed += OnStatisticsChanged;

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_refreshing) return;

        _refreshing = true;
        _loadError = null;

        try
        {
            var statsTask = StatsService.GetAsync();
            var favoritesTask = FavoritesService.GetAsync();
            var learningTask = LearningService.GetWordsAsync();

            await Task.WhenAll(statsTask, favoritesTask, learningTask);

            _stats = await statsTask;
            _favorites = await favoritesTask;
            _learning = await learningTask;
        }
        catch
        {
            _loadError = "Your statistics could not be loaded. Try again in a moment.";
        }
        finally
        {
            _loading = false;
            _refreshing = false;

            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnStatisticsChanged()
    {
        _ = InvokeAsync(RefreshAsync);
    }

    private string StatValue(int value)
    {
        return _loading ? "–" : value.ToString();
    }

    private static string FormatStudyTime(long totalSeconds)
    {
        if (totalSeconds <= 0)
            return "0m";

        if (totalSeconds < 60)
            return "<1m";

        var duration = TimeSpan.FromSeconds(totalSeconds);

        return duration.TotalHours >= 1
            ? $"{(int)duration.TotalHours}h {duration.Minutes}m"
            : $"{duration.Minutes}m";
    }

    private void OpenFavorite(string word)
    {
        Navigation.NavigateTo($"/dictionary?word={Uri.EscapeDataString(word)}");
    }

    public void Dispose()
    {
        StatisticsChanges.Changed -= OnStatisticsChanged;
    }
}
