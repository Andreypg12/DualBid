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
                .ForMember(dest => dest.Role, orig => orig.MapFrom(o => o.Role))
                .ForMember(dest => dest.Auctions, orig => orig.MapFrom(o => o.Auction))
                .ForMember(dest => dest.Bids, orig => orig.MapFrom(o => o.Bid))
                .ForMember(dest => dest.State, orig => orig.MapFrom(o => o.State))
                .ReverseMap();

            CreateMap<UserDTO, User>()
                // BLOQUEAR la navegación en dirección DTO → Entidad
                .ForMember(d => d.Role, o => o.Ignore())
                .ForMember(d => d.State, o => o.Ignore())
                .ForMember(d => d.Bid, o => o.Ignore())
                .ForMember(d => d.Auction, o => o.Ignore())
                .ForMember(d => d.RegistrationDate, o => o.Ignore())
                .ForMember(d => d.Password, o => o.Ignore());
        }
    }
}
