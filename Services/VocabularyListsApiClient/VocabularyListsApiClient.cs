using LexiFlow.Configuration;
using LexiFlow.Models;
using LexiFlow.Models.QuizModels;
using LexiFlow.Models.SearchModels;
using LexiFlow.Models.VocabularyModels;
using LexiFlow.Services.Http;

namespace LexiFlow.Services.VocabularyListsApiClient;

public class VocabularyListsApiClient : IVocabularyListsApiClient
{
    private static readonly ApiRequestOptions Options = new()
    {
        RequiresAuthentication = true,
        ErrorMessage = "Vocabulary lists API is unavailable. Please try again in a moment.",
    };

    private readonly IHttpService _httpService;

    public VocabularyListsApiClient(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public Task<ApiResult<IReadOnlyList<VocabularyListSummary>>> GetListsAsync(CancellationToken cancellationToken = default)
    {
        return _httpService.GetAsync<IReadOnlyList<VocabularyListSummary>>("/api/lists", Options, cancellationToken);
    }

    public Task<ApiResult<IReadOnlyList<VocabularyListSummary>>> GetSharedListsAsync(CancellationToken cancellationToken = default)
    {
        return _httpService.GetAsync<IReadOnlyList<VocabularyListSummary>>("/api/shared-lists", Options, cancellationToken);
    }

    public Task<ApiResult<VocabularyListDetail>> GetListAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _httpService.GetAsync<VocabularyListDetail>($"/api/lists/{id}", Options, cancellationToken);
    }

    public Task<ApiResult<VocabularyListDetail>> CreateListAsync(CreateVocabularyListRequest request, CancellationToken cancellationToken = default)
    {
        return _httpService.PostAsync<VocabularyListDetail>("/api/lists", request, Options, cancellationToken);
    }

    public Task<ApiResult<VocabularyListDetail>> UpdateListAsync(Guid id, UpdateVocabularyListRequest request, CancellationToken cancellationToken = default)
    {
        return _httpService.PutAsync<VocabularyListDetail>($"/api/lists/{id}", request, Options, cancellationToken);
    }

    public Task<ApiResult<bool>> DeleteListAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _httpService.SendNoContentAsync(HttpMethod.Delete, $"/api/lists/{id}", options: Options, cancellationToken: cancellationToken);
    }

    public Task<ApiResult<VocabularyListWord>> AddWordAsync(Guid id, AddVocabularyWordRequest request, CancellationToken cancellationToken = default)
    {
        return _httpService.PostAsync<VocabularyListWord>($"/api/lists/{id}/words", request, Options, cancellationToken);
    }

    public Task<ApiResult<bool>> RemoveWordAsync(Guid id, Guid wordId, CancellationToken cancellationToken = default)
    {
        return _httpService.SendNoContentAsync(HttpMethod.Delete, $"/api/lists/{id}/words/{wordId}", options: Options, cancellationToken: cancellationToken);
    }

    public Task<ApiResult<VocabularyListShare>> ShareListAsync(Guid id, ShareVocabularyListRequest request, CancellationToken cancellationToken = default)
    {
        return _httpService.PostAsync<VocabularyListShare>($"/api/lists/{id}/share", request, Options, cancellationToken);
    }

    public Task<ApiResult<bool>> RemoveShareAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return _httpService.SendNoContentAsync(HttpMethod.Delete, $"/api/lists/{id}/share/{userId}", options: Options, cancellationToken: cancellationToken);
    }

    public Task<ApiResult<bool>> ArchiveListAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _httpService.SendNoContentAsync(HttpMethod.Post, $"/api/lists/{id}/archive", options: Options, cancellationToken: cancellationToken);
    }

    public Task<ApiResult<bool>> RestoreListAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _httpService.SendNoContentAsync(HttpMethod.Post, $"/api/lists/{id}/restore", options: Options, cancellationToken: cancellationToken);
    }

    public Task<ApiResult<IReadOnlyList<VocabularyListSummary>>> GetArchivedListsAsync(CancellationToken cancellationToken = default)
    {
        return _httpService.GetAsync<IReadOnlyList<VocabularyListSummary>>("/api/lists?archived=true", Options, cancellationToken);
    }

    public Task<ApiResult<IReadOnlyList<UserSearchResult>>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
    {
        var escaped = Uri.EscapeDataString(query.Trim());
        return _httpService.GetAsync<IReadOnlyList<UserSearchResult>>($"/api/users/search?query={escaped}", Options, cancellationToken);
    }

    public Task<ApiResult<bool>> SaveTestResultAsync(Guid id, SaveListTestResultRequest request, CancellationToken cancellationToken = default)
    {
        return _httpService.SendNoContentAsync(HttpMethod.Post, $"/api/lists/{id}/test-results", request, Options, cancellationToken);
    }
}
