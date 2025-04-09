using Exam.DAL;
using Exam.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace Exam.ViewComponent
{
    public class HeaderViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        private readonly AppDbContext _context;

        public HeaderViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            string? User = HttpContext.Session.GetString("User");

            if (User != null)
            {
                User _user = _context.Users.Where(User => User.Id == User.Id).FirstOrDefault();

                ViewData["FullName"] = _user?.FullName;
                ViewData["Picture"] = _user?.Picture;
                ViewData["Email"] = _user?.Email;

            }
            return View();
        }
    }
}
