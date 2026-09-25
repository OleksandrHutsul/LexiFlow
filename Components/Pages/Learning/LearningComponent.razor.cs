using LexiFlow.Components.Shared.Collections;
using LexiFlow.Enums.LearningEnums;
using LexiFlow.Models.LearningModels;
using LexiFlow.Services.Learning;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Pages.Learning;

public partial class LearningComponent
{
    [Inject] public required ILearningService LearningStore { get; set; }
    [Inject] public required IDialogService Dialogs { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }

    private readonly HashSet<Guid> _deletingCollectionIds = [];

    private List<LearningCollection> _collections = [];
    private bool _loading = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _collections = await LearningStore.GetCollectionsAsync();
        _loading = false;

        StateHasChanged();
    }

    private void OpenCollection(Guid id)
    {
        Navigation.NavigateTo($"/learning/{id}");
    }

    private static string StateLabel(LearningState state)
    {
        return state switch
        {
            LearningState.New => "New",
            LearningState.Learning => "Learning",
            LearningState.Review => "Review",
            LearningState.Mastered => "Learned",
            _ => state.ToString()
        };
    }

    private Task CreateCollectionAsync()
    {
        return OpenEditorAsync(null);
    }

    private Task EditCollectionAsync(LearningCollection collection)
    {
        return OpenEditorAsync(collection);
    }

    private async Task OpenEditorAsync(LearningCollection? collection)
    {
        var parameters = new DialogParameters
        {
            [nameof(CollectionEditorDialogComponent.CollectionId)] = collection?.Id,
            [nameof(CollectionEditorDialogComponent.InitialName)] = collection?.Name ?? "",
            [nameof(CollectionEditorDialogComponent.InitialAccent)] = collection?.Accent ?? "#7c5cff",
            [nameof(CollectionEditorDialogComponent.InitialDefault)] = collection?.IsDefault ?? false
        };

        var title = collection is null ? "Create collection" : "Edit collection";
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await Dialogs.ShowAsync<CollectionEditorDialogComponent>(title, parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false })
            await ReloadAsync();
    }

    private async Task SetDefaultAsync(Guid id)
    {
        var updated = await LearningStore.SetDefaultCollectionAsync(id);

        if (!updated) return;

        await ReloadAsync();

        Snackbar.Add("Default collection updated.", Severity.Success);
    }

    private async Task DeleteCollectionAsync(LearningCollection collection)
    {
        var message = collection.Words.Count == 0
            ? $"Delete “{collection.Name}”?"
            : $"Delete “{collection.Name}” and its {collection.Words.Count} words? This cannot be undone.";

        var confirmed = await Dialogs.ShowMessageBoxAsync("Delete collection", message, yesText: "Delete", cancelText: "Cancel");

        if (confirmed != true) return;

        _deletingCollectionIds.Add(collection.Id);

        try
        {
            var result = await LearningStore.DeleteCollectionAsync(collection.Id);

            if (!result.IsSuccess)
            {
                Snackbar.Add(result.Error ?? "Collection deletion failed.", Severity.Error);
                return;
            }

            _collections.RemoveAll(item => item.Id == collection.Id);

            Snackbar.Add(result.DeletedDefault
                    ? "Collection deleted. No default collection is currently selected."
                    : "Collection deleted.", Severity.Success);
        }
        finally
        {
            _deletingCollectionIds.Remove(collection.Id);
        }
    }

    private async Task ReloadAsync()
    {
        _collections = await LearningStore.GetCollectionsAsync();
    }
}
