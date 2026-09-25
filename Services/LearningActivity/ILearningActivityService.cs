namespace LexiFlow.Services.LearningActivity;

public interface ILearningActivityService
{
    Task<Models.LearningModels.LearningActivity> GetAsync();
    Task RecordPracticeAsync(int correctAnswers, int totalAnswers, TimeSpan activeTime);
}