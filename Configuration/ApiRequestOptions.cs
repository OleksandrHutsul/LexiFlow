namespace LexiFlow.Configuration;

public class ApiRequestOptions
{
    public bool RequiresAuthentication { get; init; }
    public bool InvalidateAuthenticationOnUnauthorized { get; init; } = true;
    public string? BearerToken { get; init; }
    public string? ErrorMessage { get; init; }
}
