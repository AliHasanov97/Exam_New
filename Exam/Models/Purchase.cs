namespace Exam.Models
{
    public class Purchase
    {
        public Guid? Id { get; set; }
        public Guid? UserId { get; set; }  // Foreign key to User, nullable
        public User User { get; set; }     // Navigation property for User (nullable)

        public Guid? SubjectId { get; set; }  // Foreign key to Subject, nullable
        public Subject Subject { get; set; } // Navigation property for Subject (nullable)

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string Type { get; set; } // E.g., "exam", "topic", etc.
    }
}
