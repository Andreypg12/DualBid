using DualBid.Application.DTOs;

namespace DualBid.ViewModels.Bid
{
    public class CreateBidViewModel
    {
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public String TitleComicAuction { get; set; } = string.Empty;
        public decimal MinimunIncrease { get; set; }
        public decimal CurrentBidPrice { get; set; }

        public decimal AmountOffered { get; set; }
    }
}
