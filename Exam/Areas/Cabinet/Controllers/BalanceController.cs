using Exam.DAL;
using Exam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class BalanceController : Controller
    {
        private readonly AppDbContext _context;

        public BalanceController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            string? userID = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(userID))
            {
                return RedirectToAction("index", "Home", new { area = "" });
            }

            var user = _context.Users
                .Include(u => u.Tranzactions)
                .FirstOrDefault(u => u.Id.ToString() == userID);

            if (user == null)
            {
                return NotFound();
            }

            // Get active tariffs with discount information
            var tariffs = _context.Tariffs
                .Where(t => t.IsActive)
                .OrderBy(t => t.Price)
                .ToList();

            ViewBag.Tariffs = tariffs;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> AddBalance(decimal amount, string? promocode, string paymentMethod)
        {
            string? userID = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(userID))
            {
                return Json(new { success = false, message = "İstifadəçi tapılmadı" });
            }

            // Validate amount
            if (amount < 5 || amount > 500)
            {
                return Json(new { success = false, message = "Məbləğ 5-500 ₼ aralığında olmalıdır" });
            }

            var user = await _context.Users.FindAsync(Guid.Parse(userID));
            if (user == null)
            {
                return Json(new { success = false, message = "İstifadəçi tapılmadı" });
            }

            // Process promocode if exists
            decimal discountPercent = 0;
            if (!string.IsNullOrEmpty(promocode))
            {
                // In a real app, validate promocode from database
                if (promocode == "WELCOME10")
                {
                    discountPercent = 10;
                }
                else if (promocode == "NEWUSER20")
                {
                    discountPercent = 20;
                }
            }

            // Create transaction
            var transaction = new Tranzaction
            {
                Id = Guid.NewGuid(),
                Amount = amount * (100 - discountPercent) / 100,
                OriginalAmount = discountPercent > 0 ? amount : (decimal?)null,
                DateTime = DateTime.Now,
                Title = "Balans artırımı",
                Description = $"Balans {paymentMethod} ilə artırıldı",
                FinancialOperations = "Deposit",
                Icon = "bi-plus-circle",
                DiscountPercent = discountPercent,
                DiscountAmount = amount * discountPercent / 100,
                Balance = user.Balance + (amount * (100 - discountPercent) / 100),
                UserId = user.Id
            };

            // Update user balance
            user.Balance += transaction.Amount;

            try
            {
                _context.Tranzactions.Add(transaction);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    balance = user.Balance.ToString("0.00", new CultureInfo("az-Latn-AZ")),
                    message = "Balans uğurla artırıldı"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xəta baş verdi: " + ex.Message });
            }
        }

        public async Task<IActionResult> RefreshBalance()
        {
            string? userID = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userID))
            {
                return Json(new { success = false });
            }

            var user = await _context.Users.FindAsync(Guid.Parse(userID));
            if (user == null)
            {
                return Json(new { success = false });
            }

            return Json(new
            {
                success = true,
                balance = user.Balance.ToString("0.00", new CultureInfo("az-Latn-AZ"))
            });
        }
    }
}