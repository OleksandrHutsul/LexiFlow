using LexiFlow.Models.DictionaryModels;

namespace LexiFlow.Services.Favorites;

public interface IFavoritesService
{
    Task<List<DictionaryEntry>> GetAsync();
    Task<bool> IsFavoriteAsync(string word);
    Task ToggleAsync(DictionaryEntry entry);
}
