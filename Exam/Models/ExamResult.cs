namespace Exam.Models
{
    public class ExamResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid QuestionId { get; set; }
        public Question? Question { get; set; }

        public string? YouAnswered { get; set; } // istifadəçi cavabı
        public Guid ExamId { get; set; }
        public ExamSession? Exam { get; set; }
    }

}
