using Shared.Models;

namespace Frontend.Models
{
    /// <summary>
    /// Represents the response received from an authentication attempt.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Indicates if the login attempt was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Contains any message returned from the login process, such as errors or success messages.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Contains the authentication token (JWT or session token) if the login is successful.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Contains the user data if the login is successful.
        /// </summary>
        public UserDto? User { get; set; }
    }
}
