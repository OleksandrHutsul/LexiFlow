namespace LexiFlow.Models.FeedbackModels;

public class FeedbackReportItem
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public string? PageOrFeature { get; set; }
    public string? ContactEmail { get; set; }
    public string? Diagnostics { get; set; }
    public string Status { get; set; } = "New";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
}
