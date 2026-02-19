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

        //Esto es lo que hace la comunicacion entre una vista y la otra
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var comic = await _serviceComic.FindByIdAsync(id); 
            if (comic == null) return NotFound();

            return View(comic); 
        }
    }


}
