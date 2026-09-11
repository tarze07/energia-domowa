namespace EnergiaDomowa.Api.Data;

public static class EnergyMath
{
    public static decimal DailyKwh(int watts, decimal hoursPerDay) =>
        Math.Round(watts / 1000m * hoursPerDay, 3);

    public static decimal Cost(decimal kwh, decimal pricePerKwh) =>
        Math.Round(kwh * pricePerKwh, 2);
}
