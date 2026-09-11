namespace EnergiaDomowa.Api.Models;

public class MeterReading
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal TotalKwh { get; set; }
    public string? Note { get; set; }
}
