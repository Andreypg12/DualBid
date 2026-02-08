using System.Diagnostics;
using System.Text.Json;
using DualBid.Models;
using Microsoft.AspNetCore.Mvc;

namespace DualBid.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ============================================================ 
        //            MÉTODO MANEJO DE ERRORES 
        // ============================================================ 
        [HttpGet]
        public IActionResult ErrorHandler(string? messagesJson)
        {
            if (string.IsNullOrWhiteSpace(messagesJson))
            {
                ViewBag.ErrorMessages = new ErrorMiddlewareViewModel
                {
                    IdEvent = "SIN-DATO",
                    ListMessages = new List<string> { "No se recibió información de error." },
                    Path = "N/A"
                };

                return View("ErrorHandler");
            }

            ErrorMiddlewareViewModel? errorObject = null;

            try
            {
                errorObject = JsonSerializer.Deserialize<ErrorMiddlewareViewModel>(
                    messagesJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al deserializar mensaje del middleware: {ex.Message} ");

                errorObject = new ErrorMiddlewareViewModel
                {
                    IdEvent = "JSON-INVALIDO",
                    ListMessages = new List<string>
            {
                "El mensaje recibido no tiene un formato válido."
            }
                };
            }

            ViewBag.ErrorMessages = errorObject;
            return View("ErrorHandler");
        }
    }
}
