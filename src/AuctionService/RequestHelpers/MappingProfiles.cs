using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.ReqestHelpers;

public class MappingProfiles : Profile
{
  public MappingProfiles()
  {
     CreateMap<Auction, AuctionDTO>().IncludeMembers(x=>x.Item);
     CreateMap<Item, AuctionDTO>();
     CreateMap<CreateActionDTO, Auction>()
             .ForMember(d => d.Item, o =>o.MapFrom(s=>s));
     CreateMap<CreateActionDTO, Item>(); 
  }
}