namespace MiniExam.Api.Models
{
    public class QuestionOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int SortOrder { get; set; }

        public Question Question { get; set; } = null!;
    }
}
