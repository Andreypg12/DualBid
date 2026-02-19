using Microsoft.AspNetCore.Mvc;
using DualBid.Application.Services.Interfaces;

namespace DualBid.Controllers
{
    public class UserStatusController : Controller
    {
        private readonly IServiceUserStatus _serviceUserStatus;
        public UserStatusController(IServiceUserStatus serviceUserStatus)
        {
            _serviceUserStatus = serviceUserStatus;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collection = await _serviceUserStatus.ListAsync();
            return View(collection);

        }
    }
}
