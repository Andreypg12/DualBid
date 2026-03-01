using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Implementations;
using DualBid.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Implementations
{
    public class ServiceStateConservation : IServiceStateConservation
    {
        private readonly IRepositoryStateConservation _repository;

        private readonly IMapper _mapper;

        public ServiceStateConservation(IRepositoryStateConservation repositoryPublisher, IMapper mapper)
        {
            _repository = repositoryPublisher;
            _mapper = mapper;
        }

        public async Task<StateConservationDTO?> FindByIdAsync(int id)
        {
            var @object = await _repository.FindByIdAsync(id);
            var objectMapped = _mapper.Map<StateConservationDTO>(@object);
            return objectMapped;
        }

        public async Task<ICollection<StateConservationDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();

            return _mapper.Map<ICollection<StateConservationDTO>>(list);
        }
    }
}
