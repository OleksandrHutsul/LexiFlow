using LexiFlow.Services.DictionaryApiClient;
using LexiFlow.Services.LearningActivity;
using LexiFlow.Services.VocabularyListsApiClient;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace LexiFlow.Components.Pages.Collections;

public partial class VocabularyListDetailComponent : IAsyncDisposable
{
    [Inject] public required IVocabularyListsApiClient ListsApi { get; set; }
    [Inject] public required IDictionaryApiClient DictionaryApi { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Inject] public required IDialogService Dialogs { get; set; }
    [Inject] public required IJSRuntime JS { get; set; }
    [Inject] public required ILearningActivityService Activity { get; set; }

    [Parameter] public Guid ListId { get; set; }

    private Models.VocabularyModels.VocabularyListDetail? _list;
    private string _editName = "";
    private string? _editDescription;
    private bool _loading = true;
    private bool _saving;

    protected override async Task OnParametersSetAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;

        try
        {
            var result = await ListsApi.GetListAsync(ListId);

            if (!result.IsSuccess || result.Value is null)
            {
                _list = null;
                Snackbar.Add(result.Error ?? "Could not load the list.", Severity.Error);
                return;
            }

            _list = result.Value;
            _editName = _list.Name;
            _editDescription = _list.Description;

            await HydrateWordsAsync(_list.Words);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task SaveListAsync()
    {
        if (_list is null || _saving) return;

        _saving = true;

        try
        {
            var result = await ListsApi.UpdateListAsync(_list.Id, new()
            {
                Name = _editName,
                Description = _editDescription
            });

            if (!result.IsSuccess || result.Value is null)
            {
                Snackbar.Add(result.Error ?? "Could not update the list.", Severity.Error);
                return;
            }

            _list = result.Value;

            Snackbar.Add("List updated.", Severity.Success);
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task DeleteListAsync()
    {
        if (_list is null || _saving) return;

        var confirmed = await Dialogs.ShowMessageBoxAsync("Delete list permanently?", "This action is permanent. Shared access and study data may also be removed.",
            yesText: "Delete", cancelText: "Cancel");

        if (confirmed != true) return;

        _saving = true;

        try
        {
            var result = await ListsApi.DeleteListAsync(_list.Id);

            if (result.IsSuccess)
            {
                Navigation.NavigateTo("/lists");
                return;
            }

            Snackbar.Add(result.Error ?? "Could not delete the list.", Severity.Error);
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task ToggleArchiveAsync()
    {
        if (_list is null || _saving) return;

        _saving = true;

        try
        {
            var result = _list.IsArchived
                ? await ListsApi.RestoreListAsync(_list.Id)
                : await ListsApi.ArchiveListAsync(_list.Id);

            if (!result.IsSuccess)
            {
                Snackbar.Add(result.Error ?? "Could not change archive status.", Severity.Error);
                return;
            }

            _list.IsArchived = !_list.IsArchived;

            Snackbar.Add(_list.IsArchived ? "List archived." : "List restored.", Severity.Success);
        }
        finally
        {
            _saving = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();

        if (!_testActive) return;

        try
        {
            await JS.InvokeVoidAsync("lexiFlow.testGuard.disable");
        }
        catch (JSDisconnectedException)
        {
        }
    }
}