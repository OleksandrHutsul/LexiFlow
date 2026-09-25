using LexiFlow.Enums.TranslationEnums;
using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.LearningModels;
using LexiFlow.Models.TranslationModels;
using LexiFlow.Models.VocabularyModels;
using LexiFlow.Services.DictionaryApiClient;
using LexiFlow.Services.Favorites;
using LexiFlow.Services.Learning;
using LexiFlow.Services.PhotoTranslationApiClient;
using LexiFlow.Services.VocabularyListsApiClient;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace LexiFlow.Components.Pages.PhotoTranslate;

public partial class PhotoTranslateComponent : IAsyncDisposable
{
    [Inject] public required IPhotoTranslationApiClient PhotoApi { get; set; }
    [Inject] public required IDictionaryApiClient DictionaryApi { get; set; }
    [Inject] public required IFavoritesService Favorites { get; set; }
    [Inject] public required ILearningService Learning { get; set; }
    [Inject] public required IVocabularyListsApiClient ListsApi { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required IJSRuntime JS { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Inject] public required IDialogService Dialogs { get; set; }

    private const long MaxBytes = 10 * 1024 * 1024;

    private PhotoTranslationResult? _result;
    private PhotoTranslationMode _mode = PhotoTranslationMode.Text;
    private ElementReference _dropZone;
    private DotNetObjectReference<PhotoTranslateComponent>? _self;
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _reviewSearchCts;
    private CancellationTokenSource? _lookupCts;
    private List<TextToken> _tokens = [];
    private List<VocabularyListSummary> _ownedLists = [];
    private List<string> _wordLists = [];
    private List<LearningCollection> _learningCollections = [];
    private List<ImageVocabularyReviewItem> _reviewItems = [];
    private DictionaryEntry? _selectedEntry;
    private IReadOnlyList<DictionarySuggestion> _lookupSuggestions = [];
    private Guid? _selectedListId;
    private Guid? _selectedLearningCollectionId;
    private int? _rangeStart;
    private int? _rangeEnd;
    private int? _dragOrigin;
    private string _error = "";
    private string _translationNotice = "";
    private string _inspectorPhrase = "";
    private string _resolvedQuery = "";
    private bool _processing;
    private bool _dragging;
    private bool _wordLoading;
    private bool _phraseMiss;
    private bool _isFavorite;
    private bool _inLearning;
    private bool _addingToList;
    private bool _importing;
    private bool _importComplete;
    private bool _retryingTranslation;
    private bool _pointerSelecting;
    private bool _suppressClick;

    private bool HasSelection => _rangeStart is not null && _rangeEnd is not null;
    private string SelectedPhrase => HasSelection ? string.Join(' ', SelectedWordTokens().Select(token => token.Text)) : "";
    private int SelectedWordCount => SelectedWordTokens().Count();
    private string InspectorPhrase => !string.IsNullOrWhiteSpace(SelectedPhrase) ? SelectedPhrase : _inspectorPhrase;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _self = DotNetObjectReference.Create(this);

        await JS.InvokeVoidAsync("lexiFlow.photo.bind", _dropZone, _self);

        var lists = await ListsApi.GetListsAsync();

        if (lists.IsSuccess && lists.Value is not null)
            _ownedLists = lists.Value.Where(list => !list.IsArchived).ToList();

        await ReloadLearningCollectionsAsync();

        StateHasChanged();
    }

    private void ChangeMode(PhotoTranslationMode mode)
    {
        if (_processing || _mode == mode) return;

        _mode = mode;
        _result = null;
        _tokens = [];
        _reviewItems = [];
        _inspectorPhrase = "";
        _error = "";
        _translationNotice = "";

        ClearSelectionState();
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        _reviewSearchCts?.Cancel();
        _reviewSearchCts?.Dispose();

        CancelLookup();

        if (_self is null) return;

        try
        {
            await JS.InvokeVoidAsync("lexiFlow.photo.unbind");
        }
        catch (JSDisconnectedException)
        {
        }

        _self.Dispose();
    }
}
