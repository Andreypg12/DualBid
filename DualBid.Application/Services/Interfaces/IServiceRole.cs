using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;

namespace DualBid.Application.Services.Interfaces
{
    public interface IServiceRole
    {
        Task<ICollection<RoleDTO>> ListAsync();
        Task<RoleDTO> FindByIdAsync(int id);
    }
}
