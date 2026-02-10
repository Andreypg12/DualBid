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
    }
}
