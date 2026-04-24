using Microsoft.AspNetCore.Mvc;
using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DualBid.Controllers
{
    [Authorize(Roles = "Administrator")]
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
