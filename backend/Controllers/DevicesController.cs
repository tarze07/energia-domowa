using EnergiaDomowa.Api.Data;
using EnergiaDomowa.Api.Dtos;
using EnergiaDomowa.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergiaDomowa.Api.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAll(CancellationToken ct)
    {
        var price = await GetPrice(ct);
        var devices = await db.Devices.Include(d => d.Room).OrderBy(d => d.Name).ToListAsync(ct);
        return Ok(devices.Select(d => ToDto(d, price)));
    }

    [HttpPost]
    public async Task<ActionResult<DeviceDto>> Create(UpsertDeviceDto dto, CancellationToken ct)
    {
        var error = Validate(dto);
        if (error is not null)
        {
            return BadRequest(error);
        }

        var room = await db.Rooms.FindAsync([dto.RoomId], ct);
        if (room is null)
        {
            return BadRequest("Nie znaleziono pomieszczenia.");
        }

        var device = new Device
        {
            Name = dto.Name.Trim(),
            RoomId = dto.RoomId,
            PowerWatts = dto.PowerWatts,
            HoursPerDay = dto.HoursPerDay,
            IsActive = dto.IsActive
        };
        db.Devices.Add(device);
        await db.SaveChangesAsync(ct);
        device.Room = room;
        return Created($"/api/devices/{device.Id}", ToDto(device, await GetPrice(ct)));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DeviceDto>> Update(int id, UpsertDeviceDto dto, CancellationToken ct)
    {
        var error = Validate(dto);
        if (error is not null)
        {
            return BadRequest(error);
        }

        var device = await db.Devices.Include(d => d.Room).FirstOrDefaultAsync(d => d.Id == id, ct);
        if (device is null)
        {
            return NotFound();
        }

        var room = await db.Rooms.FindAsync([dto.RoomId], ct);
        if (room is null)
        {
            return BadRequest("Nie znaleziono pomieszczenia.");
        }

        device.Name = dto.Name.Trim();
        device.RoomId = dto.RoomId;
        device.Room = room;
        device.PowerWatts = dto.PowerWatts;
        device.HoursPerDay = dto.HoursPerDay;
        device.IsActive = dto.IsActive;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(device, await GetPrice(ct)));
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<ActionResult<DeviceDto>> Toggle(int id, CancellationToken ct)
    {
        var device = await db.Devices.Include(d => d.Room).FirstOrDefaultAsync(d => d.Id == id, ct);
        if (device is null)
        {
            return NotFound();
        }

        device.IsActive = !device.IsActive;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(device, await GetPrice(ct)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var device = await db.Devices.FindAsync([id], ct);
        if (device is null)
        {
            return NotFound();
        }

        db.Devices.Remove(device);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static string? Validate(UpsertDeviceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Nazwa urządzenia jest wymagana.";
        }

        if (dto.PowerWatts is < 1 or > 50000)
        {
            return "Moc musi być w zakresie 1–50000 W.";
        }

        if (dto.HoursPerDay is < 0 or > 24)
        {
            return "Czas pracy musi być w zakresie 0–24 h.";
        }

        return null;
    }

    private async Task<decimal> GetPrice(CancellationToken ct) =>
        await db.Tariffs.Select(t => t.PricePerKwh).FirstOrDefaultAsync(ct);

    private static DeviceDto ToDto(Device d, decimal price)
    {
        var daily = d.IsActive ? EnergyMath.DailyKwh(d.PowerWatts, d.HoursPerDay) : 0;
        var monthly = Math.Round(daily * 30, 2);
        return new DeviceDto(
            d.Id,
            d.Name,
            d.RoomId,
            d.Room.Name,
            d.PowerWatts,
            d.HoursPerDay,
            d.IsActive,
            daily,
            monthly,
            EnergyMath.Cost(monthly, price));
    }
}
