using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using DualBid.ViewModels.Bid;
using Microsoft.AspNetCore.Mvc;
using Mono.TextTemplating;

namespace DualBid.Controllers
{
    public class BidController : Controller
    {
        private readonly IServiceBid _serviceBid;

        public BidController(IServiceBid serviceBid)
        {
            _serviceBid = serviceBid;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collection = await _serviceBid.ListAsync();
            return View(collection);
        }

        public async Task<IActionResult> AuctionBiddingHistory(int auctionId, string comicTitle)
        {
            var history = await _serviceBid.AuctionBiddingHistory(auctionId);

            var vm = new AuctionBiddingHistoryViewModel
            {
                AuctionId = auctionId,
                ComicTitle = comicTitle,
                Bids = history
            };

            return View(vm);
        }
    }
}
