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
            await LoadCombosAsync();

            return View(new AuctionDTO());
        }

        // POST: LibroController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AuctionDTO dto)
        {
            var keysToRemove = ModelState.Keys
            .Where(k => k.StartsWith("Comic.") || k == "Comic")
            .ToList();

            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            dto.CreatorUserId = userIdClaim != null ? int.Parse(userIdClaim) : 0;

            dto.StateId = (dto.StartDate < DateTime.Now) ? 1 : 2;

            // Verificar si el usuario está logueado
            if (dto.CreatorUserId == 0)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
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
                    "Errores de validación",
                    $"El formulario contiene errores:<br>{errores}",
                    SweetAlertMessageType.warning
                );
                // Importante: Recargar combos antes de retornar vista
                await LoadCombosAsync();
                return View(dto);
            }

            // Si todo está bien, guardar
            await _serviceAuction.AddAsync(dto);

            // Notificar creación
            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Auction created successfully",
                $"The auction was registered successfully.",
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
