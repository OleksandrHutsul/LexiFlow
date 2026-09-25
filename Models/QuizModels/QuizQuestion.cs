namespace LexiFlow.Models.QuizModels;

public class QuizQuestion
{
    public string Prompt { get; set; } = "";
    public string Answer { get; set; } = "";
    public string Type { get; set; } = "Definition";
    public List<string> Choices { get; set; } = [];
}
