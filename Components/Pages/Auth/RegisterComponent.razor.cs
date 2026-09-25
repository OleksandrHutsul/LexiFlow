using LexiFlow.Models.AuthenticationModels;
using LexiFlow.Services.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace LexiFlow.Components.Pages.Auth;

public partial class RegisterComponent
{
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
    [Inject] public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }

    private readonly RegisterRequest _model = new();
    private readonly List<string> _errors = [];

    private bool _isBusy;
    private bool _showPassword;

    private InputType PasswordInputType => _showPassword ? InputType.Text : InputType.Password;
    private string PasswordIcon => _showPassword ? Icons.Material.Rounded.VisibilityOff : Icons.Material.Rounded.Visibility;
    private int PasswordStrengthPercent => Math.Min(100, PasswordScore * 20);
    
    private string PasswordStrengthLabel => PasswordScore switch
    {
        <= 1 => "Weak password",
        2 => "Fair password",
        3 => "Good password",
        _ => "Strong password"
    };
    
    private int PasswordScore
    {
        get
        {
            var password = _model.Password ?? "";
            var score = 0;

            if (password.Length >= 8)
                score++;

            if (password.Any(char.IsUpper))
                score++;

            if (password.Any(char.IsLower))
                score++;

            if (password.Any(char.IsDigit))
                score++;

            if (password.Any(character => !char.IsLetterOrDigit(character)))
                score++;

            return score;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (authState.User.Identity?.IsAuthenticated == true)
            Navigation.NavigateTo("/");
    }

    private void TogglePassword()
    {
        _showPassword = !_showPassword;
    }

    private async Task SubmitAsync()
    {
        if (_isBusy) return;

        _errors.Clear();
        Validate();

        if (_errors.Count > 0) return;

        _isBusy = true;

        try
        {
            var result = await AuthenticationService.RegisterAsync(_model);

            if (result.IsSuccess)
            {
                Navigation.NavigateTo("/");
                return;
            }

            _errors.AddRange(result.Errors);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_model.UserName))
            _errors.Add("Username is required.");

        if (string.IsNullOrWhiteSpace(_model.Email))
            _errors.Add("Email is required.");

        if ((_model.Password ?? "").Length < 8)
            _errors.Add("Password must be at least 8 characters.");

        if (_model.Password != _model.ConfirmPassword)
            _errors.Add("Passwords do not match.");
    }
}
