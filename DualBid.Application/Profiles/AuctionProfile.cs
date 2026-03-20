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
                .ForMember(dest => dest.Id, orig => orig.MapFrom(o => o.Id))
                .ForMember(dest => dest.StartDate, orig => orig.MapFrom(o => o.StartDate))
                .ForMember(dest => dest.ExpectedEndDate, orig => orig.MapFrom(o => o.ExpectedEndDate))
                .ForMember(dest => dest.ActualEndDate, orig => orig.MapFrom(o => o.ActualEndDate))
                .ForMember(dest => dest.BasePrice, orig => orig.MapFrom(o => o.BasePrice))
                .ForMember(dest => dest.MinimunIncrease, orig => orig.MapFrom(o => o.MinimunIncrease))
                .ForMember(dest => dest.Comic, orig => orig.MapFrom(o => o.Comic))
                .ForMember(dest => dest.ComicId, orig => orig.MapFrom(o => o.ComicId))
                .ForMember(dest => dest.CreatorUser, orig => orig.MapFrom(o => o.CreatorUser))
                .ForMember(dest => dest.CreatorUserId, orig => orig.MapFrom(o => o.CreatorUserId))
                .ForMember(dest => dest.State, orig => orig.MapFrom(o => o.State))
                .ForMember(dest => dest.StateId, orig => orig.MapFrom(o => o.StateId))
                .ForMember(dest => dest.Bids, orig => orig.MapFrom(o => o.Bid))
                .ReverseMap();

            CreateMap<AuctionDTO, Auction>()
                // BLOQUEAR la navegación en dirección DTO → Entidad
                .ForMember(d => d.ActualEndDate, o => o.Ignore())
                .ForMember(d => d.Comic, o => o.Ignore())
                .ForMember(d => d.CreatorUser, o => o.Ignore())
                .ForMember(d => d.State, o => o.Ignore())
                .ForMember(d => d.Bid, o => o.Ignore())
                .ForMember(d => d.WinningBid, o => o.Ignore());
        }
    }
}
