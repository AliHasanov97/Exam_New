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
            string? userId = HttpContext.Session.GetString("User");

            if (!string.IsNullOrEmpty(userId))
            {
                User? _user = _context.Users.FirstOrDefault(x => x.Id.ToString() == userId);

                if (_user != null)
                {
                    ViewData["FullName"] = _user.FullName;
                    ViewData["Picture"] = _user.Picture;
                    ViewData["Email"] = _user.Email;
                }
            }
            return View();
        }
    }
}
