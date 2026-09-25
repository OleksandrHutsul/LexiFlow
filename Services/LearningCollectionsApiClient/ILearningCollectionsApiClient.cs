using LexiFlow.Models;
using LexiFlow.Models.LearningModels;

namespace LexiFlow.Services.LearningCollectionsApiClient;

public interface ILearningCollectionsApiClient
{
    Task<ApiResult<LearningCollectionsSnapshot>> GetAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<LearningCollectionsSnapshot>> SaveAsync(IReadOnlyList<LearningCollection> collections, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
