using Microsoft.AspNetCore.Mvc;
using DualBid.Application.Services.Interfaces;

namespace DualBid.Controllers
{
    public class RoleController : Controller
    {
        private readonly IServiceRole _serviceRole;
        public RoleController(IServiceRole serviceRole)
        {
            _serviceRole = serviceRole;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collection = await _serviceRole.ListAsync();
            return View(collection);

        }
    }
}
