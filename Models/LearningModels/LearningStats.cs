namespace LexiFlow.Models.LearningModels;

public record LearningStats(int LearnedWords, int MasteredWords, int FavoriteWords, int Streak, long StudySeconds, double Accuracy, int DailyGoal, int DailyProgress, 
    List<LearningWord> DifficultWords, List<int> WeeklyActivity);
