namespace MiniExam.Api.Models
{
    public class ExamAnswer
    {
        public int Id { get; set; }
        public int ExamResultId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
        public bool IsCorrect { get; set; }

        public ExamResult ExamResult { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public QuestionOption SelectedOption { get; set; } = null!;
    }
}
