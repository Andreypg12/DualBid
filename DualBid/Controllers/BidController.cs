using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Hubs;
using DualBid.ViewModels.Bid;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DualBid.Controllers
{
    public class BidController : Controller
    {
        private readonly IServiceBid _serviceBid;
        private readonly IHubContext<AuctionHub> _hubContext;

        public BidController(IServiceBid serviceBid, IHubContext<AuctionHub> hubContext)
        {
            _serviceBid = serviceBid;
            _hubContext = hubContext;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateBidViewModel viewModeldto)
        {
            if (!ModelState.IsValid)
            {
                var errores = string.Join("<br>",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                );

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

            // Evento 1: NuevaPujaSimulada para todos los del grupo
            await _hubContext.Clients
                .Group($"auction-{viewModeldto.AuctionId}")
                .SendAsync("NuevaPujaSimulada", new
                {
                    auctionId = viewModeldto.AuctionId,
                    nuevoMonto = viewModeldto.AmountOffered,
                    liderActual = $"Usuario {viewModeldto.UserId}"
                });

            // Evento 2: UsuarioSuperado (simulado) a un usuario específico
            // Aquí lo simulamos enviándolo a otro userId fijo o recibido desde la vista
            var usuarioSuperadoId = "2";

            await _hubContext.Clients
                .Group($"user-{usuarioSuperadoId}")
                .SendAsync("UsuarioSuperado", new
                {
                    auctionId = viewModeldto.AuctionId,
                    nuevoMonto = viewModeldto.AmountOffered,
                    mensaje = $"Has sido superado en la subasta {viewModeldto.TitleComicAuction}. Nueva puja: ${viewModeldto.AmountOffered:F2}"
                });

            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Puja registrada correctamente",
                $"La puja de {dto.AmountOffered} fue registrada exitosamente.",
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