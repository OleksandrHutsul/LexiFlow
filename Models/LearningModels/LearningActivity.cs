namespace LexiFlow.Models.LearningModels;

public class LearningActivity
{
    public int CorrectAnswers { get; set; }
    public int TotalAnswers { get; set; }
    public long StudySeconds { get; set; }
    public List<DateTimeOffset> ActiveDays { get; set; } = [];
}
