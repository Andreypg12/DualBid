using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using DualBid.ViewModels.Auction;
using DualBid.ViewModels.Bid;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DualBid.Controllers
{
    public class AuctionController : Controller
    {
        private readonly IServiceAuction _serviceAuction;
        private readonly ICurrentUserService _currentUserService;

        public AuctionController(IServiceAuction serviceAuction, ICurrentUserService currentUserService)
        {
            _serviceAuction = serviceAuction;
            _currentUserService = currentUserService;

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
    }
}
