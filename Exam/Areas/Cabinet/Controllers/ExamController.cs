using Exam.DAL;
using Exam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class ExamController : Controller
    {
        private readonly AppDbContext _context;

        public ExamController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Parameter(Guid id)
        {
            string? userID = HttpContext.Session.GetString("User");
            if (userID == null)
            {
                return RedirectToAction("Login", "Account");
            }

            User? user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userID);
            if (user == null)
            {
                return NotFound();
            }

            var subject = _context.Subjects.FirstOrDefault(s => s.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            ViewBag.SubjectId = id;
            ViewBag.SubjectName = subject.Name;
            ViewBag.SubjectDescription = subject.Description;
            ViewBag.Max = _context.Questions.Where(q => q.SubjectId == id).Max(q => q.QuestionNumber) ?? 100;
            ViewBag.price = subject.PriceForExam;
            ViewBag.Check = subject.PriceForExam <= user.Balance;

            return View();
        }

        [HttpPost]
        [ActionName("Parameter")]
        public async Task<IActionResult> ParameterPost(Guid id, int questionCount, int min, int max)
        {
            // Validate user session
            string? userID = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userID))
            {
                return RedirectToAction("Login", "Account");
            }


            User? user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userID);// Validate input parameters
            Subject? subject = _context.Subjects.FirstOrDefault(s => s.Id == id);
            if (min >= max)
            {
                ModelState.AddModelError("", "Max sual nömrəsi min dəyərindən böyük olmalıdır!");
                return RedirectToAction("Parameter", new { id = id });
            }

            if (questionCount <= 0)
            {
                ModelState.AddModelError("", "Sual sayı müsbət ədəd olmalıdır!");
                return RedirectToAction("Parameter", "Exam", new { area = "Cabinet", Id = id });
            }

            try
            {
                var examSession = new ExamSession
                {
                    Id = Guid.NewGuid(),
                    DateTime =  DateTime.Now,
                    ExamDuration = TimeSpan.FromHours(1),
                    ExamCompletion = TimeSpan.Zero,
                    UserId = Guid.Parse(userID),
                    SubjectId = id,
                    ExamCount = questionCount,

                };

                _context.ExamSessions.Add(examSession);
                _context.SaveChanges();

                user!.Balance -= subject!.PriceForExam;

                var tranzaction = new Tranzaction
                {
                    Id = Guid.NewGuid(),
                    DateTime = DateTime.Now,
                    Icon = "book", // və ya mövzuya uyğun dinamik ikon
                    Title = "İmtahan",
                    Description = $"{subject!.Name} mövzusu üzrə imtahan verildi",
                    FinancialOperations = "Expence", // balansdan çıxıldığı üçün
                    DiscountPercent = 0, // əgər endirim yoxdursa
                    DiscountAmount = 0,
                    OriginalAmount = subject.PriceForExam,
                    Amount = subject.PriceForExam,
                    Balance = user!.Balance, // qalan balans
                    UserId = user.Id
                };
                _context.Tranzactions.Add(tranzaction);

                // Bütün dəyişiklikləri yadda saxla
                await _context.SaveChangesAsync();


                TempData["ExamSessionId"] = examSession.Id;
                TempData["min"] = min;
                TempData["max"] = max;

                return RedirectToAction("Index", "Exam", new { sessionId = examSession.Id });
            }
            catch (Exception ex)
            {
                // Log the error (you should implement proper logging)
                ModelState.AddModelError("", "Xəta baş verdi: " + ex.Message);
                return RedirectToAction("Parameter", new { id = id });
            }
        }


        public IActionResult Index()
        {
            string? userID = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userID))
            {
                return RedirectToAction("Login", "Account");
            }
            User? user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userID);
            ViewBag.UserName = user!.FullName;
            ViewBag.UserImage = user.Picture;

            var examSessionId = TempData["ExamSessionId"] as Guid?;
            var min = TempData["min"] as int?;
            var max = TempData["max"] as int?;
            if (examSessionId.HasValue)
            {
                var examSession = _context.ExamSessions
                    .Include(e => e.Subject)
                    .FirstOrDefault(e => e.Id == examSessionId.Value);

                if (examSession != null)
                {
                    var questions = _context.Questions
     .Where(q => q.SubjectId == examSession.SubjectId &&
                 q.QuestionNumber >= min &&
                 q.QuestionNumber <= max)
     .OrderBy(q => Guid.NewGuid())  // Təsadüfi sıralama üçün
     .Take(examSession.ExamCount)
     .ToList();
                    ViewBag.ExamSessionId = examSession.Id;
                    ViewBag.ExamName = examSession.Subject!.Name;
                    return View(questions);
                }
            }

            return View(new List<Question>());
        }

        [HttpPost]
        public IActionResult SubmitExam([FromBody] SubmitExamRequest request)
        {
            try
            {
                string? userID = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(userID))
                    return Unauthorized("İstifadəçi daxil olmayıb.");

                var examSession = _context.ExamSessions
                    .FirstOrDefault(e => e.Id == request.ExamSessionId && e.UserId == Guid.Parse(userID));
                if (examSession == null)
                    return NotFound("Imtahan sessiyası tapılmadı.");

                foreach (var answer in request.Answers)
                {
                    var question = _context.Questions.Find(answer.QuestionId);

                    var examResult = new ExamResult
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = answer.QuestionId,
                        YouAnswered = answer.SelectedOption,
                        ExamId = request.ExamSessionId
                    };
                    _context.ExamResults.Add(examResult);
                }

                examSession.ExamCompletion =  DateTime.Now - examSession.DateTime;
                _context.SaveChanges();

                return Ok(new { success = true, redirectUrl = Url.Action("Details", "Results", new { examSessionId = request.ExamSessionId }) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
    public class SubmitExamRequest
{
    public Guid ExamSessionId { get; set; }
    public List<QuestionAnswer> Answers { get; set; } = new();
}

public class QuestionAnswer
{
    public Guid QuestionId { get; set; }
    public string? SelectedOption { get; set; }
}
}
