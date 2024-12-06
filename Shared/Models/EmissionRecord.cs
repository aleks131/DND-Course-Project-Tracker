using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class EmissionRecord
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string? Description { get; set; }
    }
} 