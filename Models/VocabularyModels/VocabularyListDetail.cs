namespace LexiFlow.Models.VocabularyModels;

public class VocabularyListDetail : VocabularyListSummary
{
    public List<VocabularyListWord> Words { get; set; } = [];
    public List<VocabularyListShare> Shares { get; set; } = [];
}
