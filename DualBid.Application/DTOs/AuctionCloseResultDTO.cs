namespace DualBid.Application.DTOs
{
    public class AuctionCloseResultDTO
    {
        public int AuctionId { get; set; }
        public string? ComicTitle { get; set; }
        public int? WinnerUserId { get; set; }
        public string? WinnerName { get; set; }
        public decimal FinalAmount { get; set; }
        public int? OwnerUserId { get; set; }

        public int FinalStateId { get; set; }
    }
}
