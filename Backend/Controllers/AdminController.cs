using System;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("create")]
        [AllowAnonymous] // Allow creating the first admin without authentication
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
        {
            try
            {
                // Check if username already exists
                var existingUserByUsername = await _userManager.FindByNameAsync(request.Username);
                if (existingUserByUsername != null)
                {
                    return BadRequest(new { message = "Username is already taken" });
                }

                // Check if email already exists
                var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingUserByEmail != null)
                {
                    return BadRequest(new { message = "Email is already registered" });
                }

                var user = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    EmailConfirmed = true // Auto-confirm email for admin accounts
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    // Assign admin role
                    var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation($"Created new admin account for {user.UserName}");
                        return Ok(new
                        {
                            id = user.Id,
                            username = user.UserName,
                            email = user.Email,
                            firstName = user.FirstName,
                            lastName = user.LastName,
                            phoneNumber = user.PhoneNumber,
                            role = "Admin"
                        });
                    }
                    else
                    {
                        // If role assignment fails, delete the created user
                        await _userManager.DeleteAsync(user);
                        return StatusCode(500, new { message = "Failed to assign admin role" });
                    }
                }

                return BadRequest(new { message = result.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin account");
                return StatusCode(500, new { message = "An error occurred while creating the admin account" });
            }
        }
    }
}
