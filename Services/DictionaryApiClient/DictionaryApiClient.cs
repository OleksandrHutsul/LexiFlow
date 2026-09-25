using LexiFlow.Configuration;
using LexiFlow.Models;
using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.SearchModels;
using LexiFlow.Services.Http;

namespace LexiFlow.Services.DictionaryApiClient;

public class DictionaryApiClient : IDictionaryApiClient
{
    private readonly IHttpService _httpService;

    public DictionaryApiClient(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public Task<ApiResult<IReadOnlyList<SearchSuggestion>>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(ApiResult<IReadOnlyList<SearchSuggestion>>.Success([]));

        var escaped = Uri.EscapeDataString(query.Trim());

        return _httpService.GetAsync<IReadOnlyList<SearchSuggestion>>($"/api/dictionary/search?query={escaped}", new ApiRequestOptions
        {
            ErrorMessage = "Dictionary suggestions are temporarily unavailable."
        }, cancellationToken);
    }

    public Task<ApiResult<DictionaryEntry>> GetWordAsync(string word, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(word))
            return Task.FromResult(ApiResult<DictionaryEntry>.Failure("Enter a word to look up."));

        var path = string.Join('-', word.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        var escaped = Uri.EscapeDataString(path);

        return _httpService.GetAsync<DictionaryEntry>($"/api/dictionary/{escaped}", new ApiRequestOptions
        {
            ErrorMessage = $"The dictionary API did not return data for \"{word}\"."
        }, cancellationToken);
    }

    public Task<ApiResult<DictionaryLookupResult>> LookupAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(ApiResult<DictionaryLookupResult>.Failure("Enter a word or phrase to look up."));

        var escaped = Uri.EscapeDataString(query.Trim());

        return _httpService.GetAsync<DictionaryLookupResult>($"/api/dictionary/lookup?query={escaped}", new ApiRequestOptions
        {
            ErrorMessage = "The phrase could not be looked up."
        }, cancellationToken);
    }

    public Task<ApiResult<DictionaryEntry>> GetSuggestionAsync(DictionarySuggestion suggestion, CancellationToken cancellationToken = default)
    {
        return _httpService.PostAsync<DictionaryEntry>("/api/dictionary/suggestion", suggestion,
            new ApiRequestOptions
            {
                ErrorMessage = $"The dictionary entry for \"{suggestion.Word}\" could not be loaded."
            }, cancellationToken);
    }
}
