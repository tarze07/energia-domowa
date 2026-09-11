using EnergiaDomowa.Api.Models;

namespace EnergiaDomowa.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Rooms.Any())
        {
            return;
        }

        var salon = new Room { Name = "Salon" };
        var kuchnia = new Room { Name = "Kuchnia" };
        var lazienka = new Room { Name = "Łazienka" };
        var sypialnia = new Room { Name = "Sypialnia" };
        var gabinet = new Room { Name = "Gabinet" };
        var garaż = new Room { Name = "Garaż" };

        db.Rooms.AddRange(salon, kuchnia, lazienka, sypialnia, gabinet, garaż);
        db.SaveChanges();

        db.Devices.AddRange(
            new Device { Name = "Telewizor", RoomId = salon.Id, PowerWatts = 90, HoursPerDay = 4.5m, IsActive = true },
            new Device { Name = "Oświetlenie LED", RoomId = salon.Id, PowerWatts = 48, HoursPerDay = 5m, IsActive = true },
            new Device { Name = "Konsola", RoomId = salon.Id, PowerWatts = 140, HoursPerDay = 1.5m, IsActive = true },
            new Device { Name = "Lodówka", RoomId = kuchnia.Id, PowerWatts = 120, HoursPerDay = 24m, IsActive = true },
            new Device { Name = "Zmywarka", RoomId = kuchnia.Id, PowerWatts = 1800, HoursPerDay = 0.6m, IsActive = true },
            new Device { Name = "Płyta indukcyjna", RoomId = kuchnia.Id, PowerWatts = 2800, HoursPerDay = 0.7m, IsActive = true },
            new Device { Name = "Czajnik", RoomId = kuchnia.Id, PowerWatts = 2200, HoursPerDay = 0.2m, IsActive = true },
            new Device { Name = "Piekarnik", RoomId = kuchnia.Id, PowerWatts = 2400, HoursPerDay = 0.4m, IsActive = true },
            new Device { Name = "Bojler", RoomId = lazienka.Id, PowerWatts = 2000, HoursPerDay = 1.4m, IsActive = true },
            new Device { Name = "Pralka", RoomId = lazienka.Id, PowerWatts = 2000, HoursPerDay = 0.5m, IsActive = true },
            new Device { Name = "Suszarka do włosów", RoomId = lazienka.Id, PowerWatts = 1600, HoursPerDay = 0.15m, IsActive = true },
            new Device { Name = "Lampa nocna", RoomId = sypialnia.Id, PowerWatts = 12, HoursPerDay = 1.5m, IsActive = true },
            new Device { Name = "Komputer", RoomId = gabinet.Id, PowerWatts = 280, HoursPerDay = 7m, IsActive = true },
            new Device { Name = "Monitor", RoomId = gabinet.Id, PowerWatts = 35, HoursPerDay = 7m, IsActive = true },
            new Device { Name = "Router", RoomId = gabinet.Id, PowerWatts = 12, HoursPerDay = 24m, IsActive = true },
            new Device { Name = "Ładowarka EV", RoomId = garaż.Id, PowerWatts = 3700, HoursPerDay = 1.2m, IsActive = false });

        db.Tariffs.Add(new Tariff
        {
            Name = "G11 — taryfa całodobowa",
            PricePerKwh = 1.12m,
            DailyLimitKwh = 12m,
            MonthlyBudgetPln = 320m
        });

        var today = DateOnly.FromDateTime(DateTime.Today);
        var rng = new Random(42);
        var meter = 18420.4m;

        for (var i = 45; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var weekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var baseKwh = weekend ? 11.4m : 8.6m;
            var noise = (decimal)(rng.NextDouble() * 2.2 - 0.8);
            var kwh = Math.Round(Math.Max(5.2m, baseKwh + noise), 2);
            db.DailyConsumptions.Add(new DailyConsumption { Date = date, Kwh = kwh });
            meter += kwh;

            if (i % 7 == 0)
            {
                db.MeterReadings.Add(new MeterReading
                {
                    Date = date,
                    TotalKwh = Math.Round(meter, 1),
                    Note = i == 0 ? "Odczyt bieżący" : "Odczyt tygodniowy"
                });
            }
        }

        db.SaveChanges();
    }
}
