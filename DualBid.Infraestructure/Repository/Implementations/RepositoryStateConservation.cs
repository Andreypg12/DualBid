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
    public class RepositoryStateConservation : IRepositoryStateConservation
    {
        private readonly DualBidContext _context;

        public RepositoryStateConservation(DualBidContext Context)
        {
            this._context = Context;
        }

        public async Task<StateConservation> FindByIdAsync(int id)
        {
            var @object = await _context.Set<StateConservation>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
            return @object!;
        }

        public async Task<ICollection<StateConservation>> ListAsync()
        {
            var collection = await _context.Set<StateConservation>()
                .AsNoTracking()
                .ToListAsync();

            return collection;

        }
    }
}
