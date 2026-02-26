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
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Id, orig => orig.MapFrom(o => o.Id))
                .ForMember(dest => dest.Name, orig => orig.MapFrom(o => o.Name))
                .ForMember(dest => dest.LastNames, orig => orig.MapFrom(o => o.LastNames))
                .ForMember(dest => dest.Email, orig => orig.MapFrom(o => o.Email))
                .ForMember(dest => dest.Password, orig => orig.MapFrom(o => o.Password))
                .ForMember(dest => dest.RegistrationDate, orig => orig.MapFrom(o => o.RegistrationDate))
                .ForMember(dest => dest.Role, orig => orig.MapFrom(o => o.Role))
                .ForMember(dest => dest.Auctions, orig => orig.MapFrom(o => o.Auction))
                .ForMember(dest => dest.Bids, orig => orig.MapFrom(o => o.Bid))
                .ForMember(dest => dest.State, orig => orig.MapFrom(o => o.State));
        }
    }
}
