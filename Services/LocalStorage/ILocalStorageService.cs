namespace LexiFlow.Services.LocalStorage;

public interface ILocalStorageService
{
    ValueTask<T?> GetAsync<T>(string key);
    ValueTask SetAsync<T>(string key, T value);
    ValueTask RemoveAsync(string key);
}
