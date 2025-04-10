using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Exam.Models
{
    public class Tariff
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(20)]
        public string Color { get; set; } = "primary"; // primary, success, warning, danger, etc.

        [Required]
        public int DurationDays { get; set; }

        [Required]
        public int ExamAttempts { get; set; }

        [Required]
        public bool HasDetailedResults { get; set; } = true;

        [NotMapped]
        public List<string> Features => new List<string>
        {
            $"{ExamAttempts} imtahan",
            $"{DurationDays} gün müddət",
            HasDetailedResults ? "Ətraflı nəticələr" : "Standart nəticələr"
        };

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}