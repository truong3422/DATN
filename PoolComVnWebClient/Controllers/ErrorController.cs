using Microsoft.AspNetCore.Mvc;

namespace PoolComVnWebClient.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public IActionResult NotAccess()
        {
            return View();
        }

        [HttpGet]
        public IActionResult NotFound()
        {
            return View();
        }

        [HttpGet]
        public IActionResult InternalServerError(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }

        [HttpGet]
        public IActionResult Unauthorized() => View();
    }
}
