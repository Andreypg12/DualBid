using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Infraestructure.Data;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DualBid.Infraestructure.Repository.Implementations
{
    public class RepositoryAuction : IRepositoryAuction
    {
        private readonly DualBidContext _context;

        public RepositoryAuction(DualBidContext Context) 
        {
            this._context = Context;
        }

        public async Task<Auction> FindByIdAsync(int id)
        {
            var @object = await _context.Set<Auction>()
                .AsNoTracking()
                .AsSplitQuery()
                .Where(a => a.Id == id)
                .Include(a => a.State)
                .Include(a => a.CreatorUser)
                .Include(a => a.Comic)
                    .ThenInclude(c => c.ImgComic)
                .Include(a => a.Comic)
                    .ThenInclude(c => c.Category)
                .Include(a => a.Comic)
                    .ThenInclude(c => c.StateConservation)
                .Include(a => a.Bid)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync();

            return @object!;
        }

        public async Task<ICollection<Auction>> ListAsync()
        {
            //Select * from Autor 
            var collection = await _context.Set<Auction>()
                .Include(a => a.State)
                .Include(a => a.Comic)
                    .ThenInclude(c => c.ImgComic)
                .Include(a => a.Bid)
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }

        public async Task<int> AddAsync(Auction entity)
        {
            await _context.Set<Auction>().AddAsync(entity);

            await _context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task UpdateAsync(Auction entity)
        {
            // entity DEBE venir trackeado
            // Igual se reestablece
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Attach(entity);
            }

            // Si el mapping ya actualizó propiedades escalares, esto garantiza update
            _context.Entry(entity).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }
    }
}
