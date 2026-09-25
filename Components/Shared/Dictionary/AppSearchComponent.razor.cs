using LexiFlow.Models.SearchModels;
using LexiFlow.Services.DictionaryApiClient;
using LexiFlow.Services.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LexiFlow.Components.Shared.Dictionary;

public partial class AppSearchComponent : IDisposable
{
    [Inject] public required IDictionaryApiClient DictionaryApi { get; set; }
    [Inject] public required ILocalStorageService Storage { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }

    [Parameter] public string Placeholder { get; set; } = "Search words, idioms, collocations...";
    [Parameter] public EventCallback<string> OnSearch { get; set; }
    [Parameter] public bool NavigateOnSelect { get; set; }
    [Parameter] public string Query { get; set; } = "";

    private const string RecentKey = "lexiflow:recent";

    private readonly string[] SuggestedWords = ["serendipity", "resilient", "negotiate", "concise", "thrive"];

    private CancellationTokenSource? _searchCts;
    private CancellationTokenSource? _closeCts;
    private ElementReference _input;
    private bool _focused;
    private bool _loading;
    private bool _hasSearched;
    private string? _searchError;
    private int _activeIndex = -1;
    private List<SearchSuggestion> _suggestions = [];
    private List<string> _recent = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _recent = await Storage.GetAsync<List<string>>(RecentKey) ?? [];
        StateHasChanged();
    }

    private async Task OnInput(ChangeEventArgs args)
    {
        Query = args.Value?.ToString() ?? "";
        _activeIndex = -1;
        _hasSearched = false;
        _searchError = null;

        _searchCts?.Cancel();

        if (Query.Trim().Length < 2)
        {
            _suggestions = [];
            _loading = false;
            return;
        }

        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;
        _loading = true;

        try
        {
            await Task.Delay(280, token);

            var result = await DictionaryApi.SearchAsync(Query, token);

            _suggestions = result.Value?.ToList() ?? [];
            _hasSearched = true;
            _searchError = result.IsSuccess ? null : result.Error ?? "Try again in a moment.";
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (!token.IsCancellationRequested)
                _loading = false;
        }
    }

    private void KeepOpen()
    {
        _closeCts?.Cancel();
        _focused = true;
    }

    private async Task OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            var word = _activeIndex >= 0 && _activeIndex < _suggestions.Count ? _suggestions[_activeIndex].Word : Query;
            await Select(word);
        }
        else if (args.Key == "ArrowDown" && _suggestions.Count > 0)
            _activeIndex = Math.Min(_activeIndex + 1, _suggestions.Count - 1);
        else if (args.Key == "ArrowUp" && _suggestions.Count > 0)
            _activeIndex = Math.Max(_activeIndex - 1, 0);
        else if (args.Key == "Escape")
            _focused = false;
    }

    private async Task Select(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        Query = word.Trim();
        _focused = false;

        _recent.RemoveAll(x => x.Equals(Query, StringComparison.OrdinalIgnoreCase));
        _recent.Insert(0, Query);
        _recent = _recent.Take(12).ToList();

        await Storage.SetAsync(RecentKey, _recent);
        await OnSearch.InvokeAsync(Query);

        if (NavigateOnSelect)
            Navigation.NavigateTo($"/dictionary?word={Uri.EscapeDataString(Query)}");
    }

    private void Clear()
    {
        _searchCts?.Cancel();

        Query = "";
        _suggestions = [];
        _activeIndex = -1;
        _hasSearched = false;
        _searchError = null;
        _loading = false;
    }

    private async Task CloseAfterFocusLeaves()
    {
        _closeCts?.Cancel();

        var request = new CancellationTokenSource();
        _closeCts = request;

        try
        {
            await Task.Delay(140, request.Token);

            if (!ReferenceEquals(_closeCts, request)) return;

            _focused = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();

        _closeCts?.Cancel();
        _closeCts?.Dispose();
    }
}
