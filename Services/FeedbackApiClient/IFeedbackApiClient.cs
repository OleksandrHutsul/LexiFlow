using LexiFlow.Models;
using LexiFlow.Models.FeedbackModels;

namespace LexiFlow.Services.FeedbackApiClient;

public interface IFeedbackApiClient
{
    Task<ApiResult<bool>> SubmitAsync(FeedbackSubmission submission, CancellationToken cancellationToken = default);
    Task<ApiResult<List<FeedbackReportItem>>> ListReportsAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<FeedbackReportItem>> GetReportAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult<FeedbackReportItem>> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
}
