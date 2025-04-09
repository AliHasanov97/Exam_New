using Microsoft.AspNetCore.Mvc;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class ExamController : Controller
    {
        public IActionResult Parameter()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
