using Microsoft.EntityFrameworkCore;
using MiniExam.Api.Models;

namespace MiniExam.Api.EF
{
    public class ExamDbContext : DbContext
    {
        public ExamDbContext(DbContextOptions<ExamDbContext> options)
            : base(options) { }

        public DbSet<Question> Questions => Set<Question>();
        public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
        public DbSet<ExamResult> ExamResults => Set<ExamResult>();
        public DbSet<ExamAnswer> ExamAnswers => Set<ExamAnswer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExamResult>().Property(r => r.FullName).HasMaxLength(200);
            modelBuilder.Entity<ExamAnswer>()
                .HasIndex(a => new { a.ExamResultId, a.QuestionId }).IsUnique();
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId);

            modelBuilder.Entity<ExamResult>()
                .HasMany(r => r.Answers)
                .WithOne(a => a.ExamResult)
                .HasForeignKey(a => a.ExamResultId);

            modelBuilder.Entity<ExamAnswer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamAnswer>()
                .HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Question>().HasData(
                new Question
                {
                    Id = 1,
                    QuestionText = "ข้อใดต่างจากข้ออื่น",
                    SortOrder = 1
                },
                new Question
                {
                    Id = 2,
                    QuestionText = "X + 2 = 4 จงหาค่า X",
                    SortOrder = 2
                }
            );

            modelBuilder.Entity<QuestionOption>().HasData(
                new QuestionOption { Id = 1, QuestionId = 1, OptionText = "3", SortOrder = 1 },
                new QuestionOption { Id = 2, QuestionId = 1, OptionText = "5", SortOrder = 2 },
                new QuestionOption { Id = 3, QuestionId = 1, OptionText = "9", IsCorrect = true, SortOrder = 3 },
                new QuestionOption { Id = 4, QuestionId = 1, OptionText = "11", SortOrder = 4 },

                new QuestionOption { Id = 5, QuestionId = 2, OptionText = "1", SortOrder = 1 },
                new QuestionOption { Id = 6, QuestionId = 2, OptionText = "2", IsCorrect = true, SortOrder = 2 },
                new QuestionOption { Id = 7, QuestionId = 2, OptionText = "3", SortOrder = 3 },
                new QuestionOption { Id = 8, QuestionId = 2, OptionText = "4", SortOrder = 4 }
            );
        }
    }
}
