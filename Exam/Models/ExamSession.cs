namespace Exam.Models
{
    public class ExamSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DateTime { get; set; }
        public TimeSpan ExamDuration { get; set; }
        public TimeSpan ExamCompletion { get; set; }

        public int ExamCount { get; set; }


        public Guid UserId { get; set; }
        public User? User { get; set; }

        public Guid SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public List<ExamResult>? Results { get; set; }
    }

}
