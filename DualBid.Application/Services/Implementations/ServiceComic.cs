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
    public class ServiceComic : IServiceComic
    {

        private readonly IRepositoryComic _repositoryComic;
        private readonly IMapper _mapper;

        public ServiceComic(IRepositoryComic repositoryComic, IMapper mapper)
        {
            _repositoryComic = repositoryComic;
            _mapper = mapper;
        }

        public Task<ComicDTO?> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<ComicDTO>> ListAsync()
        {
            var list = await _repositoryComic.ListAsync();

            return _mapper.Map<ICollection<ComicDTO>>(list);
        }
    }
}
