using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Exam.Models
{
    public class Tranzaction
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public required string Icon { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string FinancialOperations { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Balance { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public Guid UserId { get; set; }

    }
}
