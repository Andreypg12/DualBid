namespace DualBid.Application.DTOs
{
    public class ActiveAuctionDTO
    {
        public int Id { get; set; }

        public DateTime? EndDate { get; set; }

        public int? OwnerUserId { get; set; }
    }
}
