using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Models;

namespace Frontend.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsers();
        Task<UserDto> GetUserById(string id);
        Task DeleteUser(string id);
        Task UpdateUser(string id, UserDto user);
        Task<UserDto> CreateAdmin(CreateAdminDto adminDto);
    }
}
