namespace LexiFlow.Models.DictionaryModels;

public class MeaningItem
{
    public string Definition { get; set; } = "";
    public string? CefrLevel { get; set; }
    public List<DictionaryExample> Examples { get; set; } = [];
    public List<TranslationItem> Translations { get; set; } = [];
    public List<string> Synonyms { get; set; } = [];
    public List<string> Antonyms { get; set; } = [];
}
