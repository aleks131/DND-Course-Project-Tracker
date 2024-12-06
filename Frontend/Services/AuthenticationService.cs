using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Models;
using Shared.Responses;
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

        public async Task<Frontend.Models.LoginResponse> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new LoginRequest { Email = email, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        await _localStorage.SetItemAsync("authToken", result.Token);
                        await ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                        return new Frontend.Models.LoginResponse 
                        { 
                            Success = true,
                            Message = "Login successful",
                            Token = result.Token
                        };
                    }
                }

                return new Frontend.Models.LoginResponse 
                { 
                    Success = false,
                    Message = "Login failed",
                    Token = string.Empty 
                };
            }
            catch (Exception ex)
            {
                return new Frontend.Models.LoginResponse 
                { 
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Token = string.Empty 
                };
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>("authToken");
        }

        public async Task<UserDto> GetCurrentUser()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new UserDto();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("api/auth/current-user");
            
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                return user ?? new UserDto();
            }
            
            return new UserDto();
        }

        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ApiResponse<bool>> RegisterAdminAsync(CreateAdminModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register-admin", model);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return result ?? new ApiResponse<bool> { Success = false, Message = "Failed to deserialize response" };
                }
                
                return new ApiResponse<bool> 
                { 
                    Success = false, 
                    Message = $"Failed to create admin account: {response.StatusCode}" 
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> 
                { 
                    Success = false, 
                    Message = $"Error creating admin account: {ex.Message}" 
                };
            }
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}