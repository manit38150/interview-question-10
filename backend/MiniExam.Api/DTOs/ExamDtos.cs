using System.ComponentModel.DataAnnotations;

namespace MiniExam.Api.DTOs;

public sealed class SubmitExamRequest
{
    [Required, StringLength(200)]
    public string FullName { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public List<SubmitAnswerRequest?> Answers { get; init; } = [];
}

public sealed class SubmitAnswerRequest
{
    [Range(1, int.MaxValue)]
    public int QuestionId { get; init; }

    [Range(1, int.MaxValue)]
    public int SelectedOptionId { get; init; }
}

public sealed record QuestionResponse(int Id, string QuestionText, int SortOrder,
    List<QuestionOptionResponse> Options);
public sealed record QuestionOptionResponse(int Id, string OptionText, int SortOrder);
public sealed record ExamAnswerResponse(int QuestionId, int SelectedOptionId, bool IsCorrect);
public sealed record ExamResultResponse(int Id, string FullName, int Score,
    int TotalQuestions, DateTime SubmittedAt, List<ExamAnswerResponse> Answers);
