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

        public Task<Comic> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Comic>> ListAsync()
        {
            var collection = await _context.Set<Comic>()
                .Include(x => x.Publisher)
                .Include(x => x.StateConservation)
                .AsNoTracking()
                .ToListAsync();

            return collection;
        }
    }
}
