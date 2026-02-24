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
                .ForMember(dest => dest.State, orig => orig.MapFrom(o => o.State));
        }
    }
}
