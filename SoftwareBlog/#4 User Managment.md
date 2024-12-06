# User Management System
This project implements a user management system with features for secure login and registration. It includes password hashing, JWT-based authentication, and role-based access control (through claims) to ensure data security and controlled access to resources and routes.

## Table of Content
- [User Management System](#user-management-system)
- [User Roles](#user-roles)
- [Proof of Users](#proof-of-users)
  - [User (Model)](#user-model)
    - [Explanation](#user-model-explanation)
  - [User (Controller)](#user-controller)
    - [Explanation](#user-controller-explanation)
- [Login Function](#login-function)
  - [Login Page (`Register.razor`)](#login-page-registerrazor)
    - [Explanation](#registerrazor-explanation)
  - [Login Page (`Login.razor`)](#login-page-loginrazor)
    - [Explanation](#loginrazor-explanation)
  - [Authentication Service (`AuthenticationService`)](#authentication-service-authenticationservice)
    - [Explanation](#authentication-service-explanation)

## User Roles
1. **User**: Standard users with access to basic features in the application.
2. **Admin**: Admins, are authorized to management features (e.g. adding, deleting, and editing users; deleting and editing waste reports).

## Prove of Users 
### User(Models)
 ```csharp
namespace Backend.Models
{
    public class User
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<EmissionRecord> Emissions { get; set; } = new List<EmissionRecord>();
    }
}
```
#### Explanation
The User model defines the core attributes of users in the system, such as personal details (Email, FirstName, LastName), security data (PasswordHash), and timestamps (CreatedAt, LastLoginAt). Additionally, it includes navigation properties like UserRoles to associate roles with users and Emissions to track user-related activity. This structured approach supports both authentication and role-based access management in the application.

### User(Controllers)
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Services;
using System.Security.Claims;
using BackendUser = Backend.Models.User;
using SharedUser = Shared.Models.User;

namespace Backend.Controllers
{
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SharedUser>> GetUser(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("current")]
        public async Task<ActionResult<SharedUser>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("current")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var existingUser = await _userService.GetUserByIdAsync(userId);
            if (existingUser == null)
                return NotFound();

            var updatedUser = new BackendUser
            {
                Id = userId,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                LastLoginAt = DateTime.UtcNow
            };

            await _userService.UpdateUserAsync(updatedUser);
            return NoContent();
        }
    }

    public record UpdateUserRequest(string Email, string FirstName, string LastName);
}
```
#### Explanation
The UserController provides API endpoints to manage users. It allows:
Fetching user details by ID (GetUser).
Retrieving the currently logged-in user's information (GetCurrentUser).
Updating the current user's profile (UpdateCurrentUser).
With [Authorize] ensuring only authenticated users access these routes, it demonstrates the integration of authentication, claims-based user identification, and a service layer (UserService) for database operations. This design ensures security and scalability.

## Log-in Function 
 * Register.razor
```csharp
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
        <!-- Additional Branding and Features -->
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
```
#### Explanation
The login component provides a user-friendly interface for authenticating users. It collects credentials (email and password) using a form validated with Blazor's EditForm and DataAnnotationsValidator. The HandleLogin method integrates with the IAuthenticationService to authenticate users against a backend system. Upon successful authentication, it redirects users to their desired page or a default location. Errors, such as incorrect credentials, are handled gracefully with meaningful feedback to the user. The design emphasizes simplicity, security, and seamless navigation post-login.

 * Login.razor

```csharp
@page "/login"
@using System.ComponentModel.DataAnnotations
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
        <!-- Branding Content -->
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
```

### **Explanation**
The `Login.razor` component provides the user interface and functionality for logging into the system. It collects credentials via a form, validates them, and communicates with a backend service for authentication.

1. **User Input**:
   - Users enter their email and password.
   - Form inputs are validated using `DataAnnotationsValidator`.

2. **Backend Communication**:
   - On form submission, the `HandleLogin` method sends the credentials to `AuthService.LoginAsync`.
   - If the login succeeds, the user is redirected to the specified `ReturnUrl` or the home page.
   - If it fails, an error message is displayed.

3. **Error Handling and Feedback**:
   - Displays meaningful error messages for failed login attempts.
   - A loading spinner provides visual feedback during the authentication process.

This component ensures secure and user-friendly authentication within the Carbon Tracker system.

* AuthenticationService Code
```csharp
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;
using global::Shared.Models;
using System.Net.Http.Headers;

namespace Frontend.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthenticationService(
            HttpClient httpClient,
            AuthenticationStateProvider authStateProvider,
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var request = new LoginRequest { Email = username, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Token != null)
                    {
                        await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(result.Token);
                        await _localStorage.SetItemAsync("authToken", result.Token);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
            await _localStorage.RemoveItemAsync("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>("authToken");
        }

        public async Task<global::Shared.Models.UserData?> GetUserAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return null;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _httpClient.GetFromJsonAsync<global::Shared.Models.UserData>("api/auth/user");
        }

        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            try
            {
                var registerRequest = new global::Shared.Models.RegisterRequest
                {
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.Name.Split(' ')[0],
                    LastName = model.Name.Split(' ').Length > 1 ? string.Join(" ", model.Name.Split(' ').Skip(1)) : "",
                    UserType = "Customer"
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
```

### **Explanation**
The `AuthenticationService` implements core authentication functionality for the frontend application. It handles user login, logout, token management, and registration by interacting with backend APIs and local storage.

1. **Login**:
   - Sends user credentials (`LoginRequest`) to the backend API (`api/auth/login`).
   - If successful, stores the returned JWT token in local storage and marks the user as authenticated.

2. **Logout**:
   - Clears the user's authentication token from local storage and marks them as logged out.

3. **Token Management**:
   - Retrieves and manages the JWT token used for API requests (`GetTokenAsync`).

4. **User Information**:
   - Fetches the authenticated user's data from the backend using the stored token.

5. **Registration**:
   - Sends user registration details to the backend (`api/auth/register`) and handles success/failure.

The `AuthenticationService` integrates tightly with the `CustomAuthStateProvider` for Blazor's authentication state management, ensuring a secure and seamless user experience.
