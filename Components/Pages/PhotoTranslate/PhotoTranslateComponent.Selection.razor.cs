using LexiFlow.Enums.LearningEnums;
using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.VocabularyModels;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace LexiFlow.Components.Pages.PhotoTranslate;

public partial class PhotoTranslateComponent
{
    private IEnumerable<TextToken> SelectedWordTokens()
    {
        if (!HasSelection)
            return [];

        return _tokens.Where(token => token.IsWord && token.Index >= _rangeStart && token.Index <= _rangeEnd);
    }

    private async Task OnWordPointerDown(TextToken token, PointerEventArgs args)
    {
        if (!token.IsWord || args.Button != 0) return;

        _pointerSelecting = true;
        _suppressClick = false;

        if (HasSelection && IsAdjacentWord(token.Index))
        {
            var previousStart = _rangeStart!.Value;
            var previousEnd = _rangeEnd!.Value;

            SetRange(Math.Min(previousStart, token.Index), Math.Max(previousEnd, token.Index));

            _dragOrigin = token.Index <= previousStart ? previousEnd : previousStart;
        }
        else
        {
            _dragOrigin = token.Index;
            SetRange(token.Index, token.Index);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnWordPointerEnter(TextToken token)
    {
        if (!_pointerSelecting || !token.IsWord || _dragOrigin is null) return;

        SetRange(Math.Min(_dragOrigin.Value, token.Index), Math.Max(_dragOrigin.Value, token.Index));

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnRecognizedPointerMove(PointerEventArgs args)
    {
        if (!_pointerSelecting || _dragOrigin is null) return;

        int? index;

        try
        {
            index = await JS.InvokeAsync<int?>("lexiFlow.phrase.tokenIndexAt", args.ClientX, args.ClientY);
        }
        catch (JSException)
        {
            return;
        }

        if (index is null) return;

        var token = _tokens.FirstOrDefault(item => item.IsWord && item.Index == index.Value);

        if (token is null) return;

        SetRange(Math.Min(_dragOrigin.Value, token.Index), Math.Max(_dragOrigin.Value, token.Index));

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnWordPointerUp()
    {
        if (!_pointerSelecting) return;

        _pointerSelecting = false;
        _dragOrigin = null;
        _suppressClick = true;

        await LookupSelectionAsync();
    }

    private void OnWordClick()
    {
        if (_suppressClick)
            _suppressClick = false;
    }

    private bool IsTokenSelected(TextToken token)
    {
        return token.IsWord && HasSelection && token.Index >= _rangeStart && token.Index <= _rangeEnd;
    }

    private bool IsAdjacentWord(int tokenIndex)
    {
        if (!HasSelection)
            return false;

        var wordIndexes = _tokens
            .Where(token => token.IsWord)
            .Select(token => token.Index)
            .ToList();

        var startPosition = wordIndexes.IndexOf(_rangeStart!.Value);
        var endPosition = wordIndexes.IndexOf(_rangeEnd!.Value);
        var tokenPosition = wordIndexes.IndexOf(tokenIndex);

        return tokenPosition >= 0 && (tokenPosition == startPosition - 1 || tokenPosition == endPosition + 1);
    }

    private void SetRange(int start, int end)
    {
        var low = Math.Min(start, end);
        var high = Math.Max(start, end);

        var words = _tokens
            .Where(token => token.IsWord && token.Index >= low && token.Index <= high)
            .ToList();

        if (words.Count == 0)
        {
            ClearSelectionState();
            return;
        }

        var nextStart = words[0].Index;
        var nextEnd = words[^1].Index;

        if (_rangeStart == nextStart && _rangeEnd == nextEnd) return;

        CancelLookup();

        _rangeStart = nextStart;
        _rangeEnd = nextEnd;
        _phraseMiss = false;
        _selectedEntry = null;
        _lookupSuggestions = [];
        _resolvedQuery = "";
        _wordLoading = false;
        _inspectorPhrase = string.Join(' ', words.Select(token => token.Text));
    }

    private async Task LookupSelectionAsync()
    {
        var phrase = SelectedPhrase;
        var rangeStart = _rangeStart;
        var rangeEnd = _rangeEnd;

        if (string.IsNullOrWhiteSpace(phrase) || rangeStart is null || rangeEnd is null) return;

        var cancellationToken = BeginLookup();

        PrepareLookup(phrase);

        try
        {
            var lookup = await LookupDictionaryAsync(phrase, cancellationToken);

            if (!IsCurrentLookup(rangeStart, rangeEnd, phrase, cancellationToken)) return;

            var entry = lookup?.Entry;

            if (entry is null && lookup?.Suggestions is { Count: > 0 })
            {
                var suggestions = DeduplicateSuggestions(lookup.Suggestions);
                _resolvedQuery = string.IsNullOrWhiteSpace(lookup.ResolvedQuery) ? phrase : lookup.ResolvedQuery;

                if (suggestions.Count == 1)
                {
                    entry = await ResolveSuggestionEntryAsync(suggestions[0], cancellationToken);

                    if (!IsCurrentLookup(rangeStart, rangeEnd, phrase, cancellationToken)) return;
                }
                else if (suggestions.Count > 1)
                {
                    _lookupSuggestions = suggestions;
                    _wordLoading = false;
                    await InvokeAsync(StateHasChanged);
                    return;
                }
            }

            if (entry is null && SelectedWordCount == 1)
                entry = GetPhotoTranslationEntry();

            if (entry is not null)
            {
                await ApplySelectedEntryAsync(entry, cancellationToken);

                if (!IsCurrentLookup(rangeStart, rangeEnd, phrase, cancellationToken)) return;
            }
            else
            {
                _phraseMiss = true;
            }

            _wordLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<DictionaryLookupResult?> LookupDictionaryAsync(string phrase, CancellationToken cancellationToken)
    {
        var result = await DictionaryApi.LookupAsync(phrase, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
            return null;

        var lookup = result.Value;
        return lookup with { Suggestions = lookup.Suggestions ?? [] };
    }

    private static IReadOnlyList<DictionarySuggestion> DeduplicateSuggestions(IEnumerable<DictionarySuggestion> suggestions)
    {
        return suggestions
            .Where(suggestion => !string.IsNullOrWhiteSpace(suggestion.Word))
            .GroupBy(suggestion => (Word: suggestion.Word.Trim().ToLowerInvariant(), Url: suggestion.Url ?? ""))
            .Select(group => group.First())
            .ToList();
    }

    private async Task SelectSuggestionAsync(DictionarySuggestion suggestion)
    {
        if (string.IsNullOrWhiteSpace(suggestion.Word)) return;

        var phrase = SelectedPhrase;
        var rangeStart = _rangeStart;
        var rangeEnd = _rangeEnd;
        var cancellationToken = BeginLookup();

        _wordLoading = true;
        _phraseMiss = false;
        _selectedEntry = null;

        await InvokeAsync(StateHasChanged);

        try
        {
            var entry = await ResolveSuggestionEntryAsync(suggestion, cancellationToken);

            if (!IsCurrentLookup(rangeStart, rangeEnd, phrase, cancellationToken)) return;

            if (entry is not null)
            {
                await ApplySelectedEntryAsync(entry, cancellationToken);

                if (!IsCurrentLookup(rangeStart, rangeEnd, phrase, cancellationToken)) return;
            }
            else if (_lookupSuggestions.Count == 0)
            {
                _phraseMiss = true;
            }

            _wordLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<DictionaryEntry?> ResolveSuggestionEntryAsync(DictionarySuggestion suggestion, CancellationToken cancellationToken)
    {
        var result = await DictionaryApi.GetSuggestionAsync(suggestion, cancellationToken);

        if (result.IsSuccess && result.Value is not null)
            return result.Value;

        return null;
    }

    private void ReturnToSuggestions()
    {
        if (_lookupSuggestions.Count == 0) return;

        CancelLookup();

        _selectedEntry = null;
        _phraseMiss = false;
        _wordLoading = false;
        _isFavorite = false;
        _inLearning = false;
        _wordLists = [];
        _selectedListId = null;
    }

    private DictionaryEntry? GetPhotoTranslationEntry()
    {
        var token = SelectedWordTokens().FirstOrDefault();

        if (token is null)
            return null;

        var translation = _reviewItems
            .FirstOrDefault(item => item.OriginalWord.Equals(token.Text, StringComparison.OrdinalIgnoreCase) ||
            item.Word.Equals(token.Text, StringComparison.OrdinalIgnoreCase))?.Translation;

        if (string.IsNullOrWhiteSpace(translation))
            return null;

        return new DictionaryEntry
        {
            Word = token.Text,
            Translations =
            [
                new TranslationItem
                {
                    Language = "uk",
                    Text = translation
                }
            ]
        };
    }

    private void PrepareLookup(string phrase)
    {
        _inspectorPhrase = phrase;
        _wordLoading = true;
        _selectedEntry = null;
        _lookupSuggestions = [];
        _resolvedQuery = "";
        _phraseMiss = false;
        _isFavorite = false;
        _inLearning = false;
        _wordLists = [];
        _selectedListId = null;
    }

    private async Task ApplySelectedEntryAsync(DictionaryEntry entry, CancellationToken cancellationToken)
    {
        var favoriteTask = Favorites.IsFavoriteAsync(entry.Word);
        var learningTask = Learning.GetWordsAsync();

        await Task.WhenAll(favoriteTask, learningTask);
        cancellationToken.ThrowIfCancellationRequested();

        var isFavorite = await favoriteTask;
        var inLearning = (await learningTask).Any(word => word.Word.Equals(entry.Word, StringComparison.OrdinalIgnoreCase));

        var memberships = await Task.WhenAll(_ownedLists.Select(async list =>
        {
            var detail = await ListsApi.GetListAsync(list.Id);

            return detail.IsSuccess && detail.Value?.Words.Any(word => word.Word.Equals(entry.Word, StringComparison.OrdinalIgnoreCase)) == true
                    ? list.Name
                    : null;
        }));

        cancellationToken.ThrowIfCancellationRequested();

        _selectedEntry = entry;
        _isFavorite = isFavorite;
        _inLearning = inLearning;
        _wordLists = memberships
            .Where(name => name is not null)
            .Cast<string>()
            .ToList();
    }

    private async Task ShrinkSelectionAsync()
    {
        var words = SelectedWordTokens().ToList();

        if (words.Count <= 1)
        {
            CloseInspector();
            return;
        }

        SetRange(words[0].Index, words[^2].Index);

        await LookupSelectionAsync();
    }

    private void ClearSelectionState()
    {
        CancelLookup();

        _rangeStart = null;
        _rangeEnd = null;
        _selectedEntry = null;
        _lookupSuggestions = [];
        _resolvedQuery = "";
        _phraseMiss = false;
        _pointerSelecting = false;
        _dragOrigin = null;
    }

    private CancellationToken BeginLookup()
    {
        CancelLookup();
        _lookupCts = new CancellationTokenSource();
        return _lookupCts.Token;
    }

    private void CancelLookup()
    {
        if (_lookupCts is null) return;

        _lookupCts.Cancel();
        _lookupCts.Dispose();
        _lookupCts = null;
    }

    private bool IsCurrentLookup(int? rangeStart, int? rangeEnd, string phrase, CancellationToken cancellationToken)
    {
        return !cancellationToken.IsCancellationRequested
            && _rangeStart == rangeStart
            && _rangeEnd == rangeEnd
            && string.Equals(SelectedPhrase, phrase, StringComparison.Ordinal);
    }

    private async Task AddFavoriteAsync()
    {
        if (_selectedEntry is null) return;

        await Favorites.ToggleAsync(_selectedEntry);

        _isFavorite = true;

        Snackbar.Add("Added to Favorites.", Severity.Success);
    }

    private async Task AddLearningAsync()
    {
        if (_selectedEntry is null) return;

        var result = await Learning.AddWordAsync(_selectedEntry);

        if (result.Status == AddLearningWordStatus.CollectionRequired)
        {
            var collectionId = await ChooseLearningCollectionAsync();

            if (collectionId is null) return;

            result = await Learning.AddWordAsync(_selectedEntry, collectionId);

            await ReloadLearningCollectionsAsync();
        }

        _inLearning = true;

        Snackbar.Add(result.Status == AddLearningWordStatus.Added
                ? $"Added to {result.CollectionName}."
                : $"Already in {result.CollectionName}.",
            result.Status == AddLearningWordStatus.Added ? Severity.Success : Severity.Info);
    }

    private async Task AddToListAsync()
    {
        if (_selectedEntry is null || _selectedListId is null || _addingToList) return;

        _addingToList = true;

        try
        {
            var entry = _selectedEntry;
            var partOfSpeech = entry.Meanings.FirstOrDefault();
            var meaning = partOfSpeech?.GuideWordGroups
                .SelectMany(group => group.Meanings)
                .FirstOrDefault();

            var result = await ListsApi.AddWordAsync(_selectedListId.Value, new()
            {
                Word = entry.Word,
                PartOfSpeech = partOfSpeech?.PartOfSpeech,
                Translation = entry.Translations.FirstOrDefault()?.Text,
                Definition = meaning?.Definition,
                Example = meaning?.Examples.FirstOrDefault()?.Text
            });

            if (!result.IsSuccess)
            {
                Snackbar.Add(result.Error ?? "Could not add to list.", Severity.Error);
                return;
            }

            var list = _ownedLists.First(item => item.Id == _selectedListId.Value);

            _wordLists.Add(list.Name);
            _selectedListId = null;

            Snackbar.Add("Added to list.", Severity.Success);
        }
        finally
        {
            _addingToList = false;
        }
    }

    private async Task PlayAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        try
        {
            await JS.InvokeVoidAsync("lexiFlow.audio.playExclusive", url);
        }
        catch (JSException)
        {
            Snackbar.Add("Audio could not be played.", Severity.Warning);
        }
    }

    private void OpenWord()
    {
        if (_selectedEntry is null) return;

        Navigation.NavigateTo($"/dictionary?word={Uri.EscapeDataString(_selectedEntry.Word)}");
    }

    private void CloseInspector()
    {
        ClearSelectionState();

        _inspectorPhrase = "";
        _resolvedQuery = "";
        _wordLoading = false;
        _isFavorite = false;
        _inLearning = false;
        _wordLists = [];
        _selectedListId = null;
    }
}
