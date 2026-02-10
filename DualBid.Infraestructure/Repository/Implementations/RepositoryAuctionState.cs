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
    public class RepositoryAuctionState : IRepositoryAuctionState
    {
        private readonly DualBidContext _context;

        public RepositoryAuctionState(DualBidContext context)
        {
            _context = context;
        }

        public Task<AuctionState> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<AuctionState>> ListAsync()
        {
            //Select * from Autor 
            var collection = await _context.Set<AuctionState>()
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }
    }
}
