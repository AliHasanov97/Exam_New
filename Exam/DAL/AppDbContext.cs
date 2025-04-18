using Exam.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Exam.DAL
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CarouselSlide> CarouselSlides { get; set; }
        public DbSet<ExamSession> ExamSessions { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Question> Questions { get; set; }

        public DbSet<Subject> Subjects { get; set; }

        public DbSet<Tariff> Tariffs { get; set; }

        public DbSet<Tranzaction> Tranzactions { get; set; }

        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-many relationship configuration
            modelBuilder.Entity<Purchase>()
                .HasKey(p => p.Id);


            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.User)
                .WithMany(u => u.Purchases)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Burada CASCADE yazılır

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Subject)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SubjectId)
                .OnDelete(DeleteBehavior.Cascade); // Burada CASCADE yazılır


            modelBuilder.Entity<ExamResult>()
    .HasOne(er => er.Question)
    .WithMany()
    .HasForeignKey(er => er.QuestionId)
    .OnDelete(DeleteBehavior.NoAction); // <-- BURADA!

            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.Exam)
                .WithMany(e => e.Results)
                .HasForeignKey(er => er.ExamId)
                .OnDelete(DeleteBehavior.Cascade); // buranı istəyə görə NoAction edə bilərsən
        }
    }
}
