using Microsoft.AspNetCore.Mvc;


namespace worksServer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Json(new { succeeded = true });
        }

    }
}
