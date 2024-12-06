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
