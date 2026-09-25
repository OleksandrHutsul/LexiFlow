using LexiFlow.Services.LocalStorage;
using LexiFlow.Services.State;

namespace LexiFlow.Services.LearningActivity;

public class LearningActivityService : ILearningActivityService
{
    private const string ActivityKey = "lexiflow:learning-activity";

    private readonly ILocalStorageService _storage;
    private readonly StatisticsChangeNotifier _notifier;

    public LearningActivityService(ILocalStorageService storage, StatisticsChangeNotifier notifier)
    {
        _storage = storage;
        _notifier = notifier;
    }

    public async Task<Models.LearningModels.LearningActivity> GetAsync()
    {
        return await _storage.GetAsync<Models.LearningModels.LearningActivity>(ActivityKey) ?? new Models.LearningModels.LearningActivity();
    }

    public async Task RecordPracticeAsync(int correctAnswers, int totalAnswers, TimeSpan activeTime)
    {
        if (totalAnswers <= 0) return;

        var activity = await GetAsync();

        activity.CorrectAnswers += Math.Clamp(correctAnswers, 0, totalAnswers);
        activity.TotalAnswers += totalAnswers;
        activity.StudySeconds += Math.Max(0, (long)Math.Round(activeTime.TotalSeconds));

        var now = DateTimeOffset.Now;

        if (activity.ActiveDays.All(x => x.LocalDateTime.Date != now.LocalDateTime.Date))
            activity.ActiveDays.Add(now);

        await _storage.SetAsync(ActivityKey, activity);
        _notifier.NotifyChanged();
    }
}
