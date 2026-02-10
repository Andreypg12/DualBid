using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;
using AutoMapper;

namespace DualBid.Application.Profiles
{
    public class BidProfile : Profile
    {
        public BidProfile()
        {
            CreateMap<Bid, BidDTO>();
            /* CreateMap<Libro, LibroDTO>(); 
            CreateMap<Autor, AutorDTO>() 
                .ForMember(d => d.Libros, opt => opt.MapFrom(s => s.Libro));*/
        }
    }
}
