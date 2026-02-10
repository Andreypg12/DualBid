using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;

namespace DualBid.Application.Profiles
{
    public class AuctionProfile : Profile
    {
        public AuctionProfile()
        {
            CreateMap<Auction, AuctionDTO>();

            /* CreateMap<Libro, LibroDTO>(); 
                    CreateMap<Autor, AutorDTO>() 
                        .ForMember(d => d.Libros, opt => opt.MapFrom(s => s.Libro));*/
        }





    }
}
