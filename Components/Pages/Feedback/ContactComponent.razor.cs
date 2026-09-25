using LexiFlow.Configuration;
using LexiFlow.Models;
using LexiFlow.Models.FeedbackModels;
using LexiFlow.Services.FeedbackApiClient;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Reflection;

namespace LexiFlow.Components.Pages.Feedback;

public partial class ContactComponent
{
    [Inject] public required IFeedbackApiClient FeedbackApi { get; set; }
    [Inject] public required IOptions<AppContactOptions> ContactOptions { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required IJSRuntime JS { get; set; }

    [SupplyParameterFromQuery(Name = "type")] public string? TypeQuery { get; set; }

    private static readonly string[] ReportTypes =
    [
        "Bug",
        "Incorrect translation",
        "Incorrect dictionary data",
        "Feature request",
        "Other"
    ];

    private AppContactOptions ContactInfo => ContactOptions.Value;

    private string _type = "Bug";
    private string _description = "";
    private string _pageOrFeature = "";
    private string _email = "";
    private bool _includeDiagnostics = true;
    private bool _sending;
    private bool _submitted;
    private string? _error;
    private string _userAgent = "";
    private string _referrer = "";
    private string _language = "";
    private string _platform = "";

    protected override void OnInitialized()
    {
        var reportType = ReportTypes.FirstOrDefault(type => type.Equals(TypeQuery, StringComparison.OrdinalIgnoreCase));

        if (reportType is not null)
            _type = reportType;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            var environment = await JS.InvokeAsync<BrowserEnvironment>("lexiFlow.diagnostics.getEnvironment");

            _userAgent = environment.UserAgent ?? "";
            _referrer = environment.Referrer ?? "";
            _language = environment.Language ?? "";
            _platform = environment.Platform ?? "";
        }
        catch (JSException)
        {
        }
    }

    private async Task SubmitAsync()
    {
        _error = null;

        if (_description.Trim().Length < 10)
        {
            _error = "Please describe the issue in at least 10 characters.";
            return;
        }

        _sending = true;

        try
        {
            var submission = new FeedbackSubmission
            {
                Type = _type,
                Description = _description.Trim(),
                PageOrFeature = string.IsNullOrWhiteSpace(_pageOrFeature) ? null : _pageOrFeature.Trim(),
                ContactEmail = string.IsNullOrWhiteSpace(_email) ? null : _email.Trim(),
                Diagnostics = _includeDiagnostics ? BuildDiagnostics() : null
            };

            var result = await FeedbackApi.SubmitAsync(submission);

            if (!result.IsSuccess)
            {
                _error = result.Error ?? "The report could not be sent.";
                return;
            }

            _submitted = true;
        }
        finally
        {
            _sending = false;
        }
    }

    private void ResetForm()
    {
        _type = "Bug";
        _description = "";
        _pageOrFeature = "";
        _email = "";
        _submitted = false;
        _error = null;
    }

    private string BuildDiagnostics()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

        var diagnostics = new List<string>
        {
            $"page={Navigation.Uri}",
            $"version={version}",
            $"timestamp={DateTimeOffset.UtcNow:O}"
        };

        if (!string.IsNullOrWhiteSpace(_referrer))
            diagnostics.Add($"referrer={_referrer}");

        if (!string.IsNullOrWhiteSpace(_language))
            diagnostics.Add($"language={_language}");

        if (!string.IsNullOrWhiteSpace(_platform))
            diagnostics.Add($"platform={_platform}");

        if (!string.IsNullOrWhiteSpace(_userAgent))
            diagnostics.Add($"userAgent={_userAgent}");

        return string.Join(Environment.NewLine, diagnostics);
    }
}
