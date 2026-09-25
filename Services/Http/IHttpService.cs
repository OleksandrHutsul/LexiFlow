using LexiFlow.Configuration;
using LexiFlow.Models;

namespace LexiFlow.Services.Http;

public interface IHttpService
{
    Task<ApiResult<T>> GetAsync<T>(string url, ApiRequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T>> PostAsync<T>(string url, object? body = null, ApiRequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T>> PutAsync<T>(string url, object? body = null, ApiRequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T>> PatchAsync<T>(string url, object? body = null, ApiRequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> SendNoContentAsync(HttpMethod method, string url, object? body = null, ApiRequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request, ApiRequestOptions? options = null, CancellationToken cancellationToken = default);
}
