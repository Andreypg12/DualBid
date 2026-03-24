using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
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
    }
}
