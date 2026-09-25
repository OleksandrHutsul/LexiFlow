using LexiFlow.Models.FeedbackModels;
using LexiFlow.Services.FeedbackApiClient;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LexiFlow.Components.Pages.Feedback;

public partial class FeedbackReportsComponent
{
    [Inject] public required IFeedbackApiClient FeedbackApi { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }

    private static readonly string[] Statuses = ["New", "InProgress", "Resolved"];

    private List<FeedbackReportItem> _reports = [];
    private FeedbackReportItem? _selected;
    private string _status = "New";
    private bool _loading = true;
    private bool _saving;
    private string? _error;

    protected override Task OnInitializedAsync()
    {
        return LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _error = null;

        try
        {
            var result = await FeedbackApi.ListReportsAsync();

            if (result.IsSuccess && result.Value is not null)
            {
                _reports = result.Value;
                return;
            }

            _error = result.Error ?? "Feedback reports could not be loaded.";
        }
        finally
        {
            _loading = false;
        }
    }

    private void HandleRowClick(TableRowClickEventArgs<FeedbackReportItem> args)
    {
        if (args.Item is null) return;

        _selected = args.Item;
        _status = args.Item.Status;
    }

    private void CloseDetail()
    {
        _selected = null;
    }

    private async Task SaveStatusAsync()
    {
        if (_selected is null || _saving) return;

        _saving = true;

        try
        {
            var result = await FeedbackApi.UpdateStatusAsync(_selected.Id, _status);

            if (!result.IsSuccess || result.Value is null)
            {
                Snackbar.Add(result.Error ?? "Status could not be updated.", Severity.Error);
                return;
            }

            var updatedReport = result.Value;
            var index = _reports.FindIndex(item => item.Id == updatedReport.Id);

            if (index >= 0)
                _reports[index] = updatedReport;

            _selected = updatedReport;

            Snackbar.Add("Status updated.", Severity.Success);
        }
        finally
        {
            _saving = false;
        }
    }

    private static string Truncate(string value, int max)
    {
        return value.Length <= max ? value : value[..max] + "…";
    }

    private static string FormatStatus(string status)
    {
        return status switch
        {
            "InProgress" => "In Progress",
            _ => status
        };
    }
}
