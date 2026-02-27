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
                        .Include(x => x.Auction)
                        .Include(x => x.Seller)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            return @object!;
        }

        //public async Task<ICollection<Comic>> ListAsync()
        //{
        //    var collection = await _context.Set<Comic>()
        //        .Include(x => x.Publisher)
        //        .Include(x => x.StateConservation)
        //        .Include(x => x.ImgComic)
        //        .AsNoTracking()
        //        .ToListAsync();

        //    return collection;
        //}

        public async Task<ICollection<Comic>> ListAsync()
        {
            var collection = await _context.Set<Comic>()
                .Include(x => x.Publisher)
                .Include(x => x.StateConservation)
                .Include(x => x.ImgComic)
                .Include(x => x.Category)
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
                .AsNoTracking()
                .ToListAsync();

            return collection;
        }
    }
}
