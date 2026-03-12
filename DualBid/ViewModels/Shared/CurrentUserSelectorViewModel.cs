using Microsoft.AspNetCore.Mvc.Rendering;

namespace DualBid.ViewModels.Shared
{
    public class CurrentUserSelectorViewModel
    {
        public int? SelectedUserId { get; set; }
        public string CurrentUserDisplayName { get; set; } = "Guest";
        public List<SelectListItem> Users { get; set; } = new();
    }
}
