using LexiFlow.Services.Learning;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Shared.Collections;

public partial class CollectionEditorDialogComponent
{
    [Inject] public required ILearningService Learning { get; set; }

    [CascadingParameter] public required IMudDialogInstance Dialog { get; set; }
    [Parameter] public Guid? CollectionId { get; set; }
    [Parameter] public string InitialName { get; set; } = "";
    [Parameter] public string InitialAccent { get; set; } = "#7c5cff";
    [Parameter] public bool InitialDefault { get; set; }

    private string _name = "";
    private string _accent = "#7c5cff";
    private bool _makeDefault;
    private bool _saving;
    private string? _error;

    protected override void OnInitialized()
    {
        _name = InitialName;
        _accent = InitialAccent;
        _makeDefault = InitialDefault;
    }

    private async Task SaveAsync()
    {
        _saving = true;
        _error = null;

        try
        {
            var collection = CollectionId.HasValue
                ? await Learning.UpdateCollectionAsync(CollectionId.Value, _name, _accent, _makeDefault)
                : await Learning.CreateCollectionAsync(_name, _accent, _makeDefault);

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
