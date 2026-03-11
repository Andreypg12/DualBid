using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Interfaces;

namespace DualBid.Application.Services.Implementations
{
    public class ServiceBid : IServiceBid
    {
        private readonly IRepositoryBid _repository;
        private readonly IMapper _mapper;

        public ServiceBid(IRepositoryBid repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ICollection<BidDTO>> AuctionBiddingHistory(int auctionId)
        {
            var list = await _repository.AuctionBiddingHistory(auctionId);
            return _mapper.Map<ICollection<BidDTO>>(list);
        }

        public Task<BidDTO?> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<BidDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();
            return _mapper.Map<ICollection<BidDTO>>(list);
        }

        public async Task<int> AddAsync(BidDTO dto)
        {
            try
            {
                var entity = _mapper.Map<Bid>(dto);
                return await _repository.AddAsync(entity);
            }
            catch (AutoMapperMappingException ex)
            {
                var msg = ex.ToString(); // incluye tipos origen/destino y qué miembro falló
                throw;
            }

        }
    }
}

