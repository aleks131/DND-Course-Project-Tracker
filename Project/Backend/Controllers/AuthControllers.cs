@* Pages/Login.razor *@
@page "/login"
@using System.ComponentModel.DataAnnotations
@using Microsoft.AspNetCore.WebUtilities
@inject NavigationManager NavigationManager
@inject IAuthenticationService AuthService

<PageTitle>Login - Carbon Tracker</PageTitle>

<div class="auth-container login-container">
    <div class="auth-left">
        <div class="auth-brand">
            <div class="logo-container animate-fadeIn">
                <img src="/images/logo.png" alt="Carbon Tracker Logo" class="brand-logo" />
            </div>
            <h1 class="brand-name animate-fadeIn-delay">Carbon Tracker</h1>
            <p class="brand-tagline animate-fadeIn-delay-2">Track your environmental impact</p>
        </div>

        <div class="auth-features">
            <div class="feature-item animate-slideIn">
                <div class="feature-icon">
                    <i class="fas fa-chart-line"></i>
                </div>
                <div class="feature-content">
                    <h3>Real-time Analytics</h3>
                    <p>Monitor your carbon footprint with detailed insights</p>
                </div>
            </div>
            <div class="feature-item animate-slideIn-delay">
                <div class="feature-icon">
                    <i class="fas fa-leaf"></i>
                </div>
                <div class="feature-content">
                    <h3>Eco-friendly Tips</h3>
                    <p>Get personalized recommendations for a greener lifestyle</p>
                </div>
            </div>
            <div class="feature-item animate-slideIn-delay-2">
                <div class="feature-icon">
                    <i class="fas fa-users"></i>
                </div>
                <div class="feature-content">
                    <h3>Community Impact</h3>
                    <p>Join others in making a difference for our planet</p>
                </div>
            </div>
        </div>
    </div>

    <div class="auth-right">
        <div class="auth-form-container animate-fadeIn">
            <h2>Welcome Back!</h2>
            <p class="auth-subtitle">Sign in to continue your journey</p>

            @if (!string.IsNullOrEmpty(loginModel.Error))
            {
                <div class="alert alert-danger animate-shake">
                    <i class="fas fa-exclamation-circle"></i>
                    @loginModel.Error
                </div>
            }

            <EditForm Model="@loginModel" OnValidSubmit="HandleLogin">
                <DataAnnotationsValidator />

                <div class="form-group">
                    <label>Email</label>
                    <InputText @bind-Value="loginModel.Username" class="form-control" />
                    <ValidationMessage For="@(() => loginModel.Username)" />
                </div>

                <div class="form-group">
                    <label>Password</label>
                    <InputText type="password" @bind-Value="loginModel.Password" class="form-control" />
                    <ValidationMessage For="@(() => loginModel.Password)" />
                </div>

                <button type="submit" class="btn btn-primary" disabled="@isLoading">
                    @if (isLoading)
                    {
                        <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                        <span>Loading...</span>
                    }
                    else
                    {
                        <span>Login</span>
                    }
                </button>
            </EditForm>

            <div class="social-login">
                <p>Or continue with</p>
                <div class="social-buttons">
                    <button class="btn btn-social btn-google">
                        <i class="fab fa-google"></i>
                        Google
                    </button>
                    <button class="btn btn-social btn-github">
                        <i class="fab fa-github"></i>
                        GitHub
                    </button>
                </div>
            </div>

            <div class="auth-footer">
                <p>Don't have an account?</p>
                <a href="/register" class="btn btn-outline btn-block">
                    <i class="fas fa-user-plus"></i>
                    Create Account
                </a>
            </div>
        </div>
    </div>
</div>

@code {
    private LoginModel loginModel = new();
    private bool isLoading;

    protected override void OnInitialized()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("returnUrl", out var returnUrl))
        {
            loginModel.ReturnUrl = returnUrl;
        }
    }

    private async Task HandleLogin()
    {
        try
        {
            isLoading = true;
            loginModel.Error = null;
            
            var success = await AuthService.LoginAsync(loginModel.Username, loginModel.Password);
            if (success)
            {
                var returnUrl = string.IsNullOrEmpty(loginModel.ReturnUrl) ? "/" : loginModel.ReturnUrl;
                NavigationManager.NavigateTo(returnUrl);
            }
            else
            {
                loginModel.Error = "Invalid username or password";
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            loginModel.Error = "An error occurred during login. Please try again.";
            Console.WriteLine($"Login error: {ex.Message}");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}
