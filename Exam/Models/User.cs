using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Exam.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required string FullName { get; set; }
        public required string Picture { get; set; }

        [EmailAddress]
        public required string Email { get; set; }
        public decimal Balance { get; set; }
        public bool Active { get; set; }

        public List<Tranzaction>? Tranzactions { get; set; }
        public List<Notification>? Notifications { get; set; }


    }
}
