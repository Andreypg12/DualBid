using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Repository.Interfaces;

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
    }
}
