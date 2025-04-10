using Exam.DAL;
using Exam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
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
                    .FirstOrDefaultAsync();

                if (_user != null)
                {
                    ViewData["Yükləmə"] = _user.Tranzactions?
                        .Where(x => x.FinancialOperations == "Deposit")
                        .Sum(x => x.Amount) ?? 0;

                    ViewData["Ödəniş"] = _user.Tranzactions?
                        .Where(x => x.FinancialOperations == "Expence")
                        .Sum(x => x.Amount) ?? 0;

                    Console.WriteLine(ViewData["Yükləmə"]);
                    return View(_user);
                }
            }
            return Redirect("/");
        }
    }
}
