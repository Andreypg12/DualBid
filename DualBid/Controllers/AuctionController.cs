using DualBid.Application.DTOs;
using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using DualBid.ViewModels.Auction;
using DualBid.ViewModels.Bid;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace DualBid.Controllers
{
    public class AuctionController : Controller
    {
        private readonly IServiceAuction _serviceAuction;
        private readonly IServiceComic _serviceComic;
        private readonly IServiceAuctionState _serviceAuctionState;

        public AuctionController(IServiceAuction serviceAuction, IServiceComic serviceComic, IServiceAuctionState serviceAuctionState)
        {
            _serviceAuction = serviceAuction;
            _serviceComic = serviceComic;
            _serviceAuctionState = serviceAuctionState;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string state = "active")
        {
            var all = await _serviceAuction.ListAsync();
            bool showActive = state != "inactive";

            var filtered = all.Where(a => showActive ? a.State.Id == 1 || a.State.Id == 2 : a.State.Id == 3 || a.State.Id == 4);

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
                {
                    throw new Exception("No comic ID provided");

                }
                var @object = await _serviceAuction.FindByIdAsync(id.Value);

                if (@object == null)
                {
                    throw new Exception("No comic found with the provided ID");
                }

                return View(@object);

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

        // POST: LibroController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateAuctionViewModel pvm)
        {
            //agarro el comic
            var comic = await _serviceComic.FindByIdAsync(pvm.Auction.ComicId);

            //Valido que exista
            if (comic == null)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "Comic Not Found",
                    "The selected comic does not exist or has been deleted.",
                    SweetAlertMessageType.error
                );
                CreateAuctionViewModel vm = await CreateAuctionCreateVM();
                return View(vm);
            }

            //Verifico que no tenga ninguna subasta activa ligada
            bool hasActiveAuction = comic.Auction.Any(a => a.State.Id == 1 || a.State.Id == 2);

            //Si tiene me tira error
            if (hasActiveAuction)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "¡Comic with active auction!",
                    "This comic alredy has an active or waiting auction",
                    SweetAlertMessageType.error
                );
                CreateAuctionViewModel vm = await CreateAuctionCreateVM();

                return View(vm);
            }

            //Verifico que el comic este disponible para subastar y si no tiro error
            if (!comic.availability)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "¡Comic isn't avaible!",
                    "The comic isn't avaible for auction",
                    SweetAlertMessageType.error
                );
                CreateAuctionViewModel vm = await CreateAuctionCreateVM();

                return View(vm);
            }

            //Le quito lo de los comics para que no me de error
            var keysToRemove = ModelState.Keys
                .Where(k => k.StartsWith("Comics.") ||
                k == "Comics" ||
                k.Contains(".Comic.")) // Para propiedades anidadas
            .ToList();

            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            //Verifico que la persona que lo crea este logueada
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            pvm.Auction.CreatorUserId = userIdClaim != null ? int.Parse(userIdClaim) : 0;

            pvm.Auction.StateId = 1;

            // Verificar si el usuario está logueado
            if (pvm.Auction.CreatorUserId == 0)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "¡You need to log in!",
                    "You must be logged in to create an auction",
                    SweetAlertMessageType.error
                );
                CreateAuctionViewModel vm = await CreateAuctionCreateVM();

                return View(vm);
            }



            // Verificar si el modelo es válido (esto ejecuta las validaciones del DTO automáticamente)
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
                // Importante: Recargar combos antes de retornar vista
                CreateAuctionViewModel vm = await CreateAuctionCreateVM();

                return View(vm);
            }

            //Le cambio la disponibilidad al objeto a no disponible
            await _serviceComic.UpdateAvailabilityAsync(pvm.Auction.ComicId, false);
            
            // Si todo está bien, guardar
            await _serviceAuction.AddAsync(pvm.Auction);

            // Notificar creación
            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Auction created successfully",
                $"The auction was registered successfully.",
                SweetAlertMessageType.success
            );

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

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, EditAuctionViewModel viewModel)
        {



            String errorMessage;
            if (!validateAuctionDates(viewModel.StartDate, viewModel.ExpectedEndDate, out errorMessage))
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Date validation errors",
                    errorMessage,
                    SweetAlertMessageType.warning
                );

                return RedirectToAction(nameof(Details), new { id });
            }

            if (!ModelState.IsValid)
            {
                var errores = string.Join("<br>",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                );

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Errors in the data",
                    errores,
                    SweetAlertMessageType.warning
                );

                return RedirectToAction(nameof(Details), new { id });
            }
                
            // Obtener el DTO existente
            var existingDto = await _serviceAuction.FindByIdAsync(id);

            if (existingDto == null) {

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction Not Found",
                    "The Auction does not exist or has been deleted.",
                    SweetAlertMessageType.error
                );
                return RedirectToAction(nameof(Index));
            }
            else if (existingDto.State.Id != 1)
            {

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction not editable",
                    "The Auction must be waiting if you want it edited",
                    SweetAlertMessageType.error
                );
                return RedirectToAction(nameof(Details), new { id });
            }
            else if (existingDto.Bids.Count >= 1)
            {

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction with bids",
                    "The Auction cannot be edited because it has bids",
                    SweetAlertMessageType.error
                );
                return RedirectToAction(nameof(Details), new { id });
            }

            // Actualizar solo los campos editables
            existingDto.StartDate = viewModel.StartDate;
            existingDto.ExpectedEndDate = viewModel.ExpectedEndDate;
            existingDto.BasePrice = viewModel.BasePrice;
            existingDto.MinimunIncrease = viewModel.MinimunIncrease;

            await _serviceAuction.UpdateAsync(id, existingDto);

            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Auction updated",
                $"The auction {existingDto.Comic.Title} has been successfully modified",
                SweetAlertMessageType.success
            );

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool validateAuctionDates(DateTime startDate, DateTime expectedEndDate, out String errorMessage)
        {
            if (startDate >= expectedEndDate)
            {
                errorMessage = "The star date must be earlier than the en date";
                return false;
            }
            else if (startDate < DateTime.Now || expectedEndDate < DateTime.Now)
            {
                errorMessage = "The start date and expected end date must be in the future";
                return false;
            }
            else
            {
                errorMessage = "";
                return true;
            }
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
                        "Error",
                        "Auction not found",
                        SweetAlertMessageType.error
                    );
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Verificar que el usuario sea el creador
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                if (auction.CreatorUser.Id != userId)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Unauthorized",
                        "You are not authorized to activate this auction",
                        SweetAlertMessageType.error
                    );
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Verificar que la subasta esté en estado 1 (pendiente)
                if (auction.State.Id != 1)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Invalid Action",
                        "This auction cannot be activated in its current state",
                        SweetAlertMessageType.warning
                    );
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Verificar que la fecha de inicio no haya pasado
                if (auction.StartDate > DateTime.Now)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Cannot Activate",
                        "The auction start date is in the future. You can only activate it when the start date arrives.",
                        SweetAlertMessageType.warning
                    );
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _serviceAuction.UpdateStateAsync(id, 2);

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction Activated",
                    "The auction has been successfully activated",
                    SweetAlertMessageType.success
                );

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Error",
                    $"An error occurred while activating the auction: {ex.Message}",
                    SweetAlertMessageType.error
                );
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
                        "Error",
                        "Auction not found",
                        SweetAlertMessageType.error
                    );
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Verificar que el usuario sea el creador
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (auction.CreatorUser.Id != userId)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Unauthorized",
                        "You are not authorized to close this auction",
                        SweetAlertMessageType.error
                    );
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Verificar que la subasta esté en estado 1 o 2
                if (auction.State.Id != 1 && auction.State.Id != 2)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Invalid Action",
                        "This auction cannot be closed in its current state",
                        SweetAlertMessageType.warning
                    );
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Verificar que no tenga pujas (cancelar solo si no hay pujas)
                if (auction.Bids != null && auction.Bids.Any())
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Cannot Close",
                        "This auction cannot be closed because it already has bids",
                        SweetAlertMessageType.warning
                    );
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _serviceComic.UpdateAvailabilityAsync(auction.Comic.Id, true);

                // Aqui se hace el update
                await _serviceAuction.UpdateStateAsync(id, 4);

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Auction Closed",
                    "The auction has been successfully closed/cancelled",
                    SweetAlertMessageType.success
                );

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Error",
                    $"An error occurred while closing the auction: {ex.Message}",
                    SweetAlertMessageType.error
                );
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
