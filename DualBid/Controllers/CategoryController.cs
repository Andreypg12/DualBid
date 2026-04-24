using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DualBid.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CategoryController : Controller
    {

        private readonly IServiceCategory _serviceCategory;

        public CategoryController(IServiceCategory serviceCategory)
        {
            _serviceCategory = serviceCategory;
        }
        [HttpGet]

        public async Task<IActionResult> Index()
        {
            var collection = await _serviceCategory.ListAsync();

            return View(collection);
        }
    }
}