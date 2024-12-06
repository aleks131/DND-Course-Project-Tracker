using Backend.Models;

namespace Backend.Services
{
    public interface IAuthService
    {
        Task<string?> ValidateUserAsync(string email, string password);
        Task<bool> RegisterUserAsync(string email, string password, string firstName, string lastName, string userType);
    }
} 