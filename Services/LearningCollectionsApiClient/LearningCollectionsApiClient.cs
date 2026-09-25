using LexiFlow.Configuration;
using LexiFlow.Models;
using LexiFlow.Models.LearningModels;
using LexiFlow.Services.Http;

namespace LexiFlow.Services.LearningCollectionsApiClient;

public class LearningCollectionsApiClient : ILearningCollectionsApiClient
{
    private readonly IHttpService _httpService;

    public LearningCollectionsApiClient(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public Task<ApiResult<LearningCollectionsSnapshot>> GetAsync(CancellationToken cancellationToken = default)
    {
        return _httpService.GetAsync<LearningCollectionsSnapshot>("/api/learning-collections", new ApiRequestOptions
        {
            RequiresAuthentication = true,
            ErrorMessage = "Learning collections could not be loaded.",
        }, cancellationToken);
    }

    public Task<ApiResult<LearningCollectionsSnapshot>> SaveAsync(IReadOnlyList<LearningCollection> collections, CancellationToken cancellationToken = default)
    {
        return _httpService.PutAsync<LearningCollectionsSnapshot>("/api/learning-collections", new { collections }, new ApiRequestOptions
        {
            RequiresAuthentication = true,
            ErrorMessage = "Learning collections could not be saved.",
        }, cancellationToken);
    }

    public Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _httpService.SendNoContentAsync(HttpMethod.Delete, $"/api/learning-collections/{id}", options: new ApiRequestOptions
        {
            RequiresAuthentication = true,
            ErrorMessage = "The collection could not be deleted.",
        }, cancellationToken: cancellationToken);
    }
}
