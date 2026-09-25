using LexiFlow.Models;
using LexiFlow.Models.NotificationModels;

namespace LexiFlow.Services.NotificationsApiClient;

public interface INotificationsApiClient
{
    Task<ApiResult<IReadOnlyList<AppNotification>>> GetAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> MarkReadAsync(Guid id, CancellationToken cancellationToken = default);
}