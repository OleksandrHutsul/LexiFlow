using LexiFlow.Models;
using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.SearchModels;

namespace LexiFlow.Services.DictionaryApiClient;

public interface IDictionaryApiClient
{
    Task<ApiResult<IReadOnlyList<SearchSuggestion>>> SearchAsync(string query, CancellationToken cancellationToken);
    Task<ApiResult<DictionaryEntry>> GetWordAsync(string word, CancellationToken cancellationToken);
    Task<ApiResult<DictionaryLookupResult>> LookupAsync(string query, CancellationToken cancellationToken = default);
    Task<ApiResult<DictionaryEntry>> GetSuggestionAsync(DictionarySuggestion suggestion, CancellationToken cancellationToken = default);
}
