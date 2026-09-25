using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.SearchModels;

namespace LexiFlow.Models.TranslationModels;

public class ImageVocabularyReviewItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalWord { get; set; } = "";
    public string Word { get; set; } = "";
    public string Translation { get; set; } = "";
    public string? TranslationError { get; set; }
    public DictionaryEntry? Entry { get; set; }
    public bool IsEditing { get; set; }
    public bool IsSearching { get; set; }
    public string SearchQuery { get; set; } = "";
    public string? SearchError { get; set; }
    public List<SearchSuggestion> Suggestions { get; set; } = [];

    public DictionaryEntry ToDictionaryEntry()
    {
        return new DictionaryEntry
        {
            Word = Word.Trim(),
            Provider = Entry?.Provider ?? "",
            Origin = Entry?.Origin ?? "",
            Pronunciations = Entry?.Pronunciations.ToList() ?? [],
            Meanings = Entry?.Meanings.ToList() ?? [],
            Translations = BuildTranslations(),
            Synonyms = Entry?.Synonyms.ToList() ?? [],
            Antonyms = Entry?.Antonyms.ToList() ?? [],
            PhrasalVerbs = Entry?.PhrasalVerbs.ToList() ?? [],
            Idioms = Entry?.Idioms.ToList() ?? [],
            Collocations = Entry?.Collocations.ToList() ?? [],
            Ipa = Entry?.Ipa,
            Audio = Entry?.Audio
        };
    }

    private List<TranslationItem> BuildTranslations()
    {
        if (string.IsNullOrWhiteSpace(Translation))
            return [];

        return
        [
            new TranslationItem
            {
                Language = "uk",
                Text = Translation.Trim()
            }
        ];
    }
}
