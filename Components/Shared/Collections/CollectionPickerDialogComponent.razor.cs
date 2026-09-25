using LexiFlow.Models.LearningModels;
using LexiFlow.Services.Learning;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Shared.Collections;

public partial class CollectionPickerDialogComponent
{
    [Inject] public required ILearningService Learning { get; set; }

    [CascadingParameter] public required IMudDialogInstance Dialog { get; set; }

    private List<LearningCollection> _collections = [];
    private bool _loading = true;
    private bool _saving;
    private bool _makeDefault;
    private string _newName = "";
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        _collections = await Learning.GetCollectionsAsync();
        _loading = false;
    }

    private void Choose(Guid id)
    {
        Dialog.Close(DialogResult.Ok(id));
    }

    private async Task CreateAndChooseAsync()
    {
        _saving = true;
        _error = null;

        try
        {
            var collection = await Learning.CreateCollectionAsync(_newName, makeDefault: _makeDefault);
            Dialog.Close(DialogResult.Ok(collection.Id));
        }
        catch (InvalidOperationException exception)
        {
            _error = exception.Message;
        }
        finally
        {
            _saving = false;
        }
    }

    private void Cancel()
    {
        Dialog.Cancel();
    }
}
