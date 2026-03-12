using DualBid.Application.Services.Interfaces;
using DualBid.ViewModels;
using DualBid.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DualBid.ViewComponents
{
    public class CurrentUserSelectorViewComponent : ViewComponent
    {
        private readonly IserviceUser _serviceUser;
        private readonly ICurrentUserService _currentUserService;

        public CurrentUserSelectorViewComponent(
            IserviceUser serviceUser,
            ICurrentUserService currentUserService)
        {
            _serviceUser = serviceUser;
            _currentUserService = currentUserService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var users = await _serviceUser.ListAsync();
            var currentUser = await _currentUserService.GetCurrentUserAsync();

            var vm = new CurrentUserSelectorViewModel
            {
                SelectedUserId = currentUser?.Id,
                CurrentUserDisplayName = currentUser?.CompleteName ?? "Guest",
                Users = users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.CompleteName} ({u.Role?.Description})",
                        Selected = currentUser != null && u.Id == currentUser.Id
                    })
                    .ToList()
            };

            return View(vm);
        }
    }
}
