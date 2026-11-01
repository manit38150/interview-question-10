using Microsoft.EntityFrameworkCore;
using MiniExam.Api.DTOs;
using MiniExam.Api.EF;
using MiniExam.Api.Models;

namespace MiniExam.Api.Services;

public sealed class ExamService(ExamDbContext db) : IExamService
{
    public Task<List<QuestionResponse>> GetQuestionsAsync(CancellationToken cancellationToken) =>
        db.Questions.AsNoTracking().OrderBy(q => q.SortOrder).ThenBy(q => q.Id)
            .Select(q => new QuestionResponse(q.Id, q.QuestionText, q.SortOrder,
                q.Options.OrderBy(o => o.SortOrder).ThenBy(o => o.Id)
                    .Select(o => new QuestionOptionResponse(o.Id, o.OptionText, o.SortOrder)).ToList()))
            .ToListAsync(cancellationToken);

    public async Task<ExamSubmissionOutcome> SubmitAsync(
        SubmitExamRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 200)
            return new(Error: "กรุณากรอกชื่อ-สกุล ความยาวไม่เกิน 200 ตัวอักษร");

        var questions = await db.Questions.AsNoTracking().Include(q => q.Options)
            .ToListAsync(cancellationToken);
        if (questions.Count == 0)
            return new(Error: "ยังไม่มีข้อสอบในระบบ", NoQuestions: true);

        if (request.Answers is null || request.Answers.Any(a => a is null)
            || request.Answers.Count != questions.Count
            || request.Answers.Select(a => a!.QuestionId).Distinct().Count() != questions.Count)
            return new(Error: "กรุณาเลือกคำตอบให้ครบทุกข้อ ข้อละหนึ่งคำตอบ");

        var questionsById = questions.ToDictionary(q => q.Id);
        var result = new ExamResult
        {
            FullName = request.FullName.Trim(),
            TotalQuestions = questions.Count,
            SubmittedAt = DateTime.UtcNow
        };
        foreach (var answer in request.Answers)
        {
            if (!questionsById.TryGetValue(answer!.QuestionId, out var question))
                return new(Error: "ไม่พบคำถามที่ระบุ");
            var option = question.Options.SingleOrDefault(o => o.Id == answer.SelectedOptionId);
            if (option is null)
                return new(Error: "ตัวเลือกไม่ตรงกับคำถามที่ระบุ");
            result.Answers.Add(new ExamAnswer
            {
                QuestionId = question.Id,
                SelectedOptionId = option.Id,
                IsCorrect = option.IsCorrect
            });
        }

        result.Score = result.Answers.Count(a => a.IsCorrect);
        db.ExamResults.Add(result);
        await db.SaveChangesAsync(cancellationToken);
        return new(Result: ToResponse(result));
    }

    public async Task<ExamResultResponse?> GetResultAsync(int id, CancellationToken cancellationToken)
    {
        var result = await db.ExamResults.AsNoTracking().Include(r => r.Answers)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        return result is null ? null : ToResponse(result);
    }

    private static ExamResultResponse ToResponse(ExamResult result) => new(
        result.Id, result.FullName, result.Score, result.TotalQuestions, result.SubmittedAt,
        result.Answers.OrderBy(a => a.QuestionId)
            .Select(a => new ExamAnswerResponse(a.QuestionId, a.SelectedOptionId, a.IsCorrect)).ToList());
}
