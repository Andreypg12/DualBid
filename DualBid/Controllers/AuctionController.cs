using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DualBid.Controllers
{
    public class AuctionController : Controller
    {
        private readonly IServiceAuction _serviceAuction;

        public AuctionController(IServiceAuction serviceAuction)
        {
            _serviceAuction = serviceAuction;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collection = await _serviceAuction.ListAsync();
            return View(collection);
        }
    }
}
