using LexiFlow.Enums.LearningEnums;
using LexiFlow.Models.LearningModels;
using LexiFlow.Services.Favorites;
using LexiFlow.Services.Learning;
using LexiFlow.Services.LearningActivity;
using LexiFlow.Services.UserSettings;

namespace LexiFlow.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    private readonly ILearningService _learningService;
    private readonly IFavoritesService _favoritesService;
    private readonly IUserSettingsService _settingsService;
    private readonly ILearningActivityService _activityService;

    public StatisticsService(ILearningService learningService, IFavoritesService favoritesService, IUserSettingsService settingsService, ILearningActivityService activityService)
    {
        _learningService = learningService;
        _favoritesService = favoritesService;
        _settingsService = settingsService;
        _activityService = activityService;
    }

    public async Task<LearningStats> GetAsync()
    {
        var wordsTask = _learningService.GetWordsAsync();
        var favoritesTask = _favoritesService.GetAsync();
        var settingsTask = _settingsService.GetAsync();
        var activityTask = _activityService.GetAsync();

        await Task.WhenAll(wordsTask, favoritesTask, settingsTask, activityTask);

        var words = await wordsTask;
        var favorites = await favoritesTask;
        var settings = await settingsTask;
        var activity = await activityTask;

        var uniqueWords = words
            .GroupBy(word => word.Word, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var today = DateOnly.FromDateTime(DateTime.Now);

        var activeDays = words
            .SelectMany(word => new DateTimeOffset?[] { word.AddedAt, word.LastReviewedAt })
            .Where(value => value.HasValue)
            .Select(value => DateOnly.FromDateTime(value!.Value.LocalDateTime))
            .Concat(activity.ActiveDays.Select(value => DateOnly.FromDateTime(value.LocalDateTime)))
            .ToHashSet();

        var masteredWords = uniqueWords.Count(group => group.Any(word => word.State == LearningState.Mastered));
        var dailyProgress = uniqueWords.Count(group => group.Any(word => DateOnly.FromDateTime(word.AddedAt.LocalDateTime) == today));

        var difficultWords = uniqueWords
            .Select(group => group.FirstOrDefault(word => word.State is LearningState.New or LearningState.Learning))
            .Where(word => word is not null)
            .Cast<LearningWord>()
            .Take(8)
            .ToList();

        var weeklyActivity = Enumerable.Range(0, 7)
            .Select(offset => activeDays.Contains(today.AddDays(offset - 6)) ? 100 : 0)
            .ToList();

        return new LearningStats(LearnedWords: uniqueWords.Count, MasteredWords: masteredWords, FavoriteWords: favorites.Count, Streak: CalculateStreak(activeDays, today),
            StudySeconds: activity.StudySeconds, Accuracy: activity.TotalAnswers == 0 ? 0 : (double)activity.CorrectAnswers / activity.TotalAnswers,
            DailyGoal: settings.DailyGoal, DailyProgress: dailyProgress, DifficultWords: difficultWords, WeeklyActivity: weeklyActivity);
    }

    private static int CalculateStreak(HashSet<DateOnly> activeDays, DateOnly today)
    {
        var cursor = activeDays.Contains(today) ? today : today.AddDays(-1);
        var streak = 0;

        while (activeDays.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }
}
