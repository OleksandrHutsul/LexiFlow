using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.SearchModels;
using LexiFlow.Models.VocabularyModels;
using MudBlazor;

namespace LexiFlow.Components.Pages.Collections;

public partial class VocabularyListDetailComponent
{
    private string _searchText = "";
    private string? _searchError;
    private bool _searching;
    private List<SearchSuggestion> _suggestions = [];
    private CancellationTokenSource? _searchCts;

    private async Task HydrateWordsAsync(IEnumerable<VocabularyListWord> words)
    {
        var missingWords = words
            .Where(word => string.IsNullOrWhiteSpace(word.Translation))
            .ToList();

        if (missingWords.Count == 0) return;

        var tasks = missingWords.Select(async word =>
        {
            var result = await DictionaryApi.GetWordAsync(word.Word, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
                ApplyEntry(word, result.Value);
        });

        await Task.WhenAll(tasks);
    }

    private async Task SearchChangedAsync(string value)
    {
        _searchText = value;
        _suggestions = [];
        _searchError = null;

        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();

        if (value.Trim().Length < 2)
        {
            _searching = false;
            return;
        }

        var cancellationToken = _searchCts.Token;
        _searching = true;

        try
        {
            await Task.Delay(300, cancellationToken);

            var result = await DictionaryApi.SearchAsync(value, cancellationToken);

            if (result.IsSuccess && result.Value is not null)
                _suggestions = result.Value.ToList();
            else
                _searchError = result.Error ?? "Dictionary search failed.";
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (_searchCts?.Token == cancellationToken)
                _searching = false;
        }
    }

    private async Task SelectSuggestionAsync(SearchSuggestion suggestion)
    {
        if (_list is null || _saving) return;

        if (_list.Words.Any(word => string.Equals(word.Word, suggestion.Word, StringComparison.OrdinalIgnoreCase)))
        {
            _searchError = $"“{suggestion.Word}” is already in this list.";
            return;
        }

        _saving = true;

        try
        {
            var entryResult = await DictionaryApi.GetWordAsync(suggestion.Word, CancellationToken.None);

            if (!entryResult.IsSuccess || entryResult.Value is null)
            {
                _searchError = entryResult.Error ?? "Could not load this dictionary entry.";
                return;
            }

            var request = BuildRequest(suggestion, entryResult.Value);
            var result = await ListsApi.AddWordAsync(_list.Id, request);

            if (!result.IsSuccess || result.Value is null)
            {
                _searchError = result.Error ?? "Could not add the word.";
                return;
            }

            ApplyEntry(result.Value, entryResult.Value);

            _list.Words.Add(result.Value);
            _list.Words = _list.Words.OrderBy(word => word.Word).ToList();
            _list.WordCount = _list.Words.Count;

            _searchText = "";
            _suggestions = [];

            Snackbar.Add("Dictionary entry added.", Severity.Success);
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task RemoveWordAsync(Guid id)
    {
        if (_list is null || _saving) return;

        _saving = true;

        try
        {
            var result = await ListsApi.RemoveWordAsync(_list.Id, id);

            if (!result.IsSuccess)
            {
                Snackbar.Add(result.Error ?? "Could not remove the word.", Severity.Error);
                return;
            }

            _list.Words.RemoveAll(word => word.Id == id);
            _list.WordCount = _list.Words.Count;

            if (_cardIndex >= _list.Words.Count)
                _cardIndex = Math.Max(0, _list.Words.Count - 1);
        }
        finally
        {
            _saving = false;
        }
    }

    private static AddVocabularyWordRequest BuildRequest(SearchSuggestion suggestion, DictionaryEntry entry)
    {
        var british = entry.Pronunciations.FirstOrDefault(pronunciation => IsDialect(pronunciation.Dialect, "uk", "brit"));
        var american = entry.Pronunciations.FirstOrDefault(pronunciation => IsDialect(pronunciation.Dialect, "us", "amer"));

        var partOfSpeech = entry.Meanings.FirstOrDefault();
        var meaning = partOfSpeech?.GuideWordGroups
            .SelectMany(group => group.Meanings)
            .FirstOrDefault();

        return new AddVocabularyWordRequest
        {
            DictionaryWordId = suggestion.DictionaryWordId,
            Word = entry.Word,
            PartOfSpeech = partOfSpeech?.PartOfSpeech,
            Translation = entry.Translations.FirstOrDefault()?.Text,
            Definition = meaning?.Definition,
            Example = meaning?.Examples.FirstOrDefault()?.Text,
            BritishIpa = british?.Ipa,
            BritishAudioUrl = british?.AudioUrl,
            AmericanIpa = american?.Ipa,
            AmericanAudioUrl = american?.AudioUrl
        };
    }

    private static void ApplyEntry(VocabularyListWord word, DictionaryEntry entry)
    {
        var request = BuildRequest(new SearchSuggestion(entry.Word), entry);

        word.PartOfSpeech ??= request.PartOfSpeech;
        word.Translation ??= request.Translation;
        word.Definition ??= request.Definition;
        word.Example ??= request.Example;
        word.BritishIpa ??= request.BritishIpa;
        word.BritishAudioUrl ??= request.BritishAudioUrl;
        word.AmericanIpa ??= request.AmericanIpa;
        word.AmericanAudioUrl ??= request.AmericanAudioUrl;
    }
    
    private static bool IsDialect(string? value, params string[] terms)
    {
        return value is not null && terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
