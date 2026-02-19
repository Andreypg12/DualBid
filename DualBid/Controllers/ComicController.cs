using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DualBid.Controllers
{
    public class ComicController : Controller
    {
        private readonly IServiceComic _serviceComic;

        public ComicController(IServiceComic serviceComic)
        {
            _serviceComic = serviceComic;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collection = await _serviceComic.ListAsync();
            return View(collection);
        }
    }
}
