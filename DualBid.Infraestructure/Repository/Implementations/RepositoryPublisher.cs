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
    public class RepositoryPublisher : IRepositoryPublisher
    {
        private readonly DualBidContext _context;

        public RepositoryPublisher(DualBidContext Context)
        {
            this._context = Context;
        }

        public async Task<Publisher> FindByIdAsync(int id)
        {
            var @object = await _context.Set<Publisher>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
            return @object!;
        }

        public Task<ICollection<Publisher>> ListAsync()
        {
            throw new NotImplementedException();

        }
    }
}
