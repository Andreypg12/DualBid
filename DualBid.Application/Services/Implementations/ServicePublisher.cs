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
    public class ServicePublisher : IServicePublisher
    {
        private readonly IRepositoryPublisher _repository;

        private readonly IMapper _mapper;

        public ServicePublisher(IRepositoryPublisher repositoryPublisher, IMapper mapper)
        {
            _repository = repositoryPublisher;
            _mapper = mapper;
        }

        public async Task<PublisherDTO?> FindByIdAsync(int id)
        {
            var @object = await _repository.FindByIdAsync(id);
            var objectMapped = _mapper.Map<PublisherDTO>(@object);
            return objectMapped;
        }

        public Task<ICollection<PublisherDTO>> ListAsync()
        {
            throw new NotImplementedException();
        }
    }
}
