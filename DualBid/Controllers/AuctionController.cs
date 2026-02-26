using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;

namespace DualBid.Controllers
{
    public class AuctionController : Controller
    {
        private readonly IServiceAuction _serviceAuction;

        public AuctionController(IServiceAuction serviceAuction)
        {
            _serviceAuction = serviceAuction;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collection = await _serviceAuction.ListAsync();
            return View(collection);
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
