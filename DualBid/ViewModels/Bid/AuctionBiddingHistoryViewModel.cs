using DualBid.Application.DTOs;

namespace DualBid.ViewModels.Bid
{
    public class AuctionBiddingHistoryViewModel
    {
        public string ComicTitle { get; set; } = string.Empty;
        public int AuctionId { get; set; }
        public IEnumerable<BidDTO> Bids { get; set; } = Enumerable.Empty<BidDTO>();
    }
}
