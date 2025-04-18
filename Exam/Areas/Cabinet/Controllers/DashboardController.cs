using Exam.DAL;
using Exam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            string? userID = HttpContext.Session.GetString("User");


            if (userID != null && Guid.TryParse(userID, out Guid userIdGuid))
            {
                User? _user = await _context.Users
                    .Where(u => u.Id == userIdGuid)
                    .Include(x => x.Tranzactions)
                    .Include(x => x.Exams!)
                        .ThenInclude(x => x.Subject!)
                        .ThenInclude(x => x.Questions!)
                    .FirstOrDefaultAsync();


                if (_user == null)
                {
                    return Redirect("/");
                }

                var examSessions = _context.ExamSessions
                    .Where(u => u.UserId.ToString() == userID)
                    .Include(s => s.Results!)
                    .ThenInclude(s => s.Question)
                    .Include(r => r.Subject)
                    .OrderByDescending(s => s.DateTime) // Ən yeni dən əvvəlkiyə sıralama
                    .ToList();

                ViewBag.Score = examSessions.Any(e => e.ExamCompletion.Seconds > 0)
                 ? Math.Round(examSessions
                     .Where(e => e.ExamCompletion.Seconds > 0)
                     .Average(e => e.Results!.Count(x => x.Question?.CorrectAnswer == x.YouAnswered) * 100.0 / e.ExamCount), 2)
                 : 0;
                var last = _user.Exams!
                    .OrderByDescending(x => x.DateTime)
                    .FirstOrDefault();
                ViewBag.LastName = last?.Subject?.Name ?? "Məlumat yoxdur";
                ViewBag.LastDate = last?.DateTime.ToString("dd MMMM yyyy") ?? "Tarix yoxdur";
                ViewBag.LastScore = last?.Results != null && last.ExamCount > 0
                    ? Math.Round(last.Results.Count(x => x.Question?.CorrectAnswer == x.YouAnswered) * 100.0 / last.ExamCount, 2)
                    : 0;
                return View(_user);
            }

            return Redirect("/");
        }
    }
}
