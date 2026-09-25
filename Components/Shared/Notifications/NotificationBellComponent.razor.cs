using LexiFlow.Models.NotificationModels;
using LexiFlow.Services.NotificationsApiClient;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace LexiFlow.Components.Shared.Notifications;

public partial class NotificationBellComponent : IDisposable
{
    [Inject] public required INotificationsApiClient NotificationsApi { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }

    private readonly CancellationTokenSource _pollCancellation = new();

    private List<AppNotification> _items = [];
    private MudMenu? _menu;
    private bool _loading;

    private int UnreadCount => _items.Count(x => !x.IsRead);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await RefreshAsync();
        _ = PollAsync(_pollCancellation.Token);
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
                await InvokeAsync(RefreshAsync);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RefreshAsync()
    {
        await LoadAsync();
        StateHasChanged();
    }

    private async Task LoadAsync()
    {
        _loading = true;

        var result = await NotificationsApi.GetAsync();

        if (result.IsSuccess && result.Value is not null)
            _items = result.Value.OrderByDescending(x => x.CreatedAt).ToList();

        _loading = false;
    }

    private async Task OpenNotificationsAsync(MouseEventArgs args)
    {
        if (_menu is null) return;

        await LoadAsync();
        await _menu.OpenMenuAsync(args);
    }

    private async Task OpenAsync(AppNotification item)
    {
        if (!item.IsRead)
        {
            var result = await NotificationsApi.MarkReadAsync(item.Id);

            if (result.IsSuccess)
                item.IsRead = true;
            else
                Snackbar.Add(result.Error ?? "Could not mark notification as read.", Severity.Warning);
        }

        if (item.RelatedListId is Guid id)
            Navigation.NavigateTo($"/lists/{id}");
    }

    public void Dispose()
    {
        _pollCancellation.Cancel();
        _pollCancellation.Dispose();
    }
}
