namespace LexiFlow.Models.DictionaryModels;

public class DictionaryPartOfSpeech
{
    public string PartOfSpeech { get; set; } = "";
    public List<GuideWordGroup> GuideWordGroups { get; set; } = [];
}
