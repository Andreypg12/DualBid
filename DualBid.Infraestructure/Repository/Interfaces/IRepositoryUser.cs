using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Infraestructure.Models;

namespace DualBid.Infraestructure.Repository.Interfaces
{
     public interface IRepositoryUser
    {
        Task<ICollection<User>> ListAsync();
        Task<User> FindByIdAsync(int id);
        Task UpdateAsync(User entity);
        Task<User> LoginAsync(string id, string password);
        Task<User> RegisterAsync(User user);
        Task<bool> EmailExistsAsync(string email);
    }
}
