using Exam.DAL;
using Exam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;
        public NotificationController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            string? userID = HttpContext.Session.GetString("User");
            if (userID != null)
            {
                List<Notification> notifications = _context.Users
                  .Where(u => u.Id.ToString() == userID)
                  .SelectMany(u => u.Notifications!)
                                    .ToList();
                if (notifications != null)
                {
                    return View(notifications);
                }
            }
            return View();
        }
    }
}
