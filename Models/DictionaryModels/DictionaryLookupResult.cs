namespace LexiFlow.Models.DictionaryModels;

public record DictionaryLookupResult(string Query, string ResolvedQuery, DictionaryEntry? Entry, IReadOnlyList<DictionarySuggestion> Suggestions, bool IsFound);
