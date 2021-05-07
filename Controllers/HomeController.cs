using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
