using LexiFlow.Components.Shared.Collections;
using LexiFlow.Enums.LearningEnums;
using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.PronunciationModels;
using LexiFlow.Models.SettingModels;
using LexiFlow.Services.Favorites;
using LexiFlow.Services.Learning;
using LexiFlow.Services.UserSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace LexiFlow.Components.Shared.Dictionary;

public partial class WordDetailComponent
{
    [Inject] public required IFavoritesService Favorites { get; set; }
    [Inject] public required ILearningService Learning { get; set; }
    [Inject] public required IUserSettingsService SettingsService { get; set; }
    [Inject] public required IJSRuntime JS { get; set; }
    [Inject] public required IDialogService Dialogs { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }

    [Parameter, EditorRequired] public DictionaryEntry? Entry { get; set; }

    private bool _isFavorite;
    private bool _addingToLearning;
    private string? _defaultCollectionName;
    private UserSettings _settings = new();
    private string? _autoPlayedWord;
    private string? _statusWord;

    private IEnumerable<PronunciationVariant> OrderedPronunciations
    {
        get
        {
            if (Entry is null) return [];

            return Entry.Pronunciations
                .OrderByDescending(item => IsPreferredDialect(item.Dialect))
                .ThenBy(item => item.Dialect, StringComparer.OrdinalIgnoreCase);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        _settings = await SettingsService.GetAsync();

        if (Entry is null || !string.Equals(_autoPlayedWord, Entry.Word, StringComparison.OrdinalIgnoreCase))
            _autoPlayedWord = null;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Entry is null) return;

        if (!string.Equals(_statusWord, Entry.Word, StringComparison.OrdinalIgnoreCase))
        {
            _statusWord = Entry.Word;
            _isFavorite = await Favorites.IsFavoriteAsync(Entry.Word);
            _defaultCollectionName ??= (await Learning.GetDefaultCollectionAsync())?.Name;

            StateHasChanged();
        }

        await AutoPlayAsync();
    }

    private async Task AutoPlayAsync()
    {
        if (!_settings.AutoPlayPronunciation || Entry is null) return;
        if (string.Equals(_autoPlayedWord, Entry.Word, StringComparison.OrdinalIgnoreCase)) return;

        _autoPlayedWord = Entry.Word;

        var pronunciation = OrderedPronunciations.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.AudioUrl));
        if (pronunciation?.AudioUrl is not null)
            await PlayAudioAsync(pronunciation.AudioUrl);
    }

    private bool IsPreferredDialect(string? dialect)
    {
        if (string.IsNullOrWhiteSpace(dialect)) return false;

        return _settings.DefaultPronunciation switch
        {
            "UK" => ContainsAny(dialect, "uk", "brit"),
            _ => ContainsAny(dialect, "us", "amer")
        };
    }

    private static bool ContainsAny(string value, params string[] tokens)
    {
        return tokens.Any(token => value.Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    private async Task ToggleFavoriteAsync()
    {
        if (Entry is null) return;

        await Favorites.ToggleAsync(Entry);
        _isFavorite = !_isFavorite;
    }

    private async Task AddToLearningAsync()
    {
        if (Entry is null || _addingToLearning) return;

        _addingToLearning = true;

        try
        {
            var result = await Learning.AddWordAsync(Entry);

            if (result.Status == AddLearningWordStatus.CollectionRequired)
            {
                var dialog = await Dialogs.ShowAsync<CollectionPickerDialogComponent>("Choose a learning collection", new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true });
                var selection = await dialog.Result;

                if (selection is not { Canceled: false, Data: Guid collectionId }) return;

                result = await Learning.AddWordAsync(Entry, collectionId);
                _defaultCollectionName = (await Learning.GetDefaultCollectionAsync())?.Name;
            }

            var message = result.Status == AddLearningWordStatus.Added
                ? $"Added to {result.CollectionName}."
                : $"Already in {result.CollectionName}.";

            var severity = result.Status == AddLearningWordStatus.Added ? Severity.Success : Severity.Info;

            Snackbar.Add(message, severity);
        }
        finally
        {
            _addingToLearning = false;
        }
    }

    private async Task PlayAudioAsync(string? audioUrl)
    {
        if (string.IsNullOrWhiteSpace(audioUrl)) return;

        try
        {
            await JS.InvokeVoidAsync("lexiFlow.audio.playExclusive", audioUrl);
        }
        catch (JSException)
        {
            Snackbar.Add("Pronunciation audio could not be played.", Severity.Warning);
        }
    }
}
