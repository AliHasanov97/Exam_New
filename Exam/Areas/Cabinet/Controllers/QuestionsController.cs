using Exam.DAL;
using Exam.Models;
using Exam.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class QuestionsController : Controller
    {
        private readonly AppDbContext _context;

        public QuestionsController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<Subject> subjects = _context.Subjects.ToList();
            return View(subjects);
        }
        public IActionResult Read(Guid id)
        {
            Guid userId = Guid.Parse(HttpContext.Session.GetString("User")!);

            var purchasedSubject = _context.Purchases
                .Where(x => x.UserId == userId && x.SubjectId == id)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefault();

            if (purchasedSubject != null)
            {
                if (purchasedSubject.EndDate == null || purchasedSubject.EndDate > DateTime.Now)
                {
                    var subject = _context.Subjects
                        .Include(s => s.Questions)
                        .FirstOrDefault(s => s.Id == id);

                    subject!.Questions = subject.Questions!
                       .OrderBy(q => q.QuestionNumber)
                       .ToList();
                    return View(subject);
                }
            }
            return RedirectToAction("Buy", "Questions", new { id = id });
        }
        public async Task<IActionResult> Buy(Guid id)
        {
            string? userID = HttpContext.Session.GetString("User");

            if (userID != null && Guid.TryParse(userID, out Guid userIdGuid))
            {
                User? _user = await _context.Users
                    .Where(u => u.Id == userIdGuid)
                    .Include(x => x.Tranzactions)
                    .FirstOrDefaultAsync();

                var subject = _context.Subjects
                   .FirstOrDefault(s => s.Id == id);
                BuyViewModel buyVM = new BuyViewModel
                {
                    Subject = subject!,
                    User = _user!
                };
                return View(buyVM);
            }
            return Redirect("/");
        }

        [HttpPost]
        public async Task<IActionResult> Buy(Guid id, string accessType)
        {
            var userID = HttpContext.Session.GetString("User");

            if (userID == null || !Guid.TryParse(userID, out Guid userIdGuid))
            {
                return Redirect("/");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid);
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == id);

            if (user == null || subject == null)
            {
                TempData["ErrorMessage"] = "İstifadəçi və ya mövzu tapılmadı";
                return RedirectToAction("Buy", new { id });
            }

            decimal price = accessType == "permanent" ? subject.PriceForRead : subject.PriceForRend;

            if (user.Balance < price)
            {
                TempData["ErrorMessage"] = "Balansınız kifayət qədər deyil";
                return RedirectToAction("Buy", new { id });
            }

            // Process purchase
            // Process purchase
            user.Balance -= price;

            var purchase = new Purchase
            {
                UserId = user.Id,
                SubjectId = subject.Id,
                StartDate = DateTime.Now,
                EndDate = accessType == "permanent" ? null : DateTime.Now.AddDays(7),
                Type = accessType
            };
            _context.Purchases.Add(purchase);

            // ✅ Yeni tranzaksiya obyektini yarat
            var tranzaction = new Tranzaction
            {
                Id = Guid.NewGuid(),
                DateTime = DateTime.Now,
                Icon = "book", // və ya mövzuya uyğun dinamik ikon
                Title = $"{subject.Name} mövzusu alındı",
                Description = accessType == "permanent" ? "Daimi giriş alındı" : "7 günlük icarə",
                FinancialOperations = "Expence", // balansdan çıxıldığı üçün
                DiscountPercent = 0, // əgər endirim yoxdursa
                DiscountAmount = 0,
                OriginalAmount = price,
                Amount = price,
                Balance = user.Balance, // qalan balans
                UserId = user.Id
            };
            _context.Tranzactions.Add(tranzaction);

            // Bütün dəyişiklikləri yadda saxla
            await _context.SaveChangesAsync();

            return RedirectToAction("Read", new { id });
        }
    }
}
