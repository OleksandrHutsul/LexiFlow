namespace LexiFlow.Models.TranslationModels;

public class PhotoTranslationResult
{
    public string OriginalText { get; set; } = "";
    public string TranslatedText { get; set; } = "";
    public List<PhotoWordTranslation> Words { get; set; } = [];
    public string? TranslationError { get; set; }
}
