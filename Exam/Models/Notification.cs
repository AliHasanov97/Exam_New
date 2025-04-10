using System.ComponentModel.DataAnnotations.Schema;

namespace Exam.Models
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required string Icon { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public bool IsRead { get; set; } = false;
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}
