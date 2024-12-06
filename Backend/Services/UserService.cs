// Services/UserService.cs
using Backend.Data;
using System;
using System.Threading.Tasks;
using BackendUser = Backend.Models.User;
using SharedUser = Shared.Models.User;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Services
{
    public class UserService
    {
        private readonly DatabaseService _databaseService;

        public UserService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<bool> RegisterAsync(BackendUser user)
        {
            // Check if user already exists
            string checkUserQuery = "SELECT COUNT(1) FROM Users WHERE Email = @Email;";
            var count = Convert.ToInt32(await _databaseService.ExecuteScalarAsync(checkUserQuery, 
                ("@Email", user.Email ?? string.Empty)));
            if (count > 0)
            {
                return false; // Email already exists
            }

            // Insert new user
            user.Id = Guid.NewGuid().ToString();
            user.CreatedAt = DateTime.UtcNow;
            string insertUserQuery = @"
                INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, CreatedAt, LastLoginAt) 
                VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, @CreatedAt, @LastLoginAt);";
            int result = await _databaseService.ExecuteNonQueryAsync(insertUserQuery,
                ("@Id", user.Id),
                ("@Email", user.Email ?? string.Empty),
                ("@PasswordHash", user.PasswordHash ?? string.Empty),
                ("@FirstName", user.FirstName ?? string.Empty),
                ("@LastName", user.LastName ?? string.Empty),
                ("@CreatedAt", user.CreatedAt.ToString("o")),
                ("@LastLoginAt", user.LastLoginAt.HasValue ? user.LastLoginAt.Value.ToString("o") : (object)DBNull.Value)
            );
            return result > 0;
        }

        public async Task<BackendUser?> LoginAsync(string email, string password)
        {
            string loginQuery = "SELECT * FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash;";
            var user = await _databaseService.QuerySingleOrDefaultAsync<BackendUser>(loginQuery, 
                ("@Email", email), 
                ("@PasswordHash", password)
            );

            if (user == null)
                return null;

            // Update LastLogin
            user.LastLoginAt = DateTime.UtcNow;
            string updateLoginTimeQuery = "UPDATE Users SET LastLoginAt = @LastLoginAt WHERE Id = @Id;";
            await _databaseService.ExecuteNonQueryAsync(updateLoginTimeQuery, 
                ("@LastLoginAt", user.LastLoginAt.Value.ToString("o")), 
                ("@Id", user.Id)
            );

            return user;
        }

        public async Task<Shared.Models.User?> GetUserByIdAsync(string? id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            var user = await _databaseService.QuerySingleOrDefaultAsync<BackendUser>(
                "SELECT * FROM Users WHERE Id = @Id",
                ("Id", id)
            );

            return user != null ? SanitizeUser(user) : null;
        }

        public async Task<BackendUser?> GetUserByEmailAsync(string email)
        {
            return await _databaseService.QuerySingleOrDefaultAsync<BackendUser>(
                "SELECT * FROM Users WHERE Email = @Email",
                ("Email", email)
            );
        }

        public async Task<int> UpdateUserAsync(BackendUser user)
        {
            return await _databaseService.ExecuteNonQueryAsync(
                @"UPDATE Users 
                SET FirstName = @FirstName, LastName = @LastName, Email = @Email, LastLoginAt = @LastLoginAt 
                WHERE Id = @Id",
                ("FirstName", user.FirstName ?? string.Empty),
                ("LastName", user.LastName ?? string.Empty),
                ("Email", user.Email ?? string.Empty),
                ("LastLoginAt", user.LastLoginAt ?? DateTime.UtcNow),
                ("Id", user.Id ?? string.Empty)
            );
        }

        public async Task<IEnumerable<SharedUser>> GetAllUsersAsync()
        {
            string query = "SELECT * FROM Users ORDER BY CreatedAt DESC;";
            var users = await _databaseService.QueryAsync<BackendUser>(query);
            return users.Select(u => new SharedUser
            {
                Id = u.Id ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                Role = u.Role ?? "User",
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt ?? DateTime.UtcNow
            });
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            string query = "DELETE FROM Users WHERE Id = @Id;";
            int result = await _databaseService.ExecuteNonQueryAsync(query, ("@Id", id));
            return result > 0;
        }

        private Shared.Models.User SanitizeUser(BackendUser user)
        {
            return new Shared.Models.User
            {
                Id = user.Id ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Username = $"{user.FirstName} {user.LastName}".Trim(),
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
