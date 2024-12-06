namespace Shared.Models
{
    public interface IRegisterModel
    {
        string Name { get; set; }
        string Email { get; set; }
        string Password { get; set; }
    }
} 