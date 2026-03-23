using DualBid.Infraestructure.Data;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Infraestructure.Repository.Implementations
{
    public class RepositoryComic : IRepositoryComic
    {

        private readonly DualBidContext _context;

        public RepositoryComic(DualBidContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Comic>> ListAsync()
        {
            var collection = await _context.Set<Comic>()
                .Include(x => x.Publisher)
                .Include(x => x.StateConservation)
                .Include(x => x.ImgComic)
                .Include(x => x.Category)
                .Include(x => x.Auction)
                .Include(x => x.Seller)
                .AsNoTracking()
                .ToListAsync();

            return collection;
        }

        public async Task<Comic> FindByIdAsync(int id)
        {
            var @object = await _context.Set<Comic>().
                        Where(comic => comic.Id == id)
                        .Include(x => x.Publisher)
                        .Include(x => x.StateConservation)
                        .Include(x => x.ImgComic)
                        .Include(x => x.Category)
                        .Include(x => x.Seller)
                        .Include(x => x.Auction)
                            .ThenInclude(a => a.State)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            return @object!;
        }

        public async Task<int> AddAsync(Comic entity, string[] selectedCategorias)
        {

            //Trae todas las categorías seleccionadas y las asigna al cómic
            await ApplyCategoriasAsync(entity, selectedCategorias);

                entity.Availability = true; // Por defecto, el cómic está disponible
                await _context.Set<Comic>().AddAsync(entity);
                await _context.SaveChangesAsync();
            

            return entity.Id;
        }


        private async Task ApplyCategoriasAsync(Comic comicToUpdate, string[] selectedCategorias)
        {

            // Esto contierte los seleccionados a enteros, elimina duplicados y convierte a lista
            var ids = selectedCategorias
                .Select(int.Parse)
                .Distinct()
                .ToList();

            // Trae las categorías requeridas
            var categorias = await _context.Category
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            comicToUpdate.Category = categorias;
        }




        public async Task<bool> UpdateAvailabilityAsync(int id, bool availability)
        {
            try
            {
                var comic = await _context.Comic.FindAsync(id);
                if (comic == null)
                    return false;

                comic.Availability = availability;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating comic availability: {ex.Message}");
                throw;
            }
        }



        public async Task<bool> UpdateAsync(Comic entity,string[] selectedCategorias,List<ImgComic> newImages,int[] imagesToDelete)
        {
            var comic = await _context.Comic
                .Include(x => x.Category)
                .Include(x => x.ImgComic)
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (comic == null)
                return false;

            comic.Title = entity.Title;
            comic.Description = entity.Description;
            comic.Isbn = entity.Isbn;
            comic.EditionNumber = entity.EditionNumber;
            comic.YearPublication = entity.YearPublication;
            comic.CreationDate = entity.CreationDate;
            comic.PublisherId = entity.PublisherId;
            comic.StateConservationId = entity.StateConservationId;

            await ApplyCategoriasAsync(comic, selectedCategorias);


            // eliminar imágenes si es necesario
            if (imagesToDelete != null && imagesToDelete.Length > 0)
            {
                var imgs = comic.ImgComic
                    .Where(i => imagesToDelete.Contains(i.Id))
                    .ToList();

                _context.ImgComic.RemoveRange(imgs);
            }

            // nuevas imágenes
            if (newImages != null && newImages.Count > 0)
            {
                foreach (var img in newImages)
                {
                    comic.ImgComic.Add(new ImgComic
                    {
                        Img = img.Img
                    });
                }
            }

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
