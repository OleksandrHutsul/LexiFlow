namespace LexiFlow.Models.VocabularyModels;

public class ShareVocabularyListRequest
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string Permission { get; set; } = "Reader";
}
