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

            // DTO → ENTIDAD (CREAR / EDITAR)
            CreateMap<BidDTO, Bid>()

                // BLOQUEAR la navegación en dirección DTO → Entidad
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.Date, o => o.Ignore());
        }
    }
}
