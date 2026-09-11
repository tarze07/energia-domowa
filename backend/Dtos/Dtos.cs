namespace EnergiaDomowa.Api.Dtos;

public record RoomDto(int Id, string Name, int DeviceCount);

public record UpsertRoomDto(string Name);

public record DeviceDto(
    int Id,
    string Name,
    int RoomId,
    string RoomName,
    int PowerWatts,
    decimal HoursPerDay,
    bool IsActive,
    decimal DailyKwh,
    decimal MonthlyKwh,
    decimal MonthlyCost);

public record UpsertDeviceDto(
    string Name,
    int RoomId,
    int PowerWatts,
    decimal HoursPerDay,
    bool IsActive);

public record MeterReadingDto(int Id, DateOnly Date, decimal TotalKwh, string? Note, decimal? DeltaKwh);

public record UpsertMeterReadingDto(DateOnly Date, decimal TotalKwh, string? Note);

public record DailyConsumptionDto(int Id, DateOnly Date, decimal Kwh, decimal Cost);

public record UpsertConsumptionDto(DateOnly Date, decimal Kwh);

public record TariffDto(int Id, string Name, decimal PricePerKwh, decimal DailyLimitKwh, decimal MonthlyBudgetPln);

public record UpsertTariffDto(string Name, decimal PricePerKwh, decimal DailyLimitKwh, decimal MonthlyBudgetPln);

public record ChartPointDto(DateOnly Date, decimal Kwh, decimal Cost);

public record TopDeviceDto(string Name, string RoomName, decimal DailyKwh, decimal MonthlyKwh, decimal MonthlyCost);

public record DashboardDto(
    decimal TodayKwh,
    decimal TodayCost,
    decimal MonthKwh,
    decimal MonthCost,
    decimal PrevMonthKwh,
    decimal DeviceEstimateDailyKwh,
    decimal DeviceEstimateMonthlyKwh,
    decimal DeviceEstimateMonthlyCost,
    decimal DailyLimitKwh,
    decimal MonthlyBudgetPln,
    decimal BudgetUsedPercent,
    bool OverDailyLimit,
    IReadOnlyList<string> Alerts,
    IReadOnlyList<ChartPointDto> Last30Days,
    IReadOnlyList<TopDeviceDto> TopDevices);
