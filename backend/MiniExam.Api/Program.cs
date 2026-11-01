using Microsoft.EntityFrameworkCore;
using MiniExam.Api.EF;
using MiniExam.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddDbContext<ExamDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ExamDb")
        ?? throw new InvalidOperationException("ConnectionStrings:ExamDb is required.")));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<ExamDbContext>().Database.Migrate();
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
