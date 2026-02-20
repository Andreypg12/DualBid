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
    public class ServiceImgComic : IServiceImgComic
    {
        private readonly IRepositoryImgComic _repository;

        private readonly IMapper _mapper;

        public ServiceImgComic(IRepositoryImgComic repositoryImgComic, IMapper mapper)
        {
            _repository = repositoryImgComic;
            _mapper = mapper;
        }
        public async Task<ICollection<ImgComicDTO>> FindByComicIdAsync(int comicId)
        {
            var list = await _repository.FindByComicIdAsync(comicId);
            return _mapper.Map<ICollection<ImgComicDTO>>(list);
        }
    }
}
