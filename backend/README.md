# MiniExam backend

`Controllers` handles HTTP, `Services` handles validation/scoring/persistence,
`EF` configures the context, `Models` represents the database, and `DTOs` defines
the JSON request/response contract. `IExamService` is registered as scoped.

Requires .NET SDK 10. Run commands from the repository root.

```powershell
dotnet restore backend/MiniExam.sln
dotnet run --project backend/MiniExam.Api/MiniExam.Api.csproj --launch-profile http
```

Development runs at `http://localhost:5242` and automatically applies migrations,
creating `exam.db` with two seeded questions. Override the connection with
`ConnectionStrings__ExamDb`. Outside Development, apply migrations before starting:

```powershell
dotnet ef database update --project backend/MiniExam.Api/MiniExam.Api.csproj
```

Use an explicit project path: the API directory also contains a backup csproj.

| Method | Path | Behavior |
| --- | --- | --- |
| GET | `/api/exam/questions` | Ordered questions and options; excludes the answer key |
| POST | `/api/exam/submit` | Validates answers, calculates score, saves result; returns 201 and Location |
| GET | `/api/exam/results/{id}` | Saved result and answers; returns 404 if absent |

Submit exactly one answer per question. Names must contain non-whitespace text and
be at most 200 characters. Duplicate/unknown questions, null answers, and options
belonging to another question return 400. Scores and timestamps are server generated.

```json
{
  "fullName": "Test User",
  "answers": [
    { "questionId": 1, "selectedOptionId": 3 },
    { "questionId": 2, "selectedOptionId": 6 }
  ]
}
```

Verify with:

```powershell
dotnet build backend/MiniExam.sln --no-restore
./backend/Test-Backend.ps1
dotnet list backend/MiniExam.Api/MiniExam.Api.csproj package --vulnerable --include-transitive
```

The smoke test uses a separate temporary SQLite database and stops its server after
completion. It prints the directory containing its database and logs.
