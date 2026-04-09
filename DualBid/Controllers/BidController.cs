using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Hubs;
using DualBid.ViewModels.Bid;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Identity;
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
                // Validar que llegó el AuctionId
                if (viewModel.AuctionId == 0)
                {
                    TempData["ErrorMessage"] = "Invalid auction";
                    return RedirectToAction("Index", "Auction");
                }

                // Recargar la subasta usando AuctionId
                var auction = await _serviceAuction.FindByIdAsync(viewModel.AuctionId);

                if (auction == null)
                {
                    TempData["ErrorMessage"] = "Auction not found";
                    return RedirectToAction("Index", "Auction");
                }

                // Asignar la subasta al viewModel para la vista
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
                    SweetAlertMessageType.error
                );

                    return View(viewModel);
                }

                // Validar el incremento mínimo
                var currentBid = auction.CurrentBid?.AmountOffered ?? auction.BasePrice;
                var minimumRequired = currentBid + auction.MinimunIncrease;

                if (viewModel.AmountOffered < minimumRequired)
                {
                    ModelState.AddModelError("AmountOffered",
                        $"The bid must be at least ${minimumRequired:N2}");

                    TempData["ErrorMessage"] = $"Invalid amount. Minimum required is ${minimumRequired:N2}";
                    return View(viewModel);
                }

                // Crear el DTO
                BidDTO dto = new()
                {
                    AuctionId = viewModel.AuctionId, // Usar AuctionId
                    UserId = viewModel.UserId,
                    AmountOffered = viewModel.AmountOffered,
                };

                var bidId = await _serviceBid.AddAsync(dto);

                // SignalR notifications

                var nombreUsuario = User.FindFirstValue(ClaimTypes.Name) ??
                    User.FindFirstValue("CompleteName") ??
                    $"Usuario {viewModel.UserId}";

                var emailUsuario = User.FindFirstValue(ClaimTypes.Email) ?? "";

                // SignalR notifications
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

                // Enviar al usuario que fue superado
                if (viewModel.OutBidUserId > 0)
                {
                    await _hubContext.Clients
                        .Group($"user-{viewModel.OutBidUserId}")
                        .SendAsync("UsuarioSuperado", new
                        {
                            auctionId = viewModel.AuctionId,
                            nuevoMonto = viewModel.AmountOffered,
                            userId = viewModel.OutBidUserId,
                            mensaje = $"Has sido superado en la subasta {viewModel.TitleComicAuction}. Nueva puja: ${viewModel.AmountOffered:F2}"
                        });
                }

                await _hubContext.Clients
                    .Group($"user-{viewModel.OutBidUserId}")
                    .SendAsync("UsuarioSuperado", new
                    {
                        auctionId = viewModel.Auction.Id,
                        nuevoMonto = viewModel.AmountOffered,
                        userId = viewModel.OutBidUserId, // Agrega esto
                        mensaje = $"Has sido superado en la subasta {viewModel.TitleComicAuction}. Nueva puja: ${viewModel.AmountOffered:F2}"
                    });

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "¡Puja registrada!",
                    $"Tu puja de ${viewModel.AmountOffered:N2} fue registrada exitosamente.",
                    SweetAlertMessageType.success
                );

                return RedirectToAction(
                    "AuctionBiddingHistory",
                    "Bid",
                    new
                    {
                        auctionId = viewModel.AuctionId,
                        comicTitle = viewModel.TitleComicAuction
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                // Recargar la subasta si es posible
                if (viewModel.AuctionId > 0)
                {
                    viewModel.Auction = await _serviceAuction.FindByIdAsync(viewModel.AuctionId) ?? new();
                }

                TempData["ErrorMessage"] = "An error occurred while placing your bid. Please try again.";
                return View(viewModel);
            }
        }

    }
}