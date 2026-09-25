using LexiFlow.Models.VocabularyModels;
using LexiFlow.Services.VocabularyListsApiClient;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Pages.Collections;

public partial class VocabularyListsComponent
{
    [Inject] public required IVocabularyListsApiClient ListsApi { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }

    private List<VocabularyListSummary> _owned = [];
    private List<VocabularyListSummary> _shared = [];
    private List<VocabularyListSummary> _archived = [];
    private string _newName = "";
    private string? _newDescription;
    private bool _showArchived;
    private bool _loading = true;
    private bool _saving;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await LoadAsync();

        StateHasChanged();
    }

    private async Task LoadAsync()
    {
        _loading = true;

        try
        {
            var ownedTask = ListsApi.GetListsAsync();
            var sharedTask = ListsApi.GetSharedListsAsync();
            var archivedTask = ListsApi.GetArchivedListsAsync();

            await Task.WhenAll(ownedTask, sharedTask, archivedTask);

            var owned = await ownedTask;
            var shared = await sharedTask;
            var archived = await archivedTask;

            if (owned.IsSuccess && owned.Value is not null)
                _owned = owned.Value.Where(list => !list.IsArchived).ToList();
            else
                Snackbar.Add(owned.Error ?? "Could not load your lists.", Severity.Error);

            if (shared.IsSuccess && shared.Value is not null)
                _shared = shared.Value.ToList();
            else
                Snackbar.Add(shared.Error ?? "Could not load shared lists.", Severity.Error);

            if (archived.IsSuccess && archived.Value is not null)
                _archived = archived.Value.Where(list => list.IsArchived).ToList();
            else
                Snackbar.Add(archived.Error ?? "Could not load archived lists.", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task CreateListAsync()
    {
        if (_saving) return;

        if (string.IsNullOrWhiteSpace(_newName))
        {
            Snackbar.Add("Give the list a name first.", Severity.Warning);
            return;
        }

        _saving = true;

        try
        {
            var request = new CreateVocabularyListRequest
            {
                Name = _newName,
                Description = _newDescription
            };

            var result = await ListsApi.CreateListAsync(request);

            if (!result.IsSuccess || result.Value is null)
            {
                Snackbar.Add(result.Error ?? "Could not create the list.", Severity.Error);
                return;
            }

            _owned.Insert(0, result.Value);
            _newName = "";
            _newDescription = null;

            Navigation.NavigateTo($"/lists/{result.Value.Id}");
        }
        finally
        {
            _saving = false;
        }
    }

    private void OpenList(Guid id)
    {
        Navigation.NavigateTo($"/lists/{id}");
    }

    private void ToggleArchived()
    {
        _showArchived = !_showArchived;
    }
}
