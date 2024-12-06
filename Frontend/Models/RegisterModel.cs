using System.ComponentModel.DataAnnotations;
using global::Shared.Models;

namespace Frontend.Models
{
    public class RegisterModel : IRegisterModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name is too long")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name is too long")]
        public string LastName { get; set; } = string.Empty;

        // Implement IRegisterModel.Name as a computed property
        public string Name 
        { 
            get => $"{FirstName} {LastName}".Trim();
            set
            {
                var parts = (value ?? string.Empty).Split(' ', 2);
                FirstName = parts.Length > 0 ? parts[0] : string.Empty;
                LastName = parts.Length > 1 ? parts[1] : string.Empty;
            }
        }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        public string UserType { get; set; } = "User"; // Default to regular user
    }
}