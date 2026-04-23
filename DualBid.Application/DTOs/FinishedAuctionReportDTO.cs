using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.DTOs
{
    public class FinishedAuctionReportDTO
    {
        public int AuctionId { get; set; }
        public string ComicTitle { get; set; } = null!;
        public string? WinnerName { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime CloseDate { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string CloseReason { get; set; } = null!;
        public bool HasWinner { get; set; }
    }
}
