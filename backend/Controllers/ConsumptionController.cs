using EnergiaDomowa.Api.Data;
using EnergiaDomowa.Api.Dtos;
using EnergiaDomowa.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergiaDomowa.Api.Controllers;

[ApiController]
[Route("api/consumption")]
public class ConsumptionController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DailyConsumptionDto>>> GetAll(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var price = await db.Tariffs.Select(t => t.PricePerKwh).FirstOrDefaultAsync(ct);
        var query = db.DailyConsumptions.AsQueryable();
        if (from is not null)
        {
            query = query.Where(x => x.Date >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.Date <= to);
        }

        var items = await query.OrderByDescending(x => x.Date).ToListAsync(ct);
        return Ok(items.Select(x => new DailyConsumptionDto(x.Id, x.Date, x.Kwh, EnergyMath.Cost(x.Kwh, price))));
    }

    [HttpPost]
    public async Task<ActionResult<DailyConsumptionDto>> Upsert(UpsertConsumptionDto dto, CancellationToken ct)
    {
        if (dto.Kwh < 0)
        {
            return BadRequest("Zużycie nie może być ujemne.");
        }

        var existing = await db.DailyConsumptions.FirstOrDefaultAsync(x => x.Date == dto.Date, ct);
        if (existing is null)
        {
            existing = new DailyConsumption { Date = dto.Date, Kwh = dto.Kwh };
            db.DailyConsumptions.Add(existing);
        }
        else
        {
            existing.Kwh = dto.Kwh;
        }

        await db.SaveChangesAsync(ct);
        var price = await db.Tariffs.Select(t => t.PricePerKwh).FirstOrDefaultAsync(ct);
        return Ok(new DailyConsumptionDto(existing.Id, existing.Date, existing.Kwh, EnergyMath.Cost(existing.Kwh, price)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await db.DailyConsumptions.FindAsync([id], ct);
        if (item is null)
        {
            return NotFound();
        }

        db.DailyConsumptions.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
