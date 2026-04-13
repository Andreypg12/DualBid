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


        // @* Editado por ALE *@
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
                .Include(a => a.WinningBid)
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

        public async Task<ICollection<Auction>> ListActiveAsync()
        {
            var collection = await _context.Set<Auction>()
                .Include(a => a.State)
                .Include(a => a.Comic)
                    .ThenInclude(c => c.ImgComic)
                .Include(a => a.CreatorUser)
                .Include(a => a.Bid)
                .Where(a=> a.StateId == 1 || a.StateId == 2) // Solo subastas activas
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

            _context.Entry(entity).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStateAsync(int auctionId, int newStateId)
        {
            try
            {
                var query = _context.Auction
                    .Where(a => a.Id == auctionId);

                int rowsAffected;

                if (newStateId == 4)
                {
                    rowsAffected = await query.ExecuteUpdateAsync(setters => setters
                        .SetProperty(a => a.StateId, newStateId)
                        .SetProperty(a => a.ActualEndDate, DateTime.Now)
                    );
                }
                else
                {
                    rowsAffected = await query.ExecuteUpdateAsync(setters => setters
                        .SetProperty(a => a.StateId, newStateId)
                    );
                }

                Console.WriteLine($"Cambiando estado de subasta {auctionId} a {newStateId}. Filas afectadas: {rowsAffected}");

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cambiando estado: {ex.Message}");
                return false;
            }
        }


        // @* Editado por ALE *@
        //Determinar Ganador de la subasta
        // Reemplaza SOLO el método EncontrarGanadorAsync en tu RepositoryAuction.cs
        // El resto de la clase no cambia.

        public async Task<bool> EncontrarGanadorAsync(int auctionId)
        {
            var auction = await _context.Auction
                .Include(a => a.Bid)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction == null)
                return false;

            auction.ActualEndDate = DateTime.Now;

            if (auction.Bid == null || !auction.Bid.Any())
            {
                // ✅ Sin pujas: estado 4 (Cancelado) y cómic disponible nuevamente
                auction.StateId = 4;
                auction.WinningBidId = null;

                // Restaurar disponibilidad del cómic para que pueda volver a subastarse
                var comic = await _context.Comic.FindAsync(auction.ComicId);
                if (comic != null)
                    comic.Availability = true;

                await _context.SaveChangesAsync();
                return true;
            }

            // ✅ Con pujas: estado 3 (Finalizado), determinar ganador
            auction.StateId = 3;

            var winningBid = auction.Bid
                .OrderByDescending(b => b.AmountOffered)
                .ThenBy(b => b.Date)
                .First();

            auction.WinningBidId = winningBid.Id;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
