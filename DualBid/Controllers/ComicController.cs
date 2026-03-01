using System.Security.Claims;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DualBid.Controllers
{
    public class ComicController : Controller
    {
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collection = await _serviceComic.ListAsync();
            return View(collection);
        }

        //Esto es lo que hace la comunicacion entre una vista y la otra
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var comic = await _serviceComic.FindByIdAsync(id);
            if (comic == null) return NotFound();

            return View(comic);
        }


        private async Task LoadCombosAsync(IEnumerable<string>? selectedCategoriaIds = null)
        {
            // Publisher
            var publishers = await _servicePublisher.ListAsync();
            ViewBag.ListPublisher = new SelectList(publishers, "Id", "Description"); // ← AHORA SÍ

            // StateConservation 
            var states = await _serviceStateConservation.ListAsync();
            ViewBag.ListStateConservation = new SelectList(states, "Id", "Description"); // ← AHORA SÍ




            // Categorías (many-to-many)
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



            // Validación de categorías 
            if (selectedCategorias.Length == 0)
            {
                ModelState.AddModelError("Category", "You must select at least one category.");
            }

            // Imagen requerida en Create
            if (dto.ImgComic == null && imageFile == null)
            {
                ModelState.AddModelError("Imagen", "Debe seleccionar una imagen.");
            }

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

                // Si tenías un error duplicado por "Imagen", elimínalo usando la key correcta
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


    }


}
