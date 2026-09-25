namespace LexiFlow.Models.QuizModels;

public class SaveListTestResultRequest
{
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;
}
