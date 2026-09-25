namespace LexiFlow.Models.LearningModels;

public class LearningCollection
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "Core";
    public string Accent { get; set; } = "#5b7cfa";
    public bool IsDefault { get; set; }
    public List<LearningWord> Words { get; set; } = [];
}
