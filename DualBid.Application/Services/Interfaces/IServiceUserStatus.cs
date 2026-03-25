using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;

namespace DualBid.Application.Services.Interfaces
{
    public interface IServiceUserStatus
    {
        Task<ICollection<UserStateDTO>> ListAsync();
        Task<UserStateDTO> FindByIdAsync(int id);
    }
}
