using DualBid.Application.DTOs;
using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
using Humanizer;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DualBid.Controllers
{
    public class ComicController : Controller
    {
        /*Son a todas las tablas a las cuales debo acceder*/
        private readonly IServiceComic _serviceComic;
        private readonly IServicePublisher _servicePublisher;
        private readonly IServiceCategory _serviceCategoria;
        private readonly IServiceStateConservation _serviceStateConservation;

        public ComicController(IServiceComic serviceComic, IServicePublisher servicePublisher, IServiceCategory serviceCategory, IServiceStateConservation serviceStateConservation)
        {
            _serviceComic = serviceComic;
            _servicePublisher = servicePublisher;
            _serviceCategoria = serviceCategory;
            _serviceStateConservation = serviceStateConservation;
        }

        //Esta por decirlo asi es la página principal donde mustran la lista de comics
        [HttpGet]
        public async Task<IActionResult> Index(string availability = "available")
        {

            var collection = await _serviceComic.ListAsync();

            bool showAvailable = availability != "unavailable";
            var filtered = collection.Where(c => showAvailable ? c.availability : !c.availability).ToList();

            ViewBag.SelectedAvailability = availability;
            return View(filtered);
        }

        //Esto es para mostrar el detalle de un comic
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var comic = await _serviceComic.FindByIdAsync(id);
            if (comic == null) return NotFound();

            return View(comic);
        }


        //Esto es para cargar todos los componentes con los datos correspondientes.
        private async Task LoadCombosAsync(IEnumerable<string>? selectedCategoriaIds = null)
        {
            // Publisher
            var publishers = await _servicePublisher.ListAsync();
            ViewBag.ListPublisher = new SelectList(publishers, "Id", "Description");

            // StateConservation 
            var states = await _serviceStateConservation.ListAsync();
            ViewBag.ListStateConservation = new SelectList(states, "Id", "Description");

            // Categorías
            var categorias = await _serviceCategoria.ListAsync();


            var selectedIds = selectedCategoriaIds?
            .Select(id => int.Parse(id))
            .ToList();

            ViewBag.ListCategorias = new MultiSelectList(
                items: categorias,
                dataValueField: nameof(CategoryDTO.Id),    
                dataTextField: nameof(CategoryDTO.Description), 
                selectedValues: selectedIds
            );
        }



        // GET: LibroController/Create
        public async Task<IActionResult> Create()
        {
            await LoadCombosAsync(Array.Empty<string>());
            return View(new ComicDTO());
        }


        // POST: LibroController/Create
        // Cuando se aplica el POST llena el Dto con los datos del formulario y hace validaciones en base a las validaciones del DTO y guarda los errores en ModelState
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComicDTO dto, List<IFormFile> newImages, string[] selectedCategorias, bool availability)
        {
            var hoy = System.DateTime.Now.Date;
            selectedCategorias ??= Array.Empty<string>();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            dto.SellerId = userIdClaim != null ? int.Parse(userIdClaim) : 0;

            //Agrega la disponibilidad al DTO
            dto.availability = availability;

            //AGREGAR LAS VALIDACIONES

            //Valida que la fecha propuesta no sea ni mayor ni menor a la de hoy
            if (dto.CreationDate.Date != hoy || dto.CreationDate.Date != hoy)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                   "¡Date incorrect!",
                   "You should choose the correct date",
                   SweetAlertMessageType.error
                   );
                await LoadCombosAsync(selectedCategorias);
                return View(dto);
            }

            //Valida si hay un usuario seleccionado
            if (dto.SellerId == 0)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "¡You need to log in!",
                    "You must be logged in to create a comic",
                    SweetAlertMessageType.error
                    );
                await LoadCombosAsync(selectedCategorias);
                return View(dto);
            }

            //Categoria no puede estar vacío.
            if (!selectedCategorias.Any())
            {
                //ModelState.AddModelError("SelectedCategorias", "You must select at least one category.");
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "¡Categories is not enough!",
                    "You must select at least one Category",
                    SweetAlertMessageType.error
                    );
                await LoadCombosAsync(selectedCategorias);
                return View(dto);
            }

            //Imagenes no puede estar vacío.
            if (newImages == null || !newImages.Any())
            {
                //ModelState.AddModelError("ImageFiles", "You must upload at least one image.");
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "¡Images is not enough!",
                    "You must upload at least one Image",
                    SweetAlertMessageType.error
                    );
                await LoadCombosAsync(selectedCategorias);
                return View(dto);
            }

            

            //Terminan las validaciones



            // Si se envia imagen, convertirla a byte[]
            if (newImages != null && newImages.Count > 0)
            {
                dto.ImgComic = new List<ImgComicDTO>();

                foreach (var file in newImages)
                {
                    if (file == null || file.Length == 0) continue;

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    dto.ImgComic.Add(new ImgComicDTO
                    {
                        Img = ms.ToArray()
                    });
                }


                ModelState.Remove("imageFile");
                ModelState.Remove("ImgComic");
            }


            // Estas 2 desactivan el problema sin cambiar DTO ni vista
            ModelState.Remove("Publisher.Description");
            ModelState.Remove("StateConservation.Description");

            // Si el ModelState no es válido, recopilar errores y mostrar notificación
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
                await LoadCombosAsync(selectedCategorias);
                return View(dto);
            }


            await _serviceComic.AddAsync(dto, selectedCategorias);

            //Notificar creación
            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
               "Libro creado correctamente",
               $"El libro {dto.Title} fue registrado exitosamente.",
               SweetAlertMessageType.success
           );

            // Redirigir a Index después de la creación
            return RedirectToAction(nameof(Index));
        }




        //Este es el metodo que carga todo los datos necesarios dentro de la vista de edición, como el publisher, las categorias, etc
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _serviceComic.FindByIdAsync(id);
            if (dto == null) return NotFound();

            var selected = dto.Category
                .Select(c => c.Id.ToString())
                .ToList();

            await LoadCombosAsync(selected);

            return View(dto);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ComicDTO dto,List<IFormFile> newImages,string[] selectedCategorias,int[] ImagesToDelete)
        
        {
            
            bool BloqueoSubasta = dto.Auction != null &&
                          dto.Auction.Any(a =>
                              a.StateId == 2 || a.StateId == 3);


            if (BloqueoSubasta || dto.availability == false)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                   "¡The comic cannot be edited!",
                   "The comic is already in an active auction or has already been auctioned.",
                   SweetAlertMessageType.error
                   );
                await LoadCombosAsync();
                return View(dto);
            }

            selectedCategorias ??= Array.Empty<string>();
            ImagesToDelete ??= Array.Empty<int>();

            var nuevasImagenes = new List<ImgComicDTO>();

            if (newImages != null && newImages.Count > 0)
            {
                foreach (var file in newImages)
                {
                    if (file == null || file.Length == 0) continue;

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    nuevasImagenes.Add(new ImgComicDTO
                    {
                        Img = ms.ToArray()
                    });
                }
            }

            ModelState.Remove("Publisher.Description");
            ModelState.Remove("StateConservation.Description");
            if (!ModelState.IsValid)
            {
                await LoadCombosAsync(selectedCategorias);
                return View(dto);
            }

            await _serviceComic.UpdateAsync(
                dto,
                selectedCategorias,
                nuevasImagenes,
                ImagesToDelete
            );

            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Comic actualizado",
                $"El cómic {dto.Title} fue actualizado correctamente.",
                SweetAlertMessageType.success
            );

            return RedirectToAction(nameof(Index));
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comic = await _serviceComic.FindByIdAsync(id);
            bool hasBlockedAuction = comic.Auction != null &&
                          comic.Auction.Any(a =>
                              a.StateId == 2 || a.StateId == 3);


            if (comic == null)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                   "¡Comic not available!",
                   "Was an error with this comic",
                   SweetAlertMessageType.error
                   );

                return RedirectToAction(nameof(Index));
            }

            if (hasBlockedAuction || comic.availability == false)
            {

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                   "¡Error deleting the comic!",
                   "The comic has been auctioned or is part of an active auction.",
                   SweetAlertMessageType.error
                   );

                return RedirectToAction(nameof(Index));
            }

            await _serviceComic.UpdateAvailabilityAsync(id, false);

            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                   "Process complete",
                   "Comic deleted successfully",
                   SweetAlertMessageType.success
                   );

            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var comic = await _serviceComic.FindByIdAsync(id);
            bool hasBlockedAuction = comic.Auction != null &&
                          comic.Auction.Any(a =>
                              a.StateId == 2 || a.StateId == 3);

            if (comic == null)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
               "Error",
               $"The cómic was not found",
               SweetAlertMessageType.success
           );
                return RedirectToAction(nameof(Index));
            }
            if (hasBlockedAuction)
            {

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                   "¡Error restoring the comic!",
                   "The comic has been auctioned or is part of an active auction.",
                   SweetAlertMessageType.error
                   );

                return RedirectToAction(nameof(Index));
            }

            await _serviceComic.UpdateAvailabilityAsync(id, true);

            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
               "Comic restored",
               $"The cómic {comic.Title} was restored succesfully.",
               SweetAlertMessageType.success
           );

            return RedirectToAction(nameof(Index));
        }

    }
    }

    



