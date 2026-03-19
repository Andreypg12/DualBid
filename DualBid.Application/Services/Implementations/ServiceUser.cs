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
    public class ServiceUser : IserviceUser
    {
        private readonly IRepositoryUser _repository;
        private readonly IMapper _mapper;

        public ServiceUser(IRepositoryUser repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UserDTO?> FindByIdAsync(int id)
        {
            var @object = await _repository.FindByIdAsync(id);
            var objectMapped = _mapper.Map<UserDTO>(@object);
            return objectMapped;
        }

        public async Task<ICollection<UserDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();
            return _mapper.Map<ICollection<UserDTO>>(list);
        }

        public async Task UpdateAsync(int id, UserDTO dto)
        {

            var entity = await _repository.FindByIdAsync(id);

            _mapper.Map(dto, entity);

            await _repository.UpdateAsync(entity);
        }
    }
}
