using LexiFlow.Models;
using LexiFlow.Models.QuizModels;
using LexiFlow.Models.SearchModels;
using LexiFlow.Models.VocabularyModels;

namespace LexiFlow.Services.VocabularyListsApiClient;

public interface IVocabularyListsApiClient
{
    Task<ApiResult<IReadOnlyList<VocabularyListSummary>>> GetListsAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<IReadOnlyList<VocabularyListSummary>>> GetSharedListsAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<VocabularyListDetail>> GetListAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult<VocabularyListDetail>> CreateListAsync(CreateVocabularyListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult<VocabularyListDetail>> UpdateListAsync(Guid id, UpdateVocabularyListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> DeleteListAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult<VocabularyListWord>> AddWordAsync(Guid id, AddVocabularyWordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> RemoveWordAsync(Guid id, Guid wordId, CancellationToken cancellationToken = default);
    Task<ApiResult<VocabularyListShare>> ShareListAsync(Guid id, ShareVocabularyListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> RemoveShareAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> ArchiveListAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> RestoreListAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult<IReadOnlyList<VocabularyListSummary>>> GetArchivedListsAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<IReadOnlyList<UserSearchResult>>> SearchUsersAsync(string query, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> SaveTestResultAsync(Guid id, SaveListTestResultRequest request, CancellationToken cancellationToken = default);
}