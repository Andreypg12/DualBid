using DualBid.Application.DTOs;

namespace DualBid.ViewModels.Auction
{
    public class AuctionIndexViewModel
    {
        public string SelectedState { get; set; } = "active"; // "active" | "inactive"
        public IEnumerable<AuctionDTO> Auctions { get; set; } = Enumerable.Empty<AuctionDTO>();
    }
}
