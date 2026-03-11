using DualBid.Application.DTOs;
using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
using DualBid.ViewModels.Bid;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public ActionResult Create(int auctionId, int userId, string titleComicAuction, decimal minimunIncrease, decimal currentBidPrice)
        {

            var vm = new CreateBidViewModel
            {
                AuctionId = auctionId,
                UserId = userId,
                TitleComicAuction = titleComicAuction,
                MinimunIncrease = minimunIncrease,
                CurrentBidPrice = currentBidPrice
            };

            return View(vm);
        }

        // POST: LibroController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateBidViewModel viewModeldto)
        {

            if (!ModelState.IsValid)
            {
                // Recopilar todos los errores del ModelState
                var errores = string.Join("<br>",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                );

                // Notificación SweetAlert con el detalle de errores
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "Errores de validación",
                    $"El formulario contiene errores:<br>{errores}",
                    SweetAlertMessageType.warning
                );
                return View(viewModeldto);
            }

            BidDTO dto = new()
            {
                AuctionId = viewModeldto.AuctionId,
                UserId = viewModeldto.UserId,
                AmountOffered = viewModeldto.AmountOffered,
            };

            await _serviceBid.AddAsync(dto);
            //Notificar creación
            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
               "Libro creado correctamente",
               $"El libro {dto.AmountOffered} fue registrado exitosamente.",
               SweetAlertMessageType.success
           );
            return RedirectToAction(
                "AuctionBiddingHistory",
                "Bid",
                new
                {
                    auctionId = viewModeldto.AuctionId,
                    comicTitle = viewModeldto.TitleComicAuction
                });
        }
    }
}
