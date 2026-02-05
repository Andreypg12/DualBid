using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Libreria.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IserviceUser _serviceUser;

        public UserController(IserviceUser serviceAutor)
        {
            _serviceUser = serviceAutor;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collection = await _serviceUser.ListAsync();
            return View(collection);
        }
    }
}

