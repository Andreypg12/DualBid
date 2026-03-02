using DualBid.Application.Services.Interfaces;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace Libreria.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IserviceUser _serviceUser;

        public UserController(IserviceUser serviceAutor)
        {
            _serviceUser = serviceAutor;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page)
        {
            var collection = await _serviceUser.ListAsync();

            int pageNumber = page ?? 1;

            int pageSize = 1;

            return View(collection.ToPagedList(pageNumber, pageSize));
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
                var @object = await _serviceUser.FindByIdAsync(id.Value);
                if (@object == null)
                {
                    throw new Exception("Libro no existente");

                }
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                   "Detalle del Libro",
                   $"Mostrando información del Libro: {@object.CompleteName}",
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

