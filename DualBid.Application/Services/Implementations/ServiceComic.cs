using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ICollection<ComicDTO>> ListAsync()
        {
            var list = await _repositoryComic.ListAsync();
            return _mapper.Map<ICollection<ComicDTO>>(list);
        }

        public async Task<ComicDTO?> FindByIdAsync(int id)
        {
            var comic = await _repositoryComic.FindByIdAsync(id);
            var objetoMapeado = _mapper.Map<ComicDTO>(comic);
            return objetoMapeado;
        }

        public async Task<int> AddAsync(ComicDTO dto, string[] selectedCategorias)
        {
            try
            {
                var entity = _mapper.Map<Comic>(dto);
                return await _repositoryComic.AddAsync(entity, selectedCategorias);
            }
            catch (AutoMapperMappingException ex)
            {
                Console.WriteLine($"Error in service creating a comic : {ex.Message}");
                throw;
            }
        }


        public async Task<bool> UpdateAvailabilityAsync(int id, bool availability)
        {
            try
            {
                return await _repositoryComic.UpdateAvailabilityAsync(id, availability);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in service updating comic availability: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(ComicDTO dto, string[] selectedCategorias, List<ImgComicDTO> newImages, int[] imagesToDelete)
        {
            var entity = _mapper.Map<Comic>(dto);

            var entityImages = newImages?
                .Select(x => new ImgComic
                {
                    Img = x.Img
                })
                .ToList() ?? new List<ImgComic>();

            return await _repositoryComic.UpdateAsync(entity, selectedCategorias, entityImages,imagesToDelete);
        }

        public async Task<ICollection<ComicDTO>> ListComicsForAuctionByUserAsync(int userId)
        {
            var list = await _repositoryComic.ListComicsForAuctionByUserAsync(userId);

            return _mapper.Map<ICollection<ComicDTO>>(list);
        }
    }
}
