namespace LexiFlow.Models.SearchModels;

public record SearchSuggestion(string Word, string? PartOfSpeech = null, string? Level = null, Guid? DictionaryWordId = null, string? Summary = null,
    string? Translation = null, string? Pronunciation = null, string? Url = null);
