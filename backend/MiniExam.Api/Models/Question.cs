
using System.Collections.Generic;

namespace MiniExam.Api.Models
{ 
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}
