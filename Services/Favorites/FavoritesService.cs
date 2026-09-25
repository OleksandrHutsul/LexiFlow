using LexiFlow.Models.DictionaryModels;
using LexiFlow.Services.LocalStorage;
using LexiFlow.Services.State;

namespace LexiFlow.Services.Favorites;

public class FavoritesService : IFavoritesService
{
    private const string FavoritesKey = "lexiflow:favorites";

    private readonly ILocalStorageService _storage;
    private readonly StatisticsChangeNotifier _notifier;

    public FavoritesService(ILocalStorageService storage, StatisticsChangeNotifier notifier)
    {
        _storage = storage;
        _notifier = notifier;
    }

    public async Task<List<DictionaryEntry>> GetAsync()
    {
        return await _storage.GetAsync<List<DictionaryEntry>>(FavoritesKey) ?? [];
    }

    public async Task<bool> IsFavoriteAsync(string word)
    {
        var favorites = await GetAsync();
        return favorites.Any(x => x.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
    }

    public async Task ToggleAsync(DictionaryEntry entry)
    {
        var favorites = await GetAsync();
        var index = favorites.FindIndex(x => x.Word.Equals(entry.Word, StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
            favorites.RemoveAt(index);
        else
            favorites.Insert(0, entry);

        await _storage.SetAsync(FavoritesKey, favorites);
        _notifier.NotifyChanged();
    }
}
