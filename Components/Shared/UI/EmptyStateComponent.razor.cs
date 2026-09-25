using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Shared.UI;

public partial class EmptyStateComponent
{
    [Parameter] public string Icon { get; set; } = Icons.Material.Rounded.AutoAwesome;
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Text { get; set; } = "";
    [Parameter] public string? ActionHref { get; set; }
    [Parameter] public string ActionText { get; set; } = "Start";
}
