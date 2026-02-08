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

        public Task<User> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<User>> ListAsync()
        {
            //Select * from Autor 
            var collection = await _context.Set<User>()
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }
    }
}
