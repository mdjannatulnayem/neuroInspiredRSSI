
namespace RSSI_Nuro.Models;

public class ArchiveDataModel
{
    [System.ComponentModel.DataAnnotations.Key]
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public float BxGSM { get; set; }
    public float ByGSM { get; set; }
    public float BzGSM { get; set; }
    public float Bt { get; set; }
    public float Density { get; set; }
    public float Speed { get; set; }
    public float Temperature { get; set; }
    public float Intensity { get; set; }
    public float Declination { get; set; }
    public float Inclination { get; set; }
    public float North { get; set; }
    public float East { get; set; }
    public float Vertical { get; set; }
    public float Horizontal { get; set; }
}
