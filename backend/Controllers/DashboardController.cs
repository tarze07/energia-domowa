using EnergiaDomowa.Api.Data;
using EnergiaDomowa.Api.Dtos;
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

        var days = await db.DailyConsumptions
            .Where(x => x.Date >= from && x.Date <= today)
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        var monthRows = await db.DailyConsumptions
            .Where(x => x.Date >= monthStart && x.Date <= today)
            .ToListAsync(ct);

        var prevRows = await db.DailyConsumptions
            .Where(x => x.Date >= prevMonthStart && x.Date <= prevMonthEnd)
            .ToListAsync(ct);

        var todayRow = days.FirstOrDefault(x => x.Date == today);
        var todayKwh = todayRow?.Kwh ?? 0;
        var monthKwh = monthRows.Sum(x => x.Kwh);
        var prevMonthKwh = prevRows.Sum(x => x.Kwh);
        var monthCost = EnergyMath.Cost(monthKwh, price);

        var devices = await db.Devices.Include(d => d.Room).Where(d => d.IsActive).ToListAsync(ct);
        var estimateDaily = devices.Sum(d => EnergyMath.DailyKwh(d.PowerWatts, d.HoursPerDay));
        var estimateMonthly = Math.Round(estimateDaily * 30, 2);
        var estimateCost = EnergyMath.Cost(estimateMonthly, price);

        var top = devices
            .Select(d =>
            {
                var daily = EnergyMath.DailyKwh(d.PowerWatts, d.HoursPerDay);
                var monthly = Math.Round(daily * 30, 2);
                return new TopDeviceDto(d.Name, d.Room.Name, daily, monthly, EnergyMath.Cost(monthly, price));
            })
            .OrderByDescending(d => d.DailyKwh)
            .Take(6)
            .ToList();

        var chart = days.Select(x => new ChartPointDto(x.Date, x.Kwh, EnergyMath.Cost(x.Kwh, price))).ToList();
        var overLimit = todayKwh > tariff.DailyLimitKwh && tariff.DailyLimitKwh > 0;
        var budgetUsed = tariff.MonthlyBudgetPln <= 0 ? 0 : Math.Round(monthCost / tariff.MonthlyBudgetPln * 100, 1);

        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var elapsed = today.Day;
        var daysLeft = daysInMonth - elapsed;
        var forecastKwh = elapsed > 0 ? Math.Round(monthKwh / elapsed * daysInMonth, 3) : 0;
        var forecastCost = EnergyMath.Cost(forecastKwh, price);
        DateOnly? exhaustion = null;
        if (tariff.MonthlyBudgetPln > 0 && monthCost > 0 && forecastCost >= tariff.MonthlyBudgetPln)
        {
            var avgCost = monthCost / elapsed;
            var dayNumber = (int)Math.Ceiling(tariff.MonthlyBudgetPln / avgCost);
            dayNumber = Math.Clamp(dayNumber, 1, daysInMonth);
            exhaustion = monthStart.AddDays(dayNumber - 1);
        }

        var roomGroups = devices
            .GroupBy(d => d.Room.Name)
            .Select(g =>
            {
                var daily = g.Sum(d => EnergyMath.DailyKwh(d.PowerWatts, d.HoursPerDay));
                var monthly = Math.Round(daily * daysInMonth, 2);
                return new
                {
                    Name = g.Key,
                    Daily = daily,
                    Monthly = monthly,
                    Cost = EnergyMath.Cost(monthly, price)
                };
            })
            .Where(x => x.Daily > 0)
            .OrderByDescending(x => x.Daily)
            .ToList();
        var roomTotal = roomGroups.Sum(x => x.Daily);
        var rooms = roomGroups
            .Select(x => new RoomShareDto(
                x.Name,
                Math.Round(x.Daily, 3),
                x.Monthly,
                x.Cost,
                roomTotal <= 0 ? 0 : Math.Round(x.Daily / roomTotal * 100, 1)))
            .ToList();

        var alerts = new List<string>();
        if (overLimit)
        {
            alerts.Add($"Dzisiejsze zużycie {todayKwh:0.##} kWh przekroczyło limit {tariff.DailyLimitKwh:0.##} kWh.");
        }

        if (budgetUsed >= 90)
        {
            alerts.Add($"Wykorzystano {budgetUsed:0.#}% miesięcznego budżetu ({monthCost:0.00} zł z {tariff.MonthlyBudgetPln:0.00} zł).");
        }

        if (todayKwh > estimateDaily * 1.25m && estimateDaily > 0)
        {
            alerts.Add("Zużycie jest wyraźnie wyższe niż suma szacunków urządzeń — sprawdź nieznane odbiorniki.");
        }

        if (tariff.MonthlyBudgetPln > 0 && forecastCost > tariff.MonthlyBudgetPln)
        {
            var forecastAlert =
                $"Przy obecnym tempie miesiąc zamknie się na {forecastCost:0.00} zł (budżet {tariff.MonthlyBudgetPln:0.00} zł).";
            if (exhaustion is not null && exhaustion > today)
            {
                forecastAlert += $" Budżet skończy się {exhaustion.Value:dd.MM}.";
            }

            alerts.Add(forecastAlert);
        }

        return Ok(new DashboardDto(
            todayKwh,
            EnergyMath.Cost(todayKwh, price),
            monthKwh,
            monthCost,
            prevMonthKwh,
            Math.Round(estimateDaily, 2),
            estimateMonthly,
            estimateCost,
            tariff.DailyLimitKwh,
            tariff.MonthlyBudgetPln,
            budgetUsed,
            overLimit,
            forecastKwh,
            forecastCost,
            exhaustion,
            daysLeft,
            alerts,
            chart,
            top,
            rooms));
    }
}
