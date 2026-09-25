namespace LexiFlow.Models.PronunciationModels;

public class PronunciationVariant
{
    public string Dialect { get; set; } = "";
    public string? Ipa { get; set; }
    public string? AudioUrl { get; set; }
}
