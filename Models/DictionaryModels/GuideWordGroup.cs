namespace LexiFlow.Models.DictionaryModels;

public class GuideWordGroup
{
    public string? GuideWord { get; set; }
    public List<MeaningItem> Meanings { get; set; } = [];
}
