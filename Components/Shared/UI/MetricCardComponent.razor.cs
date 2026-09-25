using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Shared.UI;

public partial class MetricCardComponent
{
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string Value { get; set; } = "";
    [Parameter] public string Caption { get; set; } = "";
    [Parameter] public string Icon { get; set; } = Icons.Material.Rounded.Insights;
    [Parameter] public string Accent { get; set; } = "#7c5cff";
}
