using LexiFlow.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace LexiFlow.Components.Shared.Layout;

public partial class AppFooterComponent
{
    [Inject] public required IOptions<AppContactOptions> ContactOptions { get; set; }

    protected AppContactOptions Contact => ContactOptions.Value;
}