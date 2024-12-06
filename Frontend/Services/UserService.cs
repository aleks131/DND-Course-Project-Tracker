using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Frontend.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/users";

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UserDto>> GetAllUsers()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserDto>>(BaseUrl);
                return response ?? new List<UserDto>();
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
                return new List<UserDto>();
            }
            catch (Exception)
            {
                // Log the error in a production environment
                return new List<UserDto>();
            }
        }

        public async Task<UserDto> GetUserById(string id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<UserDto>($"{BaseUrl}/{id}");
                return response ?? throw new Exception("User not found");
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
                throw;
            }
        }

        public async Task DeleteUser(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to delete user: {error}");
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }

        public async Task UpdateUser(string id, UserDto user)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", user);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to update user: {error}");
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }

        public async Task<UserDto> CreateAdmin(CreateAdminDto adminDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/admin", adminDto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>() 
                        ?? throw new Exception("Failed to create admin: Empty response");
                }
                
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create admin: {error}");
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
                throw;
            }
        }

        public async Task<UserDto> GetCurrentUser()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<UserDto>($"{BaseUrl}/current-user");
                return response ?? throw new Exception("User not found");
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
