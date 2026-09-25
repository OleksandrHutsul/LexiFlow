namespace LexiFlow.Models.AuthenticationModels;

public class AuthenticationResponse
{
    public string AccessToken { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
    public AuthenticatedUser User { get; set; } = new();
}
