using LexiFlow.Configuration;
using LexiFlow.Models;
using LexiFlow.Models.NotificationModels;
using LexiFlow.Services.Http;

namespace LexiFlow.Services.NotificationsApiClient;

public class NotificationsApiClient : INotificationsApiClient
{
    private readonly IHttpService _httpService;

    public NotificationsApiClient(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public Task<ApiResult<IReadOnlyList<AppNotification>>> GetAsync(CancellationToken cancellationToken = default)
    {
        return _httpService.GetAsync<IReadOnlyList<AppNotification>>("/api/notifications", new ApiRequestOptions
        {
            RequiresAuthentication = true,
            ErrorMessage = "Notifications could not be loaded."
        }, cancellationToken);
    }

    public Task<ApiResult<bool>> MarkReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _httpService.SendNoContentAsync(HttpMethod.Post, $"/api/notifications/{id}/read", options: new ApiRequestOptions
        {
            RequiresAuthentication = true,
            ErrorMessage = "Notification could not be marked as read."
        }, cancellationToken: cancellationToken);
    }
}
