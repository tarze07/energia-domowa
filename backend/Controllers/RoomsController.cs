using EnergiaDomowa.Api.Data;
using EnergiaDomowa.Api.Dtos;
using EnergiaDomowa.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergiaDomowa.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll(CancellationToken ct)
    {
        var rooms = await db.Rooms
            .OrderBy(r => r.Name)
            .Select(r => new RoomDto(r.Id, r.Name, r.Devices.Count))
            .ToListAsync(ct);
        return Ok(rooms);
    }

    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create(UpsertRoomDto dto, CancellationToken ct)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Nazwa pomieszczenia jest wymagana.");
        }

        var room = new Room { Name = name };
        db.Rooms.Add(room);
        await db.SaveChangesAsync(ct);
        return Created($"/api/rooms/{room.Id}", new RoomDto(room.Id, room.Name, 0));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var room = await db.Rooms.Include(r => r.Devices).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (room is null)
        {
            return NotFound();
        }

        db.Devices.RemoveRange(room.Devices);
        db.Rooms.Remove(room);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
