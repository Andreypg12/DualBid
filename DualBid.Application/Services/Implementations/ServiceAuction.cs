using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Implementations;
using DualBid.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Implementations
{
    public class ServiceAuction : IServiceAuction
    {
        private readonly IRepositoryAuction _repository;
        private readonly IMapper _mapper;

        private const int StateActive = 2;
        private const int StateFinished = 3;
        private const int StateCancelled = 4;

        public ServiceAuction(IRepositoryAuction repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<AuctionDTO?> FindByIdAsync(int id)
        {
            var @object = await _repository.FindByIdAsync(id);
            var objectMapped = _mapper.Map<AuctionDTO>(@object);
            return objectMapped;
        }

        public async Task<ICollection<AuctionDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();
            return _mapper.Map<ICollection<AuctionDTO>>(list);
        }

        public async Task<ICollection<AuctionDTO>> ListActiveAsync()
        {
            var list = await _repository.ListActiveAsync();
            return _mapper.Map<ICollection<AuctionDTO>>(list);
        }

        public async Task<int> AddAsync(AuctionDTO dto)
        {
            try
            {
                var entity = _mapper.Map<Auction>(dto);

                return await _repository.AddAsync(entity);
            }
            catch (AutoMapperMappingException ex)
            {
                var msg = ex.ToString(); // incluye tipos origen/destino y qué miembro falló
                throw;
            }
        }

        public async Task UpdateAsync(int id, AuctionDTO dto)
        {
            // Traer entity (idealmente trackeado) antes de mapear encima
            var entity = await _repository.FindByIdAsync(id);


            _mapper.Map(dto, entity);


            await _repository.UpdateAsync(entity);
        }

        public async Task<bool> UpdateStateAsync(int auctionId, int newStateId)
        {
            return await _repository.UpdateStateAsync(auctionId, newStateId);
        }


        public async Task<bool> EncontrarGanadorAsync(int auctionId)
        {
            return await _repository.EncontrarGanadorAsync(auctionId);
        }

        public async Task<IEnumerable<ActiveAuctionDTO>> GetActiveAuctionsAsync()
        {
            var auctions = await _repository.ListActiveAsync();

            return auctions
                .Where(a => a.StateId == StateActive)
                .Select(a => new ActiveAuctionDTO
                {
                    Id = a.Id,
                    EndDate = a.ExpectedEndDate.ToUniversalTime(),
                    OwnerUserId = a.CreatorUserId
                });
        }

        public async Task<AuctionCloseResultDTO?> CloseAuctionAsync(int auctionId)
        {
            var entity = await _repository.FindByIdAsync(auctionId);

            if (entity == null)
                return null;

            // Mapear para verificar estado actual
            var auction = _mapper.Map<AuctionDTO>(entity);

            if (auction == null || auction.StateId != StateActive)
                return null;

            // EncontrarGanadorAsync maneja:
            //   con pujas   StateId=3, WinningBidId=X
            //   sin pujas   StateId=4, comic.availability=true
            await _repository.EncontrarGanadorAsync(auctionId);

            // Recargar para obtener el WinningBid con User incluido
            var updated = await _repository.FindByIdAsync(auctionId);

            var hasBids = updated?.WinningBidId != null;
            var winnerUserId = updated?.WinningBid?.UserId;
            var winnerName = updated?.WinningBid?.User?.Name;
            var finalAmount = updated?.WinningBid?.AmountOffered ?? 0m;
            var finalStateId = hasBids ? StateFinished : StateCancelled;

            return new AuctionCloseResultDTO
            {
                AuctionId = auctionId,
                ComicTitle = updated?.Comic?.Title,
                WinnerUserId = winnerUserId,
                WinnerName = winnerName,
                FinalAmount = finalAmount,
                OwnerUserId = updated?.CreatorUserId,
                FinalStateId = finalStateId
            };
        }

    }
}
