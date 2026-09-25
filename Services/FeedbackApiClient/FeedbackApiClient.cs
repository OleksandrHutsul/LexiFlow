using LexiFlow.Configuration;
using LexiFlow.Models;
using LexiFlow.Models.FeedbackModels;
using LexiFlow.Services.Http;

namespace LexiFlow.Services.FeedbackApiClient;

public class FeedbackApiClient : IFeedbackApiClient
{
    private readonly IHttpService _httpService;

    public FeedbackApiClient(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public Task<ApiResult<bool>> SubmitAsync(FeedbackSubmission submission, CancellationToken cancellationToken = default)
    {
        return _httpService.SendNoContentAsync(HttpMethod.Post, "/api/feedback", submission, CreateOptions(), cancellationToken);
    }

    public Task<ApiResult<List<FeedbackReportItem>>> ListReportsAsync(CancellationToken cancellationToken = default)
    {
        return _httpService.GetAsync<List<FeedbackReportItem>>("/api/feedback/admin", CreateOptions(), cancellationToken);
    }

    public Task<ApiResult<FeedbackReportItem>> GetReportAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _httpService.GetAsync<FeedbackReportItem>($"/api/feedback/admin/{id}", CreateOptions(), cancellationToken);
    }

    public Task<ApiResult<FeedbackReportItem>> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        return _httpService.PatchAsync<FeedbackReportItem>($"/api/feedback/admin/{id}/status", new { status }, CreateOptions(), cancellationToken);
    }

    private static ApiRequestOptions CreateOptions()
    {
        return new ApiRequestOptions
        {
            RequiresAuthentication = true,
            ErrorMessage = "Feedback service is temporarily unavailable.",
        };
    }
}
