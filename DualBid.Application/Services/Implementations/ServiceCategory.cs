using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Implementations
{
    public class ServiceCategory : IServiceCategory
    {

        private readonly IRepositoryCategory _repository;
        private readonly IMapper _mapper;

        public ServiceCategory(IRepositoryCategory repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public Task<CategoryDTO?> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<CategoryDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();

            return _mapper.Map<ICollection<CategoryDTO>>(list);
        }
    }
}
