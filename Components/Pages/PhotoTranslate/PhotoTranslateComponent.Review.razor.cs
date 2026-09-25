using LexiFlow.Components.Shared.Collections;
using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.SearchModels;
using LexiFlow.Models.TranslationModels;
using MudBlazor;

namespace LexiFlow.Components.Pages.PhotoTranslate;

public partial class PhotoTranslateComponent
{
    private static List<ImageVocabularyReviewItem> BuildReviewItems(PhotoTranslationResult result)
    {
        return result.Words
            .Where(item => !string.IsNullOrWhiteSpace(item.Word))
            .DistinctBy(item => item.Word, StringComparer.OrdinalIgnoreCase)
            .Select(item => new ImageVocabularyReviewItem
            {
                OriginalWord = item.Word.Trim(),
                Word = item.Word.Trim(),
                Translation = item.Translation?.Trim() ?? "",
                TranslationError = item.Error,
                SearchQuery = item.Word.Trim()
            })
            .ToList();
    }

    private async Task<DictionaryEntry?> GetDictionaryEntryAsync(string word)
    {
        var result = await DictionaryApi.GetWordAsync(word, CancellationToken.None);

        return result.IsSuccess ? result.Value : null;
    }

    private void BeginEditing(ImageVocabularyReviewItem item)
    {
        foreach (var reviewItem in _reviewItems)
            reviewItem.IsEditing = false;

        item.IsEditing = true;
        item.SearchQuery = item.Word;
        item.Suggestions = [];
        item.SearchError = null;
    }

    private void FinishEditing(ImageVocabularyReviewItem item)
    {
        item.IsEditing = false;
    }

    private void RemoveReviewItem(Guid id)
    {
        _reviewItems.RemoveAll(item => item.Id == id);
    }

    private async Task SearchReviewWordAsync(ImageVocabularyReviewItem item, string value)
    {
        item.SearchQuery = value;
        item.Suggestions = [];
        item.SearchError = null;

        _reviewSearchCts?.Cancel();
        _reviewSearchCts?.Dispose();
        _reviewSearchCts = new CancellationTokenSource();

        if (value.Trim().Length < 2)
        {
            item.IsSearching = false;
            return;
        }

        item.IsSearching = true;

        try
        {
            await Task.Delay(280, _reviewSearchCts.Token);

            var result = await DictionaryApi.SearchAsync(value, _reviewSearchCts.Token);

            if (result.IsSuccess && result.Value is not null)
                item.Suggestions = result.Value.Take(6).ToList();
            else
                item.SearchError = result.Error ?? "Dictionary search failed.";
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            item.IsSearching = false;
        }
    }

    private async Task SelectReviewSuggestionAsync(ImageVocabularyReviewItem item, SearchSuggestion suggestion)
    {
        item.IsSearching = true;

        try
        {
            var entry = await GetDictionaryEntryAsync(suggestion.Word);

            if (entry is null)
            {
                item.SearchError = "This dictionary entry could not be loaded.";
                return;
            }

            item.Entry = entry;
            item.Word = entry.Word;
            item.Translation = entry.Translations.FirstOrDefault()?.Text ?? suggestion.Translation ?? item.Translation;
            item.TranslationError = string.IsNullOrWhiteSpace(item.Translation)
                ? "Ukrainian translation unavailable."
                : null;
            item.SearchQuery = entry.Word;
            item.Suggestions = [];
            item.SearchError = null;
        }
        finally
        {
            item.IsSearching = false;
        }
    }

    private async Task ConfirmImportAsync()
    {
        if (_reviewItems.Count == 0 || _importing) return;

        var collectionId = _selectedLearningCollectionId ?? await ChooseLearningCollectionAsync();

        if (collectionId is null) return;

        _selectedLearningCollectionId = collectionId;
        _importing = true;

        try
        {
            var entries = _reviewItems.Select(item => item.ToDictionaryEntry());
            var result = await Learning.AddWordsAsync(entries, collectionId.Value);

            _importComplete = true;

            Snackbar.Add(
                result.AlreadyExists == 0
                    ? $"Added {result.Added} words to {result.CollectionName}."
                    : $"Added {result.Added} words to {result.CollectionName}; skipped {result.AlreadyExists} duplicates.",
                Severity.Success);
        }
        catch (InvalidOperationException exception)
        {
            Snackbar.Add(exception.Message, Severity.Error);
        }
        finally
        {
            _importing = false;
        }
    }

    private async Task<Guid?> ChooseLearningCollectionAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await Dialogs.ShowAsync<CollectionPickerDialogComponent>("Choose a learning collection", options);
        var result = await dialog.Result;

        return result is { Canceled: false, Data: Guid id } ? id : null;
    }

    private async Task CreateLearningCollectionAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await Dialogs.ShowAsync<CollectionEditorDialogComponent>("Create collection", options);
        var result = await dialog.Result;

        if (result is not { Canceled: false, Data: Guid id })
            return;

        await ReloadLearningCollectionsAsync();

        _selectedLearningCollectionId = id;
    }

    private async Task ReloadLearningCollectionsAsync()
    {
        _learningCollections = await Learning.GetCollectionsAsync();

        _selectedLearningCollectionId ??= _learningCollections
            .SingleOrDefault(collection => collection.IsDefault)
            ?.Id;
    }
}
