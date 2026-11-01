using System.Collections.Generic;
using System;

namespace MiniExam.Api.Models
{
    public class ExamResult
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmittedAt { get; set; }

        public List<ExamAnswer> Answers { get; set; } = new List<ExamAnswer>();
    }
}
