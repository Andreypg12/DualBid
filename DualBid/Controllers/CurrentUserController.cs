using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DualBid.Controllers
{
    public class CurrentUserController : Controller
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IserviceUser _serviceUser;

        public CurrentUserController(
            ICurrentUserService currentUserService,
            IserviceUser serviceUser)
        {
            _currentUserService = currentUserService;
            _serviceUser = serviceUser;
        }

        [HttpPost]
        public async Task<IActionResult> Set(int userId, string? returnUrl = null)
        {
            await _currentUserService.SetCurrentUserAsync(userId);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Clear(string? returnUrl = null)
        {
            _currentUserService.ClearCurrentUser();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
