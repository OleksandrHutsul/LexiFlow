using LexiFlow.Enums.LearningEnums;

namespace LexiFlow.Models.LearningModels;

public class LearningWord
{
    public Guid Id { get; set; }
    public string Word { get; set; } = "";
    public string CollectionName { get; set; } = "Core";
    public LearningState State { get; set; } = LearningState.New;
    public string? Level { get; set; }
    public string? Definition { get; set; }
    public string? Translation { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastReviewedAt { get; set; }
    public int EasyCount { get; set; }
    public int HardCount { get; set; }
    public int AgainCount { get; set; }
}
