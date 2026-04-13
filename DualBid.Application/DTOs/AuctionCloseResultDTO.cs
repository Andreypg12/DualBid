namespace DualBid.Application.DTOs
{
    /// <summary>
    /// Resultado devuelto por CloseAuctionAsync con toda la info
    /// necesaria para las notificaciones SignalR.
    /// </summary>
    public class AuctionCloseResultDTO
    {
        public int AuctionId { get; set; }
        public string? ComicTitle { get; set; }
        public int? WinnerUserId { get; set; }
        public string? WinnerName { get; set; }
        public decimal FinalAmount { get; set; }
        public int? OwnerUserId { get; set; }

        /// <summary>
        /// 3 = Finished (hubo pujas), 4 = Cancelled (sin pujas).
        /// </summary>
        public int FinalStateId { get; set; }
    }
}
