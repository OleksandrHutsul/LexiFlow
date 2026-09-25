namespace LexiFlow.Models.VocabularyModels;

public class VocabularyListWord
{
    public Guid Id { get; set; }
    public Guid? DictionaryWordId { get; set; }
    public string Word { get; set; } = "";
    public string? PartOfSpeech { get; set; }
    public string? Translation { get; set; }
    public string? Definition { get; set; }
    public string? Example { get; set; }
    public string? BritishIpa { get; set; }
    public string? BritishAudioUrl { get; set; }
    public string? AmericanIpa { get; set; }
    public string? AmericanAudioUrl { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}
