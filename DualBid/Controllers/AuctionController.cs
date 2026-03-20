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
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceComic _serviceComic;
        private readonly IServiceAuctionState _serviceAuctionState;

        public AuctionController(IServiceAuction serviceAuction, ICurrentUserService currentUserService, IServiceComic serviceComic, IServiceAuctionState serviceAuctionState)
        {
            _serviceAuction = serviceAuction;
            _currentUserService = currentUserService;
            _serviceComic = serviceComic;
            _serviceAuctionState = serviceAuctionState;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string state = "active")
        {
            var all = await _serviceAuction.ListAsync();
            bool showActive = state != "inactive";

            var filtered = all.Where(a => showActive ? a.State.Id == 1 : a.State.Id != 1);

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
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                       "Libro No encontrado",
                       $"No existe un Libro sin ID",
                       SweetAlertMessageType.error
                   );
                    return RedirectToAction("IndexAdmin");
                }
                var @object = await _serviceAuction.FindByIdAsync(id.Value);
                if (@object == null)
                {
                    throw new Exception("Libro no existente");

                }

                var currentUserId = _currentUserService.GetCurrentUserId();
                ViewBag.CurrentUserId = currentUserId;

                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                   "Detalle del Libro",
                   $"Mostrando información del Libro: {@object.Comic.Title}",
                   SweetAlertMessageType.info
                );

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
            await LoadCombosAsync();

            return View(new AuctionDTO());
        }

        // POST: LibroController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AuctionDTO dto)
        {
            // IMPORTANTE: Asignar el CreatorUserId antes de validar
            dto.CreatorUserId = _currentUserService.GetCurrentUserId() ?? 0;

            dto.StateId = (dto.StartDate < DateTime.Now) ? 1 : 2;

            // Verificar si el usuario está logueado
            if (dto.CreatorUserId == 0)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "¡You need to log in!",
                    "You must be logged in to create an auction",
                    SweetAlertMessageType.error
                );
                await LoadCombosAsync();
                return View(dto);
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
                    "Validation errors",
                    $"The form contains errors:<br>{errores}",
                    SweetAlertMessageType.warning
                );

                // Recargar combos antes de retornar vista
                await LoadCombosAsync();
                return View(dto);
            }

            // Si todo está bien, guardar
            await _serviceAuction.AddAsync(dto);

            // Notificar creación
            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Auction created successfully",
                $"The auction for {dto.Comic?.Title ?? "comic"} was registered successfully.",
                SweetAlertMessageType.success
            );

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCombosAsync()
        {
            ViewBag.ListComics = await _serviceComic.ListAsync();
        }
    }
}
