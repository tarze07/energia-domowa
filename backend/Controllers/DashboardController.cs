using EnergiaDomowa.Api.Data;
using EnergiaDomowa.Api.Dtos;
using EnergiaDomowa.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergiaDomowa.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var prevMonthEnd = monthStart.AddDays(-1);
        var prevMonthStart = new DateOnly(prevMonthEnd.Year, prevMonthEnd.Month, 1);
        var from = today.AddDays(-29);

        var tariff = await db.Tariffs.FirstAsync(ct);
        var price = tariff.PricePerKwh;
        var days = await LoadDays(from, today, ct);
        var monthRows = await LoadDays(monthStart, today, ct);
        var prevRows = await LoadDays(prevMonthStart, prevMonthEnd, ct);
        var devices = await db.Devices.Include(d => d.Room).Where(d => d.IsActive).ToListAsync(ct);

        var todayKwh = days.FirstOrDefault(x => x.Date == today)?.Kwh ?? 0;
        var monthKwh = monthRows.Sum(x => x.Kwh);
        var prevMonthKwh = prevRows.Sum(x => x.Kwh);
        var monthCost = EnergyMath.Cost(monthKwh, price);
        var estimateDaily = devices.Sum(d => EnergyMath.DailyKwh(d.PowerWatts, d.HoursPerDay));
        var estimateMonthly = Math.Round(estimateDaily * 30, 2);
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var elapsed = today.Day;
        var forecastKwh = DashboardCalc.ForecastKwh(monthKwh, elapsed, daysInMonth);
        var forecastCost = EnergyMath.Cost(forecastKwh, price);
        var exhaustion = DashboardCalc.ExhaustionDate(
            monthStart,
            elapsed,
            daysInMonth,
            monthCost,
            forecastCost,
            tariff.MonthlyBudgetPln);
        var overLimit = DashboardCalc.OverDailyLimit(todayKwh, tariff.DailyLimitKwh);
        var budgetUsed = DashboardCalc.BudgetUsedPercent(monthCost, tariff.MonthlyBudgetPln);

        return Ok(new DashboardDto(
            todayKwh,
            EnergyMath.Cost(todayKwh, price),
            monthKwh,
            monthCost,
            prevMonthKwh,
            Math.Round(estimateDaily, 2),
            estimateMonthly,
            EnergyMath.Cost(estimateMonthly, price),
            tariff.DailyLimitKwh,
            tariff.MonthlyBudgetPln,
            budgetUsed,
            overLimit,
            forecastKwh,
            forecastCost,
            exhaustion,
            daysInMonth - elapsed,
            DashboardCalc.Alerts(
                todayKwh,
                estimateDaily,
                monthCost,
                budgetUsed,
                forecastCost,
                today,
                exhaustion,
                tariff,
                overLimit),
            days.Select(x => new ChartPointDto(x.Date, x.Kwh, EnergyMath.Cost(x.Kwh, price))).ToList(),
            DashboardCalc.TopDevices(devices, price),
            DashboardCalc.RoomShares(devices, price, daysInMonth)));
    }

    private async Task<List<DailyConsumption>> LoadDays(DateOnly from, DateOnly to, CancellationToken ct)
    {
        return await db.DailyConsumptions
            .Where(x => x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
    }
}
