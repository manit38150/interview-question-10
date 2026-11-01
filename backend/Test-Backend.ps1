$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'MiniExam.Api'
$testDirectory = Join-Path ([IO.Path]::GetTempPath()) ('mini-exam-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory | Out-Null
$listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()
$baseUrl = "http://127.0.0.1:$port"
$savedEnvironment = @{}
foreach ($key in @('ASPNETCORE_ENVIRONMENT', 'ASPNETCORE_URLS', 'ConnectionStrings__ExamDb')) {
    $savedEnvironment[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
}
$server = $null
try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = $baseUrl
    $env:ConnectionStrings__ExamDb = "Data Source=$(Join-Path $testDirectory 'test.db')"
    $server = Start-Process -FilePath 'dotnet' -ArgumentList 'bin/Debug/net10.0/MiniExam.Api.dll' -WorkingDirectory $project -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $testDirectory 'stdout.log') -RedirectStandardError (Join-Path $testDirectory 'stderr.log')
    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        if ($server.HasExited) { throw "Server exited. See $testDirectory" }
        try {
            $questions = Invoke-RestMethod "$baseUrl/api/exam/questions" -TimeoutSec 2
            $ready = $true
            break
        } catch { Start-Sleep -Milliseconds 250 }
    }
    if (-not $ready) { throw 'Server did not become ready.' }
    if ($questions.Count -ne 2) { throw 'Expected two seeded questions.' }
    if (($questions | ConvertTo-Json -Depth 10) -match 'isCorrect') { throw 'Questions expose the answer key.' }

    function Assert-Submit($payload, $expectedStatus, $expectedScore = -1) {
        try {
            $body = [Text.Encoding]::UTF8.GetBytes(($payload | ConvertTo-Json -Depth 10))
            $response = Invoke-WebRequest "$baseUrl/api/exam/submit" -Method Post -ContentType 'application/json; charset=utf-8' -Body $body -UseBasicParsing
            $status = [int]$response.StatusCode
        } catch {
            if (-not $_.Exception.Response) { throw }
            $status = [int]$_.Exception.Response.StatusCode
        }
        if ($status -ne $expectedStatus) { throw "Expected HTTP $expectedStatus, got $status" }
        if ($expectedScore -ge 0) {
            $result = $response.Content | ConvertFrom-Json
            if ($result.Score -ne $expectedScore -or $result.TotalQuestions -ne 2) { throw 'Incorrect score.' }
            $stored = Invoke-RestMethod "$baseUrl/api/exam/results/$($result.Id)"
            if ($stored.Score -ne $expectedScore -or $stored.Answers.Count -ne 2) { throw 'Result was not persisted.' }
            if ($stored.FullName -ne $payload.fullName.Trim()) { throw 'Name was not persisted correctly.' }
            if (-not $response.Headers['Location']) { throw 'Missing result location.' }
        }
    }

    $correct = @(@{ questionId = 1; selectedOptionId = 3 }, @{ questionId = 2; selectedOptionId = 6 })
    Assert-Submit @{ fullName = ' Test User '; answers = $correct } 201 2
    Assert-Submit @{ fullName = 'Test'; answers = @(@{ questionId = 1; selectedOptionId = 1 }, @{ questionId = 2; selectedOptionId = 5 }) } 201 0
    Assert-Submit @{ fullName = 'Test'; answers = @(@{ questionId = 1; selectedOptionId = 3 }, @{ questionId = 2; selectedOptionId = 5 }) } 201 1
    Assert-Submit @{ fullName = ' '; answers = $correct } 400
    Assert-Submit @{ fullName = ('a' * 201); answers = $correct } 400
    Assert-Submit @{ fullName = 'Test'; answers = @() } 400
    Assert-Submit @{ fullName = 'Test'; answers = $null } 400
    Assert-Submit @{ fullName = 'Test'; answers = @($null, $correct[1]) } 400
    Assert-Submit @{ fullName = 'Test'; answers = @($correct[0]) } 400
    Assert-Submit @{ fullName = 'Test'; answers = @($correct[0], $correct[0]) } 400
    Assert-Submit @{ fullName = 'Test'; answers = @(@{ questionId = 999; selectedOptionId = 3 }, $correct[1]) } 400
    Assert-Submit @{ fullName = 'Test'; answers = @(@{ questionId = 1; selectedOptionId = 6 }, $correct[1]) } 400
    Assert-Submit @{ fullName = 'Test'; answers = @(@{ questionId = 1; selectedOptionId = 0 }, $correct[1]) } 400
    try {
        Invoke-WebRequest "$baseUrl/api/exam/results/2147483647" -UseBasicParsing | Out-Null
        throw 'Unknown result should return 404.'
    } catch {
        if (-not $_.Exception.Response -or [int]$_.Exception.Response.StatusCode -ne 404) { throw }
    }
    Write-Output 'PASS: migration/seed, question DTO, scoring, persistence, validation, and missing result.'
} finally {
    if ($server -and -not $server.HasExited) { Stop-Process -Id $server.Id }
    foreach ($key in $savedEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($key, $savedEnvironment[$key], 'Process')
    }
    Write-Output "Test database and logs: $testDirectory"
}
