using Exam.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Exam.DAL
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CarouselSlide> CarouselSlides { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tranzaction> Tranzactions { get; set; }

    }
}
