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
    public class AuctionStateProfile : Profile
    {
        public AuctionStateProfile() 
        {
            CreateMap<AuctionState, AuctionStateDTO>();
            /*.ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.Description));*/
        }
    }
}
