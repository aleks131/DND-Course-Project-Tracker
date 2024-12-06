using Frontend.Models;
using Shared.Models;
using Shared.Responses;

namespace Frontend.Services
{
    public interface IAuthenticationService
    {
        Task<Frontend.Models.LoginResponse> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<string?> GetTokenAsync();
        Task<UserDto> GetCurrentUser();
        Task<bool> RegisterAsync(RegisterModel model);
        Task<ApiResponse<bool>> RegisterAdminAsync(CreateAdminModel model);
    }
}