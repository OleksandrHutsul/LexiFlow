using System.Text.Json;
using Microsoft.JSInterop;

namespace LexiFlow.Services.LocalStorage;

public class LocalStorageService : ILocalStorageService
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async ValueTask<T?> GetAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string?>("lexiFlow.storage.get", key);

        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public async ValueTask SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, Options);
        await _js.InvokeVoidAsync("lexiFlow.storage.set", key, json);
    }

    public async ValueTask RemoveAsync(string key)
    {
        await _js.InvokeVoidAsync("lexiFlow.storage.remove", key);
    }
}
