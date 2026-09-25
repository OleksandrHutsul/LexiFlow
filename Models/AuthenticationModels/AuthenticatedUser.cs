namespace LexiFlow.Models.AuthenticationModels;

public class AuthenticatedUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsAdmin { get; set; }
}
