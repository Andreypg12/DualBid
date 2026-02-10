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
    public class ServiceBid : IServiceBid
    {
        private readonly IRepositoryBid _repository;
        private readonly IMapper _mapper;

        public ServiceBid(IRepositoryBid repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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
    }
}

