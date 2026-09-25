using LexiFlow.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace LexiFlow.Components.Pages;

public partial class PrivacyComponent
{
    [Inject] public required IOptions<AppContactOptions> ContactOptions { get; set; }

    private AppContactOptions Contact => ContactOptions.Value;
}