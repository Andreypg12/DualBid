using DualBid.Application.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DualBid.ViewModels.Auction
{
    public class CreateAuctionViewModel
    {
        public AuctionDTO Auction { get; set; } = new();
        public ICollection<ComicDTO> Comics { get; set; } = new List<ComicDTO>();
    }
}
