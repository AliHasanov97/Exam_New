using Exam.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class ResultsController : Controller
    {
        private readonly AppDbContext _context;

        public ResultsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            string? userID = HttpContext.Session.GetString("User");
            var examSessions = _context.ExamSessions
                .Where(u => u.UserId.ToString() == userID)
                .Include(s => s.Results!)
                .ThenInclude(s => s.Question)
                .Include(r => r.Subject)
                .OrderByDescending(s => s.DateTime) // Ən yeni dən əvvəlkiyə sıralama
                .ToList();

            if (!examSessions.Any())
            {
                ViewBag.Score = 0;
                ViewBag.MaxScore = 0;
                ViewBag.ImprovementPercentage = 0;
                return View(examSessions);
            }

            // Ən yeni və əvvəlki imtahan nəticələri
            var latestExam = examSessions.FirstOrDefault(e => e.ExamCompletion.Seconds > 0);
            var previousExam = examSessions.Skip(1).FirstOrDefault(e => e.ExamCompletion.Seconds > 0);

            // Statistikaların hesablanması
            double latestScore = latestExam?.Results?.Count(x => x.Question?.CorrectAnswer == x.YouAnswered) * 100.0 / latestExam?.ExamCount ?? 0;
            double previousScore = previousExam?.Results?.Count(x => x.Question?.CorrectAnswer == x.YouAnswered) * 100.0 / previousExam?.ExamCount ?? 0;

            double improvementPercentage = (previousScore > 0 && latestScore >= previousScore)
                ? ((latestScore - previousScore) / previousScore) * 100
                : 0;

            // ViewBag dəyərlərinin təyin edilməsi
            ViewBag.Score = examSessions.Any(e => e.ExamCompletion.Seconds > 0)
                ? Math.Round(examSessions
                    .Where(e => e.ExamCompletion.Seconds > 0)
                    .Average(e => e.Results!.Count(x => x.Question?.CorrectAnswer == x.YouAnswered) * 100.0 / e.ExamCount), 2)
                : 0;

            ViewBag.MaxScore = examSessions.Any(e => e.ExamCompletion.Seconds > 0)
                ? Math.Round(examSessions
                    .Where(e => e.ExamCompletion.Seconds > 0)
                    .Max(e => e.Results!.Count(x => x.Question?.CorrectAnswer == x.YouAnswered) * 100.0 / e.ExamCount), 2)
                : 0;

            ViewBag.ImprovementPercentage = Math.Round(improvementPercentage, 2);
            ViewBag.LatestScore = Math.Round(latestScore, 2);
            ViewBag.PreviousScore = Math.Round(previousScore, 2);

            return View(examSessions);
        }
        public IActionResult Details(Guid examSessionId)
        {
            var examSession = _context.ExamSessions
                .Where(s => s.Id == examSessionId)
                .Include(s => s.Results!)
                    .ThenInclude(r => r.Question)
                .Include(s => s.Subject)
                .FirstOrDefault();

            if (examSession == null)
            {
                return NotFound();
            }

            // Nəticələri götür
            var results = examSession.Results;

            // Düzgün cavabların sayını hesabla
            int correctCount = results!.Count(r => r.YouAnswered == r.Question?.CorrectAnswer);
            int totalQuestions = results!.Count;
            int score = (int)((correctCount / (double)totalQuestions) * 100);

            // ViewBag-lara yaz
            ViewBag.CorrectCount = correctCount;
            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.Score = score;

            // Nəticədə Exam modelini View-a göndər
            return View(examSession);

        }
    }
}

