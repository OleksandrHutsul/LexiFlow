namespace LexiFlow.Models.VocabularyModels;

public class VocabularyListSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
    public int WordCount { get; set; }
    public bool IsOwner { get; set; }
    public string Permission { get; set; } = "Reader";
    public bool IsArchived { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
}
