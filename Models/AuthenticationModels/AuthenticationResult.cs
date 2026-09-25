namespace LexiFlow.Models.AuthenticationModels;

public class AuthenticationResult
{
    public bool IsSuccess { get; init; }
    public AuthenticationResponse? Response { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static AuthenticationResult Success(AuthenticationResponse response) => new() { IsSuccess = true, Response = response };
    public static AuthenticationResult Failure(params string[] errors) => new() { Errors = errors };
    public static AuthenticationResult Failure(IEnumerable<string> errors) => new() { Errors = errors.ToList() };
}
