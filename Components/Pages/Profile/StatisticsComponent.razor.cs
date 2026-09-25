using LexiFlow.Models.LearningModels;
using LexiFlow.Services.State;
using LexiFlow.Services.Statistics;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Pages.Profile;

public partial class StatisticsComponent : IDisposable
{
    [Inject] public required IStatisticsService StatsService { get; set; }
    [Inject] public required StatisticsChangeNotifier StatisticsChanges { get; set; }

    private LearningStats _stats = new(0, 0, 0, 0, 0, 0, 1, 0, [], []);
    private bool _loading = true;

    private IEnumerable<(string Label, int Value)> Weekly => _stats.WeeklyActivity.Select((value, index) =>
                                                                          (DateTime.Today.AddDays(index - 6).ToString("ddd")[..1], value));
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        StatisticsChanges.Changed += OnStatisticsChanged;

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        _stats = await StatsService.GetAsync();
        _loading = false;

        await InvokeAsync(StateHasChanged);
    }

    private void OnStatisticsChanged()
    {
        _ = InvokeAsync(RefreshAsync);
    }

    private string FormatValue(int value)
    {
        return _loading ? "–" : value.ToString();
    }

    public void Dispose()
    {
        StatisticsChanges.Changed -= OnStatisticsChanged;
    }
}
