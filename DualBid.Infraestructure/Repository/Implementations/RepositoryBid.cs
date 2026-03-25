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
    public class RepositoryBid : IRepositoryBid
    {
        private readonly DualBidContext _context;

        public RepositoryBid(DualBidContext Context)
        {
            this._context = Context;
        }

        public async Task<ICollection<Bid>> AuctionBiddingHistory(int auctionId)
        {
            var collection = await _context.Set<Bid>()
                .Where(b => b.AuctionId == auctionId)
                .Include(b => b.User)
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }

        public Task<Bid> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Bid>> ListAsync()
        {
            // Select * from Autor
            var collection = await _context.Set<Bid>()
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }

        public async Task<int> AddAsync(Bid entity)
        {
            await _context.Set<Bid>().AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity.AuctionId;
        }
    }
}
