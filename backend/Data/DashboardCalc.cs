using EnergiaDomowa.Api.Dtos;
using EnergiaDomowa.Api.Models;

namespace EnergiaDomowa.Api.Data;

public static class DashboardCalc
{
    public static bool OverDailyLimit(decimal todayKwh, decimal dailyLimitKwh)
    {
        if (dailyLimitKwh <= 0)
        {
            return false;
        }

        return todayKwh > dailyLimitKwh;
    }

    public static decimal BudgetUsedPercent(decimal monthCost, decimal monthlyBudgetPln)
    {
        if (monthlyBudgetPln <= 0)
        {
            return 0;
        }

        return Math.Round(monthCost / monthlyBudgetPln * 100, 1);
    }

    public static decimal ForecastKwh(decimal monthKwh, int elapsed, int daysInMonth)
    {
        if (elapsed <= 0)
        {
            return 0;
        }

        return Math.Round(monthKwh / elapsed * daysInMonth, 3);
    }

    public static DateOnly? ExhaustionDate(
        DateOnly monthStart,
        int elapsed,
        int daysInMonth,
        decimal monthCost,
        decimal forecastCost,
        decimal monthlyBudgetPln)
    {
        if (monthlyBudgetPln <= 0)
        {
            return null;
        }

        if (monthCost <= 0)
        {
            return null;
        }

        if (forecastCost < monthlyBudgetPln)
        {
            return null;
        }

        var avgCost = monthCost / elapsed;
        var dayNumber = (int)Math.Ceiling(monthlyBudgetPln / avgCost);
        dayNumber = Math.Clamp(dayNumber, 1, daysInMonth);
        return monthStart.AddDays(dayNumber - 1);
    }

    public static List<TopDeviceDto> TopDevices(IEnumerable<Device> devices, decimal price)
    {
        return devices
            .Select(d =>
            {
                var daily = EnergyMath.DailyKwh(d.PowerWatts, d.HoursPerDay);
                var monthly = Math.Round(daily * 30, 2);
                return new TopDeviceDto(d.Name, d.Room.Name, daily, monthly, EnergyMath.Cost(monthly, price));
            })
            .OrderByDescending(d => d.DailyKwh)
            .Take(6)
            .ToList();
    }

    public static List<RoomShareDto> RoomShares(IEnumerable<Device> devices, decimal price, int daysInMonth)
    {
        var groups = devices
            .GroupBy(d => d.Room.Name)
            .Select(g => ToRoomGroup(g, price, daysInMonth))
            .Where(x => x.Daily > 0)
            .OrderByDescending(x => x.Daily)
            .ToList();

        var total = groups.Sum(x => x.Daily);
        return groups
            .Select(x => new RoomShareDto(
                x.Name,
                Math.Round(x.Daily, 3),
                x.Monthly,
                x.Cost,
                SharePercent(x.Daily, total)))
            .ToList();
    }

    public static List<string> Alerts(
        decimal todayKwh,
        decimal estimateDaily,
        decimal monthCost,
        decimal budgetUsed,
        decimal forecastCost,
        DateOnly today,
        DateOnly? exhaustion,
        Tariff tariff,
        bool overLimit)
    {
        var alerts = new List<string>();
        AddOverLimitAlert(alerts, todayKwh, tariff, overLimit);
        AddBudgetAlert(alerts, monthCost, budgetUsed, tariff);
        AddEstimateAlert(alerts, todayKwh, estimateDaily);
        AddForecastAlert(alerts, forecastCost, today, exhaustion, tariff);
        return alerts;
    }

    private static (string Name, decimal Daily, decimal Monthly, decimal Cost) ToRoomGroup(
        IGrouping<string, Device> group,
        decimal price,
        int daysInMonth)
    {
        var daily = group.Sum(d => EnergyMath.DailyKwh(d.PowerWatts, d.HoursPerDay));
        var monthly = Math.Round(daily * daysInMonth, 2);
        return (group.Key, daily, monthly, EnergyMath.Cost(monthly, price));
    }

    private static decimal SharePercent(decimal daily, decimal total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return Math.Round(daily / total * 100, 1);
    }

    private static void AddOverLimitAlert(List<string> alerts, decimal todayKwh, Tariff tariff, bool overLimit)
    {
        if (!overLimit)
        {
            return;
        }

        alerts.Add($"Dzisiejsze zużycie {todayKwh:0.##} kWh przekroczyło limit {tariff.DailyLimitKwh:0.##} kWh.");
    }

    private static void AddBudgetAlert(List<string> alerts, decimal monthCost, decimal budgetUsed, Tariff tariff)
    {
        if (budgetUsed < 90)
        {
            return;
        }

        alerts.Add($"Wykorzystano {budgetUsed:0.#}% miesięcznego budżetu ({monthCost:0.00} zł z {tariff.MonthlyBudgetPln:0.00} zł).");
    }

    private static void AddEstimateAlert(List<string> alerts, decimal todayKwh, decimal estimateDaily)
    {
        if (estimateDaily <= 0)
        {
            return;
        }

        if (todayKwh <= estimateDaily * 1.25m)
        {
            return;
        }

        alerts.Add("Zużycie jest wyraźnie wyższe niż suma szacunków urządzeń — sprawdź nieznane odbiorniki.");
    }

    private static void AddForecastAlert(
        List<string> alerts,
        decimal forecastCost,
        DateOnly today,
        DateOnly? exhaustion,
        Tariff tariff)
    {
        if (tariff.MonthlyBudgetPln <= 0)
        {
            return;
        }

        if (forecastCost <= tariff.MonthlyBudgetPln)
        {
            return;
        }

        var text =
            $"Przy obecnym tempie miesiąc zamknie się na {forecastCost:0.00} zł (budżet {tariff.MonthlyBudgetPln:0.00} zł).";
        if (exhaustion is null)
        {
            alerts.Add(text);
            return;
        }

        if (exhaustion <= today)
        {
            alerts.Add(text);
            return;
        }

        alerts.Add(text + $" Budżet skończy się {exhaustion.Value:dd.MM}.");
    }
}
