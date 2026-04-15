using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Application.DTOs;

namespace DualBid.Application.Services.Interfaces
{
    public interface IServiceAuction
    {
        Task<ICollection<AuctionDTO>> ListAsync();
        Task<ICollection<AuctionDTO>> ListActiveAsync();
        Task<AuctionDTO?> FindByIdAsync(int id);
        Task<int> AddAsync(AuctionDTO dto);
        Task UpdateAsync(int id, AuctionDTO dto);
        Task<bool> UpdateStateAsync(int auctionId, int newStateId);


        //@* Editado por ALE *@
        //Determinar el ganador de la subasta
        Task<bool> EncontrarGanadorAsync(int auctionId);

        Task<IEnumerable<ActiveAuctionDTO>> GetActiveAuctionsAsync();

        Task<AuctionCloseResultDTO?> CloseAuctionAsync(int auctionId);
    }
}
