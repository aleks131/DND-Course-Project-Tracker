namespace Backend.Models
{
    public class UserRole
    {
        public string UserId { get; set; } = default!;
        public int RoleId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
} 