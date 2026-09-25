namespace LexiFlow.Models.VocabularyModels;

public class VocabularyListShare
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Permission { get; set; } = "Reader";
    public DateTimeOffset SharedAt { get; set; }
}
