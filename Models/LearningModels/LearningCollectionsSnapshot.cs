namespace LexiFlow.Models.LearningModels;

public class LearningCollectionsSnapshot
{
    public bool Initialized { get; set; }
    public List<LearningCollection> Collections { get; set; } = [];
}
