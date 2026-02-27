using DualBid.Application.DTOs;
using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DualBid.Controllers
{
    public class ComicController : Controller
    {
        private readonly IServiceComic _serviceComic;
        private readonly IServicePublisher _servicePublisher;
        private readonly IServiceCategory _serviceCategoria;

        public ComicController(IServiceComic serviceComic, IServicePublisher servicePublisher, IServiceCategory serviceCategory)
        {
            _serviceComic = serviceComic;
            _servicePublisher = servicePublisher;
            _serviceCategoria = serviceCategory;

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
            // Publisher (si tienes)
            ViewBag.ListPublisher = await _servicePublisher.ListAsync();

            // Categorías (many-to-many)
            var categorias = await _serviceCategoria.ListAsync();

            ViewBag.ListCategorias = new MultiSelectList(
                items: categorias,
                dataValueField: nameof(CategoryDTO.Id),    
                dataTextField: nameof(CategoryDTO.Description), 
                selectedValues: selectedCategoriaIds
            );
        }

        public async Task<IActionResult> Create()
        {
            await LoadCombosAsync();
            return View(new ComicDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComicDTO dto, IFormFile? imageFile, string[] selectedCategorias)
        {
            selectedCategorias ??= Array.Empty<string>();

            // Imagen
            if (imageFile != null && imageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                dto.ImgComic = new List<ImgComicDTO>
                {
                    new ImgComicDTO
                    {
                        Img = ms.ToArray()
                    }
                };
            }

            await _serviceComic.AddAsync(dto, selectedCategorias);

            return RedirectToAction(nameof(Index));
        }


        //Este es el metodo que carga todo los datos necesarios dentro de la vista de edición, como el publisher, las categorias, etc
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _serviceComic.FindByIdAsync(id);
            if (dto == null) return NotFound();

            var selected = dto.Category
                .Select(c => c.Id.ToString()) // 👈 ajusta Id
                .ToList();

            await LoadCombosAsync(selected);

            return View(dto);
        }



        //Esto es el que hace la edición
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, ComicDTO dto, IFormFile? imageFile, string[] selectedCategorias)
        //{
        //    selectedCategorias ??= Array.Empty<string>();

        //    // Imagen nueva
        //    if (imageFile != null && imageFile.Length > 0)
        //    {
        //        using var ms = new MemoryStream();
        //        await imageFile.CopyToAsync(ms);
        //        dto.ImageBytes = ms.ToArray(); // 👈 ajusta
        //    }

        //    await _serviceComic.UpdateAsync(id, dto, selectedCategorias);

        //    return RedirectToAction(nameof(Index));
        //}
    }


}
