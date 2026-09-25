using LexiFlow.Models.DictionaryModels;
using LexiFlow.Services.Favorites;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Pages.Profile;

public partial class FavoritesComponent
{
    [Inject] public required IFavoritesService FavoritesStore { get; set; }

    private List<DictionaryEntry> _favorites = [];
    private string _filter = "";
    private bool _loading = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _favorites = await FavoritesStore.GetAsync();
        _loading = false;

        StateHasChanged();
    }

    private List<DictionaryEntry> GetFilteredFavorites()
    {
        return _favorites
            .Where(entry => string.IsNullOrWhiteSpace(_filter) || entry.Word.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(entry => GetFirstMeaning(entry)?.CefrLevel)
            .ThenBy(entry => entry.Word)
            .ToList();
    }

    private static MeaningItem? GetFirstMeaning(DictionaryEntry entry)
    {
        return entry.Meanings
            .SelectMany(part => part.GuideWordGroups)
            .SelectMany(group => group.Meanings)
            .FirstOrDefault();
    }
}
