using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Repository.Implementations;
using DualBid.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<UserStateDTO?> FindByIdAsync(int id)
        {
            var @object = await _repository.FindByIdAsync(id);
            var objectMapped = _mapper.Map<UserStateDTO>(@object);
            return objectMapped;
        }

        public async Task<ICollection<UserStateDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();
            return _mapper.Map<ICollection<UserStateDTO>>(list);
        }
    }
}
