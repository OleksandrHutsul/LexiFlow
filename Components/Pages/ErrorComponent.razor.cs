using System.Diagnostics;

namespace LexiFlow.Components.Pages;

public partial class ErrorComponent
{
    private string? _requestId;

    private bool ShowRequestId => !string.IsNullOrWhiteSpace(_requestId);

    protected override void OnInitialized()
    {
        _requestId = Activity.Current?.Id;
    }
}
