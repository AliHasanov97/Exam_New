namespace Exam.Models
{
    public class Subject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } =  DateTime.Now;

        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Icon { get; set; }
        public required string Color { get; set; } = "primary"; // UI rəng seçimi
        public required string Type { get; set; } // subject, topic, question
        public string Status { get; set; } = "active";

        public decimal PriceForExam { get; set; } = 0;
        public decimal PriceForRead { get; set; } = 0;
        public decimal PriceForRend { get; set; } = 0;

        public List<Question>? Questions { get; set; }
        public List<Purchase>? Purchases { get; set; }
    }

}
