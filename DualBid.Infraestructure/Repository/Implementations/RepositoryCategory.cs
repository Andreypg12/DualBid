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
    public class RepositoryCategory : IRepositoryCategory
    {
        private readonly DualBidContext _context;

        public RepositoryCategory(DualBidContext context)
        {
            _context = context;
        }

        public Task<Category> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Category>> ListAsync()
        {
            var collection = await _context.Set<Category>()
                .AsNoTracking()
                .ToListAsync();

            return collection;
        }
    }
}
