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

            //ReverseMap() crea el mapeo en ambos sentidos.
            CreateMap<ImgComic, ImgComicDTO>().ReverseMap();

            //NO es necesario mapear uno por uno si los nombres son iguales
            //USAR CUANDO LOS NOMBRES ENTRE COMIC Y COMICDTO SON DIFERENTES
            // Mapeo específico para Comic y ComicDTO.

            // BD → Vista
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
                .ForMember(dest => dest.Category, orig => orig.MapFrom(o => o.Category))
                .ForMember(dest => dest.availability, orig => orig.MapFrom(o => o.Availability));

            // Mapeo específico para ComicDTO a Comic, ignorando las propiedades de navegación.
            // Esto es importante para evitar problemas de mapeo cuando se crean o actualizan entidades porque puede romper EF.

            // Vista → BD
            CreateMap<ComicDTO, Comic>()
                .ForMember(dest => dest.Publisher, opt => opt.Ignore())
                .ForMember(dest => dest.StateConservation, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Seller, opt => opt.Ignore());
        }
    }
}
