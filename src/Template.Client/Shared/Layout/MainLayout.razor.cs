using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using MudBlazor;
using Template.Client.Extensions; // Pending verification if needed for Store or specific extensions
using Template.Client.Features.Auth.Services; // For AuthService concrete if needed
using Template.Client.Features.Auth.Services.Interfaces; // For IAuthService
using Template.Client.Features.Auth.State; // For AuthState
using Template.Client.State; // For Store
using Template.Client.Theme;

namespace Template.Client.Shared.Layout;

public partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject] public Store Store { get; set; } = default!;
    [Inject] public IAuthService AuthService { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public IConfiguration Configuration { get; set; } = default!;

    protected bool _isDarkMode = true;
    protected AuthState _authState = new();
    protected MudTheme _theme = AppTheme.DefaultTheme;

    protected bool IsDevelopment => Configuration["ASPNETCORE_ENVIRONMENT"] == "Development" || 
                                  Configuration["Environment"] == "Development";

    protected override void OnInitialized()
    {
        _authState = Store.GetState<AuthState>();
        Store.Subscribe<AuthState>(UpdateState);
    }

    private void UpdateState(AuthState newState)
    {
        _authState = newState;
        InvokeAsync(StateHasChanged);
    }

    protected async Task LogoutAsync()
    {
        await AuthService.LogoutAsync();
        // Dispatch LogoutAction if available, or handled by Service/Redirect
        // The previous code had: Store.Dispatch(new LoginFailureAction("Logged out"));
        // I will keep it identical to preserve behavior.
        Store.Dispatch(new LoginFailureAction("Logged out")); 
        Navigation.NavigateTo("/login");
    }

    protected void ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
    }

    protected bool _drawerOpen = true;

    protected void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    public void Dispose()
    {
    }
}
