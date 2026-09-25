namespace LexiFlow.Models.NotificationModels;

public class AppNotification
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public Guid? RelatedListId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
