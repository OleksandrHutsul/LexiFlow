using LexiFlow.Models.SearchModels;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Shared.Dictionary;

public partial class PhraseNotFoundComponent
{
    [Parameter] public string Phrase { get; set; } = "";
    [Parameter] public string Title { get; set; } = "Phrase not found";
    [Parameter] public IReadOnlyList<SearchSuggestion> Alternatives { get; set; } = [];
    [Parameter] public EventCallback<string> OnSelectAlternative { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
