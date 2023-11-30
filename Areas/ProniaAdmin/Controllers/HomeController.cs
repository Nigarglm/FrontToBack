using Microsoft.AspNetCore.Mvc;

namespace _16Nov_task.Areas.ProniaAdmin.Controllers
{
    public class HomeController : Controller
    {
        [Area("ProniaAdmin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
