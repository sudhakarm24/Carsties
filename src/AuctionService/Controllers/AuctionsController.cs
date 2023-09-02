using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controller;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDBContext _context;
    private readonly IMapper _mapper; 
    private readonly IPublishEndpoint _publishEndpoint;
  public AuctionsController(AuctionDBContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
  {
    this._context = context;
    this._mapper = mapper;
    this._publishEndpoint = publishEndpoint;
  }
  [HttpGet]
  public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string Date)
  {
      var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

      if(!string.IsNullOrEmpty(Date)){
        query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(Date).ToUniversalTime()) > 0);
      }

      return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync(); 
  }
  [HttpGet("{id}")]
  public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id){
      var auction = await _context.Auctions
                .Include(x=>x.Item)
                .FirstOrDefaultAsync(x=>x.Id == id);
      
      if(auction == null){
        return NotFound();
      }
      return _mapper.Map<AuctionDto>(auction);    
  }
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateActionDTO auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = "test";
    
       _context.Auctions.Add(auction);

        var result = await _context.SaveChangesAsync() > 0;

        var newAuction = _mapper.Map<AuctionDto>(auction);

        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        if (!result) return BadRequest("Could not save changes to the DB");

        return CreatedAtAction(nameof(GetAuctionById),
            new { auction.Id }, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO updateAuctionDto)
    {
        var auction =await _context.Auctions.Include(x=>x.Item)
        .FirstOrDefaultAsync(x=>x.Id == id);

        if(auction == null){
            return NotFound();
        }
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;  
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;  
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;  
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;  
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;  

         await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _context.SaveChangesAsync() > 0;
        if(result)
        { 
            return Ok();
        } 
        return BadRequest("Problem saving changes");

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction =await _context.Auctions.FindAsync(id);

        if (auction == null){
            return NotFound();
        }

        _context.Auctions.Remove(auction);
        
        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not update DB");

        return Ok();
    }

}