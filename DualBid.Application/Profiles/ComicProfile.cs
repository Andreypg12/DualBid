using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Profiles
{
    public class ComicProfile : Profile
    {
        public ComicProfile()
        {
            CreateMap<Comic, ComicDTO>()
                .ForMember(dest => dest.Id, orig => orig.MapFrom(o => o.Id))
                .ForMember(dest => dest.Title, orig => orig.MapFrom(o => o.Title))
                .ForMember(dest => dest.Description, orig => orig.MapFrom(o => o.Description))
                .ForMember(dest => dest.EditionNumber, orig => orig.MapFrom(o => o.EditionNumber))
                .ForMember(dest => dest.Isbn, orig => orig.MapFrom(o => o.Isbn))
                .ForMember(dest => dest.CreationDate, orig => orig.MapFrom(o => o.CreationDate))
                .ForMember(dest => dest.YearPublication, orig => orig.MapFrom(o => o.YearPublication))
                .ForMember(dest => dest.Publisher, orig => orig.MapFrom(o => o.Publisher))
                .ForMember(dest => dest.StateConservation, orig => orig.MapFrom(o => o.StateConservation))
                .ForMember(dest => dest.ImgComic, orig => orig.MapFrom(o => o.ImgComic))
                .ForMember(dest => dest.Category, orig => orig.MapFrom(o => o.Category));


            CreateMap<ComicDTO, Comic>()
            // para Create te conviene ignorar navegaciones y asignarlas manual
            .ForMember(dest => dest.Publisher, opt => opt.Ignore())
            .ForMember(dest => dest.StateConservation, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.ImgComic, opt => opt.Ignore())
            .ForMember(dest => dest.Seller, opt => opt.Ignore()); // si existe en la entidad


        }
    }
}
