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
    public class RepositoryRole : IRepositoryRole
    {
        private readonly DualBidContext _context;

        public RepositoryRole(DualBidContext context)
        {
            _context = context;
        }

        public async Task<Role?> FindByIdAsync(int id)
        {
            return await _context.Set<Role>()
               .AsNoTracking()
               .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<ICollection<Role>> ListAsync()
        {
            //Select * from Role
            var collection = await _context.Set<Role>()
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }
    }
}
