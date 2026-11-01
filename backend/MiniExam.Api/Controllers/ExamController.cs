using Microsoft.AspNetCore.Mvc;
using MiniExam.Api.DTOs;
using MiniExam.Api.Services;

namespace MiniExam.Api.Controllers;

[ApiController]
[Route("api/exam")]
public sealed class ExamController(IExamService examService) : ControllerBase
{
    [HttpGet("questions")]
    public async Task<ActionResult<List<QuestionResponse>>> GetQuestions(CancellationToken cancellationToken) =>
        Ok(await examService.GetQuestionsAsync(cancellationToken));

    [HttpPost("submit")]
    public async Task<ActionResult<ExamResultResponse>> Submit(
        SubmitExamRequest request, CancellationToken cancellationToken)
    {
        var outcome = await examService.SubmitAsync(request, cancellationToken);
        if (outcome.NoQuestions)
            return Conflict(new ProblemDetails { Detail = outcome.Error });
        if (outcome.Result is null)
            return BadRequest(new ProblemDetails { Detail = outcome.Error });
        return CreatedAtAction(nameof(GetResult), new { id = outcome.Result.Id }, outcome.Result);
    }

    [HttpGet("results/{id:int:min(1)}")]
    public async Task<ActionResult<ExamResultResponse>> GetResult(int id, CancellationToken cancellationToken)
    {
        var result = await examService.GetResultAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
