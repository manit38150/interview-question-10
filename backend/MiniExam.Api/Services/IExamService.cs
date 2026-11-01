using MiniExam.Api.DTOs;

namespace MiniExam.Api.Services;

public interface IExamService
{
    Task<List<QuestionResponse>> GetQuestionsAsync(CancellationToken cancellationToken);
    Task<ExamSubmissionOutcome> SubmitAsync(SubmitExamRequest request, CancellationToken cancellationToken);
    Task<ExamResultResponse?> GetResultAsync(int id, CancellationToken cancellationToken);
}

public sealed record ExamSubmissionOutcome(
    ExamResultResponse? Result = null, string? Error = null, bool NoQuestions = false);
