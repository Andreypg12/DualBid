using DualBid.Application.DTOs;
using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using DualBid.Hubs;
using DualBid.Services.BackgroundServices;
using DualBid.ViewModels.Auction;
using DualBid.ViewModels.Bid;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DualBid.Controllers
{
    public class AuctionController : Controller
    {
        private readonly IServiceAuction _serviceAuction;
        private readonly IServiceComic _serviceComic;
        private readonly IServiceAuctionState _serviceAuctionState;
        //private readonly IAuctionMonitorService _auctionMonitor;
        private readonly AuctionMonitorService _auctionMonitor;
        private readonly IHubContext<AuctionHub> _hubContext;

        public AuctionController(
            IServiceAuction serviceAuction,
            IServiceComic serviceComic,
            IServiceAuctionState serviceAuctionState,
            IHubContext<AuctionHub> hubContext,
            AuctionMonitorService auctionMonitor/*IAuctionMonitorService auctionMonitor*/
            )
        {
            _serviceAuction = serviceAuction;
            _serviceComic = serviceComic;
            _serviceAuctionState = serviceAuctionState;
            _hubContext = hubContext;
            _auctionMonitor = auctionMonitor;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string state = "active")
        {
            var all = await _serviceAuction.ListAsync();
            bool showActive = state != "inactive";

            var filtered = all.Where(a => showActive
                ? a.State.Id == 1 || a.State.Id == 2
                : a.State.Id == 3 || a.State.Id == 4);

            var vm = new AuctionIndexViewModel
            {
                SelectedState = showActive ? "active" : "inactive",
                Auctions = filtered
            };

            return View(vm);
        }

        public async Task<ActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                    throw new Exception("No auction ID provided");

                var auction = await _serviceAuction.FindByIdAsync(id.Value);

                if (auction == null)
                    throw new Exception("No auction found with the provided ID");

                // Si la subasta está activa y ya venció, el monitor debería haberla cerrado.
                // Este bloque es un fallback por si el monitor aún no corrió (ej: reinicio reciente).
                if (auction.StateId == 2 && auction.ExpectedEndDate <= DateTime.Now)
                {
                    var result = await _serviceAuction.CloseAuctionAsync(id.Value);

                    if (result != null)
                    {
                        // Quitar del monitor por si acaso aún estaba pendiente
                        _auctionMonitor.UnscheduleAuction(id.Value);

                        await NotifyAuctionClosedAsync(result);

                        // Recargar con datos actualizados
                        auction = await _serviceAuction.FindByIdAsync(id.Value);

                        if (auction == null)
                            throw new Exception("No auction found after closing");
                    }
                }

                return View(auction);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            CreateAuctionViewModel vm = await CreateAuctionCreateVM();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateAuctionViewModel pvm)
        {
            var comic = await _serviceComic.FindByIdAsync(pvm.Auction.ComicId);

            if (comic == null)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "Comic Not Found",
                    "The selected comic does not exist or has been deleted.",
                    SweetAlertMessageType.error);
                return View(await CreateAuctionCreateVM());
            }

            bool hasActiveAuction = comic.Auction.Any(a => a.State.Id == 1 || a.State.Id == 2);
            if (hasActiveAuction)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "¡Comic with active auction!",
                    "This comic already has an active or waiting auction",
                    SweetAlertMessageType.error);
                return View(await CreateAuctionCreateVM());
            }

            if (!comic.availability)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "¡Comic isn't available!",
                    "The comic isn't available for auction",
                    SweetAlertMessageType.error);
                return View(await CreateAuctionCreateVM());
            }

            var keysToRemove = ModelState.Keys
                .Where(k => k.StartsWith("Comics.") || k == "Comics" || k.Contains(".Comic."))
                .ToList();
            foreach (var key in keysToRemove)
                ModelState.Remove(key);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            pvm.Auction.CreatorUserId = userIdClaim != null ? int.Parse(userIdClaim) : 0;
            pvm.Auction.StateId = 1;

            if (pvm.Auction.CreatorUserId == 0)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "¡You need to log in!",
                    "You must be logged in to create an auction",
                    SweetAlertMessageType.error);
                return View(await CreateAuctionCreateVM());
            }

            if (!ModelState.IsValid)
            {
                var errores = string.Join("<br>", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "Errores de validación",
                    $"El formulario contiene errores:<br>{errores}",
                    SweetAlertMessageType.warning);
                return View(await CreateAuctionCreateVM());
            }

            await _serviceComic.UpdateAvailabilityAsync(pvm.Auction.ComicId, false);
            await _serviceAuction.AddAsync(pvm.Auction);

            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Auction created successfully",
                "The auction was registered successfully.",
                SweetAlertMessageType.success);

            return RedirectToAction(nameof(Index));
        }

        private async Task<CreateAuctionViewModel> CreateAuctionCreateVM()
        {
            int userIdClaim = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            return new CreateAuctionViewModel
            {
                Auction = new AuctionDTO(),
                Comics = await _serviceComic.ListComicsForAuctionByUserAsync(userIdClaim)
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, EditAuctionViewModel viewModel)
        {
            if (!validateAuctionDates(viewModel.StartDate, viewModel.ExpectedEndDate, out string errorMessage))
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Date validation errors", errorMessage, SweetAlertMessageType.warning);
                return RedirectToAction(nameof(Details), new { id });
            }

            if (!ModelState.IsValid)
            {
                var errores = string.Join("<br>", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Errors in the data", errores, SweetAlertMessageType.warning);
                return RedirectToAction(nameof(Details), new { id });
            }

            var existingDto = await _serviceAuction.FindByIdAsync(id);

            if (existingDto == null)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction Not Found", "The Auction does not exist or has been deleted.", SweetAlertMessageType.error);
                return RedirectToAction(nameof(Index));
            }

            if (existingDto.State.Id != 1)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction not editable", "The Auction must be waiting if you want it edited", SweetAlertMessageType.error);
                return RedirectToAction(nameof(Details), new { id });
            }

            if (existingDto.Bids.Count >= 1)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction with bids", "The Auction cannot be edited because it has bids", SweetAlertMessageType.error);
                return RedirectToAction(nameof(Details), new { id });
            }

            existingDto.StartDate = viewModel.StartDate;
            existingDto.ExpectedEndDate = viewModel.ExpectedEndDate;
            existingDto.BasePrice = viewModel.BasePrice;
            existingDto.MinimunIncrease = viewModel.MinimunIncrease;

            await _serviceAuction.UpdateAsync(id, existingDto);

            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Auction updated",
                $"The auction {existingDto.Comic.Title} has been successfully modified",
                SweetAlertMessageType.success);

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool validateAuctionDates(DateTime startDate, DateTime expectedEndDate, out string errorMessage)
        {
            if (startDate >= expectedEndDate)
            {
                errorMessage = "The start date must be earlier than the end date";
                return false;
            }
            if (startDate < DateTime.Now || expectedEndDate < DateTime.Now)
            {
                errorMessage = "The start date and expected end date must be in the future";
                return false;
            }
            errorMessage = "";
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActivateAuction(int id)
        {
            try
            {
                var auction = await _serviceAuction.FindByIdAsync(id);

                if (auction == null)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Error", "Auction not found", SweetAlertMessageType.error);
                    return RedirectToAction(nameof(Details), new { id });
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                if (auction.CreatorUser.Id != userId)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Unauthorized", "You are not authorized to activate this auction", SweetAlertMessageType.error);
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (auction.State.Id != 1)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Invalid Action", "This auction cannot be activated in its current state", SweetAlertMessageType.warning);
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (auction.StartDate > DateTime.Now)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Cannot Activate",
                        "The auction start date is in the future. You can only activate it when the start date arrives.",
                        SweetAlertMessageType.warning);
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _serviceAuction.UpdateStateAsync(id, 2);

                // ✅ NUEVO: Registrar en el monitor para cierre automático
                _auctionMonitor.ScheduleAuction(id, auction.ExpectedEndDate);

                // ✅ NUEVO: Notificar via SignalR que la subasta ya está activa
                await _hubContext.Clients
                    .Group($"auction-{id}")
                    .SendAsync("AuctionActivated", new
                    {
                        auctionId = id,
                        message = $"The auction is now live! Ends: {auction.ExpectedEndDate:g}"
                    });

                await _hubContext.Clients
                    .Group($"user-{auction.CreatorUserId}")
                    .SendAsync("YourAuctionActivated", new
                    {
                        auctionId = id,
                        message = "Your auction is now active!"
                    });

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction Activated", "The auction has been successfully activated", SweetAlertMessageType.success);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Error", $"An error occurred while activating the auction: {ex.Message}", SweetAlertMessageType.error);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseAuction(int id)
        {
            try
            {
                var auction = await _serviceAuction.FindByIdAsync(id);

                if (auction == null)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Error", "Auction not found", SweetAlertMessageType.error);
                    return RedirectToAction(nameof(Details), new { id });
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                if (auction.CreatorUser.Id != userId)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Unauthorized", "You are not authorized to close this auction", SweetAlertMessageType.error);
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (auction.State.Id != 1 && auction.State.Id != 2)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Invalid Action", "This auction cannot be closed in its current state", SweetAlertMessageType.warning);
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (auction.Bids != null && auction.Bids.Any())
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Cannot Close", "This auction cannot be closed because it already has bids", SweetAlertMessageType.warning);
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _serviceComic.UpdateAvailabilityAsync(auction.Comic.Id, true);
                await _serviceAuction.UpdateStateAsync(id, 4);

                //Quitar del monitor (fue cancelada manualmente, no debe cerrarse dos veces)
                _auctionMonitor.UnscheduleAuction(id);


                    await _hubContext.Clients
                    .Group($"auction-{id}")
                    .SendAsync("AuctionClosed", new
                    {
                        auctionId = id,
                        message = "The auction has been cancelled by the owner.",
                        hasBids = false,
                        winnerUserId = (int?)null,
                        winnerName = (string?)null,
                        finalAmount = 0m,
                        finalState = 4
                    });

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction Closed", "The auction has been successfully cancelled", SweetAlertMessageType.success);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Error", $"An error occurred while closing the auction: {ex.Message}", SweetAlertMessageType.error);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

      
        private async Task NotifyAuctionClosedAsync(AuctionCloseResultDTO result)
        {
            // 1. Todos los espectadores de la subasta
            await _hubContext.Clients
                .Group($"auction-{result.AuctionId}")
                .SendAsync("AuctionClosed", new
                {
                    auctionId = result.AuctionId,
                    message = result.WinnerName != null
                                     ? $"Auction ended! Winner: {result.WinnerName} with ${result.FinalAmount:N2}"
                                     : "Auction ended with no bids.",
                    hasBids = result.WinnerUserId.HasValue,
                    winnerUserId = result.WinnerUserId,
                    winnerName = result.WinnerName,
                    finalAmount = result.FinalAmount,
                    finalState = result.FinalStateId
                });

            // 2. El ganador (si hay)
            if (result.WinnerUserId.HasValue)
            {
                await _hubContext.Clients
                    .Group($"user-{result.WinnerUserId}")
                    .SendAsync("YouWonAuction", new
                    {
                        auctionId = result.AuctionId,
                        message = $"Congratulations! You won the auction for ${result.FinalAmount:N2}",
                        finalAmount = result.FinalAmount,
                        comicTitle = result.ComicTitle
                    });
            }

            // 3. El creador de la subasta
            if (result.OwnerUserId.HasValue)
            {
                await _hubContext.Clients
                    .Group($"user-{result.OwnerUserId}")
                    .SendAsync("YourAuctionEnded", new
                    {
                        auctionId = result.AuctionId,
                        message = result.WinnerName != null
                                        ? $"Your auction ended. Winner: {result.WinnerName} — ${result.FinalAmount:N2}"
                                        : "Your auction ended with no bids.",
                        hasBids = result.WinnerUserId.HasValue,
                        finalAmount = result.FinalAmount
                    });
            }
        }

        // Cuando el ganador paga: notifica a todos el recibo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotifyPaymentComplete([FromBody] PaymentRequest request)
        {
            if (request == null || request.AuctionId <= 0) return BadRequest();

            var auction = await _serviceAuction.FindByIdAsync(request.AuctionId);
            if (auction == null) return NotFound();

            
            await _serviceAuction.UpdateStateAsync(request.AuctionId, 3);

            await _hubContext.Clients
                .Group($"auction-{request.AuctionId}")
                .SendAsync("PaymentCompleted", new
                {
                    auctionId = request.AuctionId,
                    winnerName = auction.WinningBid?.User?.CompleteName,
                    finalAmount = auction.WinningBid?.AmountOffered,
                    comicTitle = auction.Comic?.Title,
                    date = DateTime.Now.ToString("g")
                });

            return Ok(new { success = true });
        }


        // Cuando el ganador libera: cancela la subasta y notifica a todos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAfterWin([FromBody] PaymentRequest request)
        {
            if (request == null || request.AuctionId <= 0) return BadRequest();

            var auction = await _serviceAuction.FindByIdAsync(request.AuctionId);
            if (auction == null) return NotFound();

            // Cambiar estado a Cancelada (4) y liberar el cómic
            await _serviceAuction.UpdateStateAsync(request.AuctionId, 4);
            await _serviceComic.UpdateAvailabilityAsync(auction.Comic.Id, true);

            // Notificar a todos que el cómic fue liberado
            await _hubContext.Clients
                .Group($"auction-{request.AuctionId}")
                .SendAsync("ComicReleased", new
                {
                    auctionId = request.AuctionId,
                    comicTitle = auction.Comic?.Title,
                    message = "The winner did not complete the payment. The comic has been returned to auction."
                });

            return Ok(new { success = true });
        }

        public class PaymentRequest { public int AuctionId { get; set; } }
    }
}