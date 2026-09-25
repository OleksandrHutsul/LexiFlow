using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Shared.Dictionary;

public partial class WordListCardComponent
{
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Icon { get; set; } = Icons.Material.Rounded.Notes;
    [Parameter] public IReadOnlyList<string> Items { get; set; } = [];
}
