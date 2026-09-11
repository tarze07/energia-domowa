namespace EnergiaDomowa.Api.Models;

public class Tariff
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerKwh { get; set; }
    public decimal DailyLimitKwh { get; set; }
    public decimal MonthlyBudgetPln { get; set; }
}
