using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    }
}
