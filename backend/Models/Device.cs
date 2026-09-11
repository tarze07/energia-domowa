namespace EnergiaDomowa.Api.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public int PowerWatts { get; set; }
    public decimal HoursPerDay { get; set; }
    public bool IsActive { get; set; } = true;
}
