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

        public async Task<Comic> FindByIdAsync(int id)
        {
            //Incluimos el publisher y el estado de conservación para que no nos de error al mostrar la vista de detalles

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

        public async Task<ICollection<Comic>> ListAsyncCategoria()
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


        public async Task<int> AddAsync(Comic entity, string[] selectedCategorias)
        {
            try
            {
                // Autor: si solo llega IdAutor, no es necesario setear navigation;
                await ApplyCategoriasAsync(entity, selectedCategorias);

                await _context.Set<Comic>().AddAsync(entity);
                entity.SellerId = 13;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var sqlEx = ex.GetBaseException() as Microsoft.Data.SqlClient.SqlException;

                if (sqlEx != null)
                {
                    Console.WriteLine($"SQL Error #{sqlEx.Number}: {sqlEx.Message}");

                    foreach (Microsoft.Data.SqlClient.SqlError err in sqlEx.Errors)
                        Console.WriteLine($"  - #{err.Number}: {err.Message}");
                }
                else
                {
                    Console.WriteLine(ex.GetBaseException().Message);
                }

                throw;
            }

            return entity.Id;
        }


        private async Task ApplyCategoriasAsync(Comic libroToUpdate, string[] selectedCategorias)
        {
            // Si no enviaron categorías, se establece vacío
            if (selectedCategorias == null || selectedCategorias.Length == 0)
            {
                libroToUpdate.Category = new List<Category>();
                return;
            }

            // Parse seguro
            var ids = selectedCategorias
                .Select(x => int.TryParse(x, out var n) ? n : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                libroToUpdate.Category = new List<Category>();
                return;
            }

            // Trae SOLO las categorías requeridas
            var categorias = await _context.Category
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            libroToUpdate.Category = categorias;
        }

        public async Task<bool> UpdateAvailabilityAsync(int id, bool availability)
        {
            try
            {
                var comic = await _context.Set<Comic>().FindAsync(id);
                if (comic == null)
                    return false;

                comic.Availability = availability;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Loggear el error si tienes un sistema de logging
                Console.WriteLine($"Error updating comic availability: {ex.Message}");
                throw;
            }
        }
    }
}
