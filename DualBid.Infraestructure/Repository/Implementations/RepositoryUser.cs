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
                .AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.State)
                .Include(x => x.Auction)
                .Include(x => x.Bid)
                .FirstOrDefaultAsync(u => u.Id == id);
            return @object!;
        }
        public async Task<ICollection<User>> ListAsync()
        {
            var collection = await _context.Set<User>()
                .AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.State)
                .Include(x => x.Auction)
                .Include(x => x.Bid)
                .OrderBy(x => x.Id)
                .ToListAsync();
            return collection;
        }

        public async Task UpdateAsync(User entity)
        {
            // Obtén la entidad existente primero
            var existingEntity = await _context.User
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == entity.Id);

            if (existingEntity != null)
            {
                // Actualiza las propiedades necesarias
                existingEntity.Name = entity.Name;
                existingEntity.LastNames = entity.LastNames;
                existingEntity.Email = entity.Email;
                existingEntity.StateId = entity.StateId; // Asegura que StateId se actualice

                _context.User.Update(existingEntity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User> LoginAsync(string id, string password)
        {
            var @object = await _context.Set<User>()
                                        .Include(b => b.Role)
                                        .Include(b => b.State)
                                        .Where(p => p.Email == id && p.Password == password)
                                        .FirstOrDefaultAsync();
            return @object!;
        }

        public async Task<User> RegisterAsync(User user)
        {
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }
    }
}
