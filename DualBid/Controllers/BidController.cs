using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Hubs;
using DualBid.ViewModels.Bid;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DualBid.Controllers
{
    public class BidController : Controller
    {
        private readonly IServiceBid _serviceBid;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly IServiceAuction _serviceAuction;

        public BidController(IServiceBid serviceBid, IHubContext<AuctionHub> hubContext, IServiceAuction serviceAuction)
        {
            _serviceBid = serviceBid;
            _hubContext = hubContext;
            _serviceAuction = serviceAuction;
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

        [HttpGet]
        public async Task<IActionResult> Create(int auctionId, string titleComicAuction)
        {
            var auction = await _serviceAuction.FindByIdAsync(auctionId);
            if (auction == null)
            {
                TempData["ErrorMessage"] = "Auction not found";
                return RedirectToAction("Index", "Auction");
            }

            if (auction.State.Id != 2)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction Unavailable",
                    "This auction is not currently accepting bids.",
                    SweetAlertMessageType.warning);
                return RedirectToAction("Details", "Auction", new { id = auctionId });
            }

            var vm = new CreateBidViewModel
            {
                Auction = auction,
                AuctionId = auction.Id,
                TitleComicAuction = titleComicAuction ?? auction.Comic?.Title ?? "Unknown",
                AmountOffered = (auction.CurrentBid?.AmountOffered ?? auction.BasePrice) + auction.MinimunIncrease
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateBidViewModel viewModel)
        {
            try
            {
                if (viewModel.AuctionId == 0)
                {
                    TempData["ErrorMessage"] = "Invalid auction";
                    return RedirectToAction("Index", "Auction");
                }

                var auction = await _serviceAuction.FindByIdAsync(viewModel.AuctionId);

                if (auction == null)
                {
                    TempData["ErrorMessage"] = "Auction not found";
                    return RedirectToAction("Index", "Auction");
                }

                if (auction.State.Id == 3 || auction.State.Id == 4)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Auction Unavailable",
                        "This auction has already ended or been cancelled. No more bids can be placed.",
                        SweetAlertMessageType.warning);
                    return RedirectToAction("Details", "Auction", new { id = viewModel.AuctionId });
                }

                if (auction.State.Id != 2)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Auction Not Active",
                        "This auction is not currently active. Bids can only be placed on active auctions.",
                        SweetAlertMessageType.warning);
                    return RedirectToAction("Details", "Auction", new { id = viewModel.AuctionId });
                }

                viewModel.Auction = auction;

                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    TempData["ErrorMessage"] = "Please correct the errors in the form";
                    TempData["ErrorDetails"] = errors;
                    return View(viewModel);
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                viewModel.UserId = userIdClaim != null ? int.Parse(userIdClaim) : 0;

                if (viewModel.UserId == 0)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "¡You need to log in!",
                        "You must be logged in to place a bid",
                        SweetAlertMessageType.error);
                    return View(viewModel);
                }

                // Validar incremento mínimo
                var currentBidAmount = auction.CurrentBid?.AmountOffered ?? auction.BasePrice;
                var minimumRequired = currentBidAmount + auction.MinimunIncrease;

                if (viewModel.AmountOffered < minimumRequired)
                {
                    ModelState.AddModelError("AmountOffered", $"The bid must be at least ${minimumRequired:N2}");
                    TempData["ErrorMessage"] = $"Invalid amount. Minimum required is ${minimumRequired:N2}";
                    return View(viewModel);
                }

                // ── Capturar el usuario superado ANTES de guardar la nueva puja 
                // CurrentBid es la puja más alta actual — su dueño será superado.
                int? outbidUserId = auction.CurrentBid?.UserId;
                string? outbidUserName = auction.CurrentBid?.User?.CompleteName;

                // Guardar la nueva puja
                var dto = new BidDTO
                {
                    AuctionId = viewModel.AuctionId,
                    UserId = viewModel.UserId,
                    AmountOffered = viewModel.AmountOffered,
                };

                var bidId = await _serviceBid.AddAsync(dto);

                // ── Datos del usuario que pujó ────────────────────────────────────
                var nombreUsuario = User.FindFirstValue(ClaimTypes.Name)
                    ?? User.FindFirstValue("CompleteName")
                    ?? $"User {viewModel.UserId}";

                var emailUsuario = User.FindFirstValue(ClaimTypes.Email) ?? "";

                // ── NOTIFICACIÓN 1: Nueva puja a todos los que ven la subasta ─────
                await _hubContext.Clients
                    .Group($"auction-{viewModel.AuctionId}")
                    .SendAsync("NuevaPujaSimulada", new
                    {
                        auctionId = viewModel.AuctionId,
                        nuevoMonto = viewModel.AmountOffered,
                        bidId = bidId,
                        userName = nombreUsuario,
                        userEmail = emailUsuario,
                        date = DateTime.Now.ToString("O")
                    });

                // Al usuario superado (si existe y es distinto al que puja)
                // outbidUserId es null si no había pujas previas.
                // El chequeo != viewModel.UserId evita notificarse a uno mismo.
                if (outbidUserId.HasValue && outbidUserId.Value != viewModel.UserId)
                {
                    await _hubContext.Clients
                        .Group($"user-{outbidUserId.Value}")
                        .SendAsync("UsuarioSuperado", new
                        {
                            auctionId = viewModel.AuctionId,
                            nuevoMonto = viewModel.AmountOffered,
                            userId = outbidUserId.Value,
                            mensaje = $"You have been outbid on \"{viewModel.TitleComicAuction}\". New bid: ${viewModel.AmountOffered:N2}"
                        });
                }

                // Al creador de la subasta (desde cualquier página) ──
                // auction.CreatorUserId es el dueño — recibe aviso aunque no esté en la página.
                // Solo si el creador no es el mismo que está pujando.
                if (auction.CreatorUserId != viewModel.UserId)
                {
                    await _hubContext.Clients
                        .Group($"user-{auction.CreatorUserId}")
                        .SendAsync("NuevaPujaEnTuSubasta", new
                        {
                            auctionId = viewModel.AuctionId,
                            nuevoMonto = viewModel.AmountOffered,
                            userName = nombreUsuario,
                            comicTitle = viewModel.TitleComicAuction,
                            mensaje = $"\"{viewModel.TitleComicAuction}\" has a new bid of ${viewModel.AmountOffered:N2} by {nombreUsuario}"
                        });
                }

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "¡Bid placed!",
                    $"Your bid of ${viewModel.AmountOffered:N2} was successfully registered.",
                    SweetAlertMessageType.success);

                return RedirectToAction(
                    "AuctionBiddingHistory",
                    "Bid",
                    new { auctionId = viewModel.AuctionId, comicTitle = viewModel.TitleComicAuction });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                if (viewModel.AuctionId > 0)
                    viewModel.Auction = await _serviceAuction.FindByIdAsync(viewModel.AuctionId) ?? new();

                TempData["ErrorMessage"] = "An error occurred while placing your bid. Please try again.";
                return View(viewModel);
            }
        }
    }
}