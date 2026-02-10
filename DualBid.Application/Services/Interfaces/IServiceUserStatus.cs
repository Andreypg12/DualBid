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
        //Icollection es una interfaz que representa una colección de objetos
        //significa: Este método devolverá muchos AuctionState, no uno solo
        Task<ICollection<UserStateDTO>> ListAsync();
        Task<UserStateDTO> FindByIdAsync(int id);
    }
}
