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
            CreateMap<ComicDTO, Comic>().ReverseMap();

            CreateMap<ComicDTO, Comic>()
                .ForMember(dest => dest.Id, orig => orig.MapFrom(o => o.Id))
                .ForMember(dest => dest.Title, orig => orig.MapFrom(o => o.Title))
                .ForMember(dest => dest.Description, orig => orig.MapFrom(o => o.Description))
                .ForMember(dest => dest.EditionNumber, orig => orig.MapFrom(o => o.EditionNumber))
                .ForMember(dest => dest.Isbn, orig => orig.MapFrom(o => o.Isbn))
                .ForMember(dest => dest.CreationDate, orig => orig.MapFrom(o => o.CreationDate))
                .ForMember(dest => dest.YearPublication, orig => orig.MapFrom(o => o.YearPublication))
                .ForMember(dest => dest.Publisher, orig => orig.MapFrom(o => o.Publisher))
                .ForMember(dest => dest.StateConservation, orig => orig.MapFrom(o => o.StateConservation))
                .ForMember(dest => dest.ImgComic, orig => orig.MapFrom(o => o.ImgComic));
        }
    }
}
