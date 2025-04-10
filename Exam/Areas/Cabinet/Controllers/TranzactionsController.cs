using Exam.DAL;
using Exam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class TranzactionsController : Controller
    {
        private readonly AppDbContext _context;

        public TranzactionsController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            string? userID = HttpContext.Session.GetString("User");
            var user = _context.Users
                .Include(u => u.Tranzactions)
                .FirstOrDefault(u => u.Id.ToString() == userID);

            if (user == null)
            {
                return NotFound();
            }

            return View(user.Tranzactions?.OrderByDescending(t => t.DateTime).ToList());
        }
    }
}
