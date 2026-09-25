using LexiFlow.Models.AuthenticationModels;
using LexiFlow.Services.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;

namespace LexiFlow.Components.Pages.Auth;

public partial class LoginComponent : IAsyncDisposable
{
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
    [Inject] public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required IJSRuntime JsRuntime { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")] 
    public string? ReturnUrl { get; set; }

    private readonly LoginRequest _model = new() { RememberMe = true };
    private readonly List<string> _errors = [];

    private DotNetObjectReference<LoginComponent>? _dotNetReference;
    private string? _autofillBindingId;
    private bool _isBusy;
    private bool _showPassword;

    private InputType PasswordInputType => _showPassword ? InputType.Text : InputType.Password;
    private string PasswordIcon => _showPassword ? Icons.Material.Rounded.VisibilityOff : Icons.Material.Rounded.Visibility;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (authState.User.Identity?.IsAuthenticated == true)
            Navigation.NavigateTo(GetSafeReturnUrl());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _dotNetReference = DotNetObjectReference.Create(this);
        _autofillBindingId = await JsRuntime.InvokeAsync<string>("lexiFlow.auth.bindLoginAutofill", _dotNetReference);
    }

    private void TogglePassword()
    {
        _showPassword = !_showPassword;
    }

    [JSInvokable]
    public Task SyncAutofillValues(string? email, string? password)
    {
        var changed = false;

        if (!string.IsNullOrWhiteSpace(email) && _model.Email != email)
        {
            _model.Email = email;
            changed = true;
        }

        if (!string.IsNullOrEmpty(password) && _model.Password != password)
        {
            _model.Password = password;
            changed = true;
        }

        if (changed)
            StateHasChanged();

        return Task.CompletedTask;
    }

    private async Task SubmitAsync()
    {
        _errors.Clear();

        if (string.IsNullOrWhiteSpace(_model.Email) || string.IsNullOrWhiteSpace(_model.Password))
        {
            _errors.Add("Email and password are required.");
            return;
        }

        _isBusy = true;

        try
        {
            var result = await AuthenticationService.LoginAsync(_model);

            if (result.IsSuccess)
            {
                Navigation.NavigateTo(GetSafeReturnUrl());
                return;
            }

            _errors.AddRange(result.Errors);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private string GetSafeReturnUrl()
    {
        if (string.IsNullOrWhiteSpace(ReturnUrl))
            return "/";

        var returnUrl = ReturnUrl.Trim();

        if (!returnUrl.StartsWith('/'))
            returnUrl = "/" + returnUrl;

        if (returnUrl.StartsWith("//", StringComparison.Ordinal))
            return "/";

        var path = returnUrl.TrimStart('/');

        if (path.Equals("login", StringComparison.OrdinalIgnoreCase) || path.StartsWith("login?", StringComparison.OrdinalIgnoreCase)
            || path.Equals("register", StringComparison.OrdinalIgnoreCase) || path.StartsWith("register?", StringComparison.OrdinalIgnoreCase))
            return "/";

        return returnUrl;
    }

    public async ValueTask DisposeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_autofillBindingId))
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("lexiFlow.auth.disposeAutofill", _autofillBindingId);
            }
            catch (JSDisconnectedException)
            {
            }
        }

        _dotNetReference?.Dispose();
    }
}
