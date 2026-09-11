namespace EnergiaDomowa.Api.Models;

public class DailyConsumption
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Kwh { get; set; }
}
