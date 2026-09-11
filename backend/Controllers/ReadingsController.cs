using EnergiaDomowa.Api.Data;
using EnergiaDomowa.Api.Dtos;
using EnergiaDomowa.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergiaDomowa.Api.Controllers;

[ApiController]
[Route("api/readings")]
public class ReadingsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MeterReadingDto>>> GetAll(CancellationToken ct)
    {
        var readings = await db.MeterReadings.OrderBy(r => r.Date).ToListAsync(ct);
        decimal? previous = null;
        var result = new List<MeterReadingDto>(readings.Count);
        foreach (var reading in readings)
        {
            var delta = previous is null ? (decimal?)null : Math.Round(reading.TotalKwh - previous.Value, 3);
            result.Add(new MeterReadingDto(reading.Id, reading.Date, reading.TotalKwh, reading.Note, delta));
            previous = reading.TotalKwh;
        }

        result.Reverse();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MeterReadingDto>> Create(UpsertMeterReadingDto dto, CancellationToken ct)
    {
        if (dto.TotalKwh < 0)
        {
            return BadRequest("Stan licznika nie może być ujemny.");
        }

        if (await db.MeterReadings.AnyAsync(r => r.Date == dto.Date, ct))
        {
            return Conflict("Odczyt dla tej daty już istnieje.");
        }

        var last = await db.MeterReadings
            .Where(r => r.Date < dto.Date)
            .OrderByDescending(r => r.Date)
            .FirstOrDefaultAsync(ct);

        if (last is not null && dto.TotalKwh < last.TotalKwh)
        {
            return BadRequest("Stan licznika nie może spaść względem poprzedniego odczytu.");
        }

        var reading = new MeterReading { Date = dto.Date, TotalKwh = dto.TotalKwh, Note = dto.Note };
        db.MeterReadings.Add(reading);
        await db.SaveChangesAsync(ct);
        var delta = last is null ? (decimal?)null : Math.Round(reading.TotalKwh - last.TotalKwh, 3);
        return Created($"/api/readings/{reading.Id}", new MeterReadingDto(reading.Id, reading.Date, reading.TotalKwh, reading.Note, delta));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var reading = await db.MeterReadings.FindAsync([id], ct);
        if (reading is null)
        {
            return NotFound();
        }

        db.MeterReadings.Remove(reading);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
