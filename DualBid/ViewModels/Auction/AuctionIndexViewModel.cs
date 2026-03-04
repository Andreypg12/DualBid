using DualBid.Application.DTOs;

namespace DualBid.ViewModels.Auction
{
    public class AuctionIndexViewModel
    {
        // "active" | "inactive"
        public string SelectedState { get; set; } = "active"; 
        public IEnumerable<AuctionDTO> Auctions { get; set; } = Enumerable.Empty<AuctionDTO>();
    }
}
