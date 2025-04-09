using Exam.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            string? personJson = HttpContext.Session.GetString("User");

            if (personJson != null)
            {
                // JSON-u User obyektinə deserializasiya edirik
                User? person = JsonConvert.DeserializeObject<User>(personJson);

                if (person != null)
                {
                    return View(person);
                }
            }
            return View();
        }
    }
}
