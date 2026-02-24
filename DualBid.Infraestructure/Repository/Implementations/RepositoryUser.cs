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
    public class ReposiroryUser : IRepositoryUser
    {
        private readonly DualBidContext _context;

        public ReposiroryUser(DualBidContext context)
        {
            _context = context;
        }

        public async Task<User> FindByIdAsync(int id)
        {
            var @object = await _context.Set<User>()
                .Where(l => l.Id == id)
                .Include(x => x.Role)
                .Include(x => x.State)
                .Include(x => x.Auction)
                .FirstOrDefaultAsync();
            return @object!;
        }

        public async Task<ICollection<User>> ListAsync()
        {
            var collection = await _context.Set<User>()
                .Include(x => x.Role)
                .Include(x => x.State)
                .Include(x => x.Auction)
                .OrderBy(x => x.Id)
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }
    }
}
