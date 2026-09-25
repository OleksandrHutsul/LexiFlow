using LexiFlow.Models.SearchModels;
using MudBlazor;

namespace LexiFlow.Components.Pages.Collections;

public partial class VocabularyListDetailComponent
{
    private string _shareEmail = "";
    private List<UserSearchResult> _userSuggestions = [];
    private Guid? _selectedUserId;

    private async Task SearchUsersChangedAsync(string value)
    {
        _shareEmail = value;
        _selectedUserId = null;
        _userSuggestions = [];

        if (value.Trim().Length < 2) return;

        var result = await ListsApi.SearchUsersAsync(value);

        if (!result.IsSuccess || result.Value is null) return;

        _userSuggestions = result.Value
            .Where(user => _list?.Shares.All(share => share.UserId != user.Id) != false)
            .Take(8)
            .ToList();
    }
    private void SelectUser(UserSearchResult user)
    {
        _selectedUserId = user.Id;
        _shareEmail = user.Email;
        _userSuggestions = [];
    }

    private async Task ShareListAsync()
    {
        if (_list is null || _saving || _list.IsArchived || string.IsNullOrWhiteSpace(_shareEmail)) return;

        if (_list.Shares.Any(share => string.Equals(share.Email, _shareEmail, StringComparison.OrdinalIgnoreCase)))
        {
            Snackbar.Add("This user already has access.", Severity.Warning);
            return;
        }

        _saving = true;

        try
        {
            var result = await ListsApi.ShareListAsync(_list.Id, new()
            {
                UserId = _selectedUserId,
                Email = _shareEmail,
                Permission = "Reader"
            });

            if (!result.IsSuccess || result.Value is null)
            {
                Snackbar.Add(result.Error ?? "Could not share the list.", Severity.Error);
                return;
            }

            _list.Shares.Add(result.Value);
            _shareEmail = "";
            _selectedUserId = null;
            _userSuggestions = [];

            Snackbar.Add("List shared. The API will notify the recipient.", Severity.Success);
        }
        finally
        {
            _saving = false;
        }
    }
    private async Task RemoveShareAsync(Guid id)
    {
        if (_list is null || _saving) return;

        _saving = true;

        try
        {
            var result = await ListsApi.RemoveShareAsync(_list.Id, id);

            if (result.IsSuccess)
            {
                _list.Shares.RemoveAll(share => share.UserId == id);
                return;
            }

            Snackbar.Add(result.Error ?? "Could not remove access.", Severity.Error);
        }
        finally
        {
            _saving = false;
        }
    }
}
