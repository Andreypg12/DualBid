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
    public class ServiceUserStatus : IServiceUserStatus
    {
        private readonly IRepositoryUserStatus _repository;
        private readonly IMapper _mapper;

        public ServiceUserStatus(IRepositoryUserStatus repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public Task<UserStateDTO?> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<UserStateDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();
            return _mapper.Map<ICollection<UserStateDTO>>(list);
        }
    }
}
