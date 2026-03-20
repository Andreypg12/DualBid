using DualBid.Application.DTOs;
using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
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

            ViewBag.ListCategorias = new MultiSelectList(
                items: categorias,
                dataValueField: nameof(CategoryDTO.Id),    
                dataTextField: nameof(CategoryDTO.Description), 
                selectedValues: selectedCategoriaIds
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
        public async Task<IActionResult> Create(ComicDTO dto, List<IFormFile> imageFile, string[] selectedCategorias)
        {
            selectedCategorias ??= Array.Empty<string>();



            //AGREGAR LAS VALIDACIONES
           

            // Si se envia imagen, convertirla a byte[]
            if (imageFile != null && imageFile.Count > 0)
            {
                dto.ImgComic = new List<ImgComicDTO>();

                foreach (var file in imageFile)
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
            if (comic == null)
            {
                TempData["SwalMessage"] = "Comic not found";
                TempData["SwalIcon"] = "error";
                return RedirectToAction(nameof(Index));
            }

            await _serviceComic.UpdateAvailabilityAsync(id, false);

            TempData["SwalMessage"] = "Comic deleted successfully";
            TempData["SwalIcon"] = "success";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var comic = await _serviceComic.FindByIdAsync(id);
            if (comic == null)
            {
                TempData["SwalMessage"] = "Comic not found";
                TempData["SwalIcon"] = "error";
                return RedirectToAction(nameof(Index));
            }

            await _serviceComic.UpdateAvailabilityAsync(id, true);

            TempData["SwalMessage"] = "Comic restored successfully";
            TempData["SwalIcon"] = "success";

            return RedirectToAction(nameof(Index));
        }

    }
    }

    



