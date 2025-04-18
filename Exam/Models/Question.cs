namespace Exam.Models
{
    public class Question
    {
        public Guid Id { get; set; }
        public int? QuestionNumber { get; set; }
        public string? QuestionText { get; set; }
        public string? AnswerA { get; set; }
        public string? AnswerB { get; set; }
        public string? AnswerC { get; set; }
        public string? AnswerD { get; set; }
        public string? AnswerE { get; set; }

        public string? CorrectAnswer { get; set; }
        public string? Explanation { get; set; }
        public string? Reference { get; set; }
        public Subject? Subject { get; set; }
        public Guid SubjectId { get; set; }
    }

}
