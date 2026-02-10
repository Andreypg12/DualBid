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
    public class ServiceAuctionState : IServiceAuctionState
    {
        private readonly IRepositoryAuctionState _repository;
        private readonly IMapper _mapper;

        public ServiceAuctionState(IRepositoryAuctionState repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public Task<AuctionStateDTO?> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<AuctionStateDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();
            return _mapper.Map<ICollection<AuctionStateDTO>>(list);
        }

    }
}
