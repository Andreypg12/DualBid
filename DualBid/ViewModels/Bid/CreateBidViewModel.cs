using DualBid.Application.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DualBid.ViewModels.Bid
{
    public class CreateBidViewModel
    {
        [ValidateNever]
        public string TitleComicAuction { get; set; } = string.Empty;

        [ValidateNever]
        public AuctionDTO Auction { get; set; } = new();
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public int OutBidUserId => Auction.CurrentBid.UserId;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal AmountOffered { get; set; }
        
    }
}
