using AuctionService.DTO;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.ReqestHelpers;

public class MappingProfiles : Profile
{
  public MappingProfiles()
  {
     CreateMap<Auction, AuctionDto>().IncludeMembers(x=>x.Item);
     CreateMap<Item, AuctionDto>();
     CreateMap<CreateActionDTO, Auction>()
             .ForMember(d => d.Item, o =>o.MapFrom(s=>s));
     CreateMap<CreateActionDTO, Item>(); 
     CreateMap<AuctionDto, AuctionCreated>();
     CreateMap<Auction, AuctionUpdated>().IncludeMembers(a => a.Item);
     CreateMap<Item, AuctionUpdated>();
  }
}