using EnergiaDomowa.Api.Data;
using EnergiaDomowa.Api.Dtos;
using EnergiaDomowa.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergiaDomowa.Api.Controllers;

[ApiController]
[Route("api/tariff")]
public class TariffController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TariffDto>> Get(CancellationToken ct)
    {
        var tariff = await db.Tariffs.FirstOrDefaultAsync(ct);
        if (tariff is null)
        {
            return NotFound();
        }

        return Ok(ToDto(tariff));
    }

    [HttpPut]
    public async Task<ActionResult<TariffDto>> Update(UpsertTariffDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.PricePerKwh < 0 || dto.DailyLimitKwh < 0 || dto.MonthlyBudgetPln < 0)
        {
            return BadRequest("Niepoprawne dane taryfy.");
        }

        var tariff = await db.Tariffs.FirstOrDefaultAsync(ct);
        if (tariff is null)
        {
            tariff = new Tariff();
            db.Tariffs.Add(tariff);
        }

        tariff.Name = dto.Name.Trim();
        tariff.PricePerKwh = dto.PricePerKwh;
        tariff.DailyLimitKwh = dto.DailyLimitKwh;
        tariff.MonthlyBudgetPln = dto.MonthlyBudgetPln;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(tariff));
    }

    private static TariffDto ToDto(Tariff t) =>
        new(t.Id, t.Name, t.PricePerKwh, t.DailyLimitKwh, t.MonthlyBudgetPln);
}
