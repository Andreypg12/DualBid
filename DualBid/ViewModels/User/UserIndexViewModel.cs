using DualBid.Application.DTOs;

namespace DualBid.ViewModels.User
{
    public class UserIndexViewModel
    {
        public string SelectedFilter { get; set; } = "all"; // "all", "blocked"
        public IEnumerable<UserDTO> Users { get; set; } = new List<UserDTO>();
        public int? Page { get; set; }
    }
}
