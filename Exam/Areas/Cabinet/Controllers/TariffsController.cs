using Microsoft.AspNetCore.Mvc;

namespace Exam.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class TariffsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
