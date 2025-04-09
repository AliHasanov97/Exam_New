using Microsoft.AspNetCore.Mvc;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class QuestionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Read()
        {
            return View();
        }
    }
}
