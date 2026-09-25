namespace LexiFlow.Models;

public class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    public static ApiResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static ApiResult<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
