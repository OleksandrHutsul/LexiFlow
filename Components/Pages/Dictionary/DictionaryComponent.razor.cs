using LexiFlow.Models.DictionaryModels;
using LexiFlow.Services.DictionaryApiClient;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Pages.Dictionary;

public partial class DictionaryComponent : IDisposable
{
    [Inject] public required IDictionaryApiClient DictionaryApi { get; set; }

    [SupplyParameterFromQuery(Name = "word")] public string? WordQuery { get; set; }

    private CancellationTokenSource? _cts;
    private DictionaryEntry? _entry;
    private string _query = "";
    private string? _error;
    private bool _loading;
    private bool _notFound;

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(WordQuery))
            await Lookup(WordQuery);
    }

    private async Task Lookup(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        _cts?.Cancel();
        _cts?.Dispose();

        var request = new CancellationTokenSource();
        _cts = request;

        _query = word.Trim();
        _entry = null;
        _error = null;
        _notFound = false;
        _loading = true;

        try
        {
            var result = await DictionaryApi.LookupAsync(_query, request.Token);

            if (request.IsCancellationRequested) return;

            if (!result.IsSuccess || result.Value is null)
            {
                _error = result.Error ?? "The phrase could not be looked up.";
                return;
            }

            if (result.Value.IsFound && result.Value.Entry is not null)
            {
                _entry = result.Value.Entry;
                return;
            }

            _notFound = true;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_cts, request))
                _loading = false;
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
