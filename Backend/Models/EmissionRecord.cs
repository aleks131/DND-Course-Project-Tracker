using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class EmissionRecord
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        public string Source { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public double Amount { get; set; }
        
        public string? Description { get; set; }

        // Conversion methods
        public static Backend.Models.EmissionRecord FromShared(Shared.Models.EmissionRecord shared)
        {
            return new Backend.Models.EmissionRecord
            {
                Id = shared.Id,
                UserId = shared.UserId,
                Date = shared.Date,
                Type = shared.Type,
                Source = shared.Source,
                Amount = shared.Amount,
                Description = shared.Description
            };
        }

        public Shared.Models.EmissionRecord ToShared()
        {
            return new Shared.Models.EmissionRecord
            {
                Id = this.Id,
                UserId = this.UserId,
                Date = this.Date,
                Type = this.Type,
                Source = this.Source,
                Amount = this.Amount,
                Description = this.Description
            };
        }
    }
} 