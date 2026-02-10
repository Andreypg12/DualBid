using System;
using System.Collections;
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
    public class RepositoryUserStatus : IRepositoryUserStatus
    {
        private readonly DualBidContext _context;

        public RepositoryUserStatus(DualBidContext context)
        {
            _context = context;
        }

        Task<UserStatus> IRepositoryUserStatus.FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<ICollection<UserStatus>> ListAsync()
        {
            //Select * from Autor 
            var collection = await _context.Set<UserStatus>()
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }
    }
}
