using EnergiaDomowa.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnergiaDomowa.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<DailyConsumption> DailyConsumptions => Set<DailyConsumption>();
    public DbSet<Tariff> Tariffs => Set<Tariff>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>().Property(x => x.Name).HasMaxLength(80).IsRequired();
        modelBuilder.Entity<Device>().Property(x => x.Name).HasMaxLength(80).IsRequired();
        modelBuilder.Entity<Device>().Property(x => x.HoursPerDay).HasPrecision(6, 2);
        modelBuilder.Entity<MeterReading>().Property(x => x.TotalKwh).HasPrecision(12, 3);
        modelBuilder.Entity<MeterReading>().HasIndex(x => x.Date).IsUnique();
        modelBuilder.Entity<DailyConsumption>().Property(x => x.Kwh).HasPrecision(10, 3);
        modelBuilder.Entity<DailyConsumption>().HasIndex(x => x.Date).IsUnique();
        modelBuilder.Entity<Tariff>().Property(x => x.PricePerKwh).HasPrecision(8, 4);
        modelBuilder.Entity<Tariff>().Property(x => x.DailyLimitKwh).HasPrecision(8, 2);
        modelBuilder.Entity<Tariff>().Property(x => x.MonthlyBudgetPln).HasPrecision(10, 2);
    }
}
