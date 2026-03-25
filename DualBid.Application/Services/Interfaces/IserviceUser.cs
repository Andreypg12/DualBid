using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Interfaces
{
     public interface IserviceUser
    {
        Task<ICollection<UserDTO>> ListAsync();
        Task<UserDTO?> FindByIdAsync(int id);
        Task UpdateAsync(int id, UserDTO dto);
        Task<UserDTO> LoginAsync(string id, string password);
    }
}
