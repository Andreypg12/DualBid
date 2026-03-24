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
            CreateMap<Auction, AuctionDTO>()
                .ForMember(dest => dest.Comic, orig => orig.MapFrom(o => o.Comic))
                .ForMember(dest => dest.CreatorUser, orig => orig.MapFrom(o => o.CreatorUser))
                .ForMember(dest => dest.State, orig => orig.MapFrom(o => o.State))
                .ForMember(dest => dest.Bids, orig => orig.MapFrom(o => o.Bid))
                .ForMember(dest => dest.StateId, orig => orig.MapFrom(o => o.StateId))
                .ReverseMap()

                // BLOQUEAR la navegación en dirección DTO → Entidad
                .ForMember(d => d.Comic, o => o.Ignore())
                .ForMember(d => d.CreatorUser, o => o.Ignore())
                .ForMember(d => d.State, o => o.Ignore())
                .ForMember(d => d.Bid, o => o.Ignore())
                .ForMember(d => d.WinningBid, o => o.Ignore())
                .ForMember(d => d.ActualEndDate, o => o.Ignore());
        }
    }
}
