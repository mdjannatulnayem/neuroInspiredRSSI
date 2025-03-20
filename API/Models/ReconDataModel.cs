
namespace RSSI_Nuro.Models;

public class ReconDataModel
{
    [System.ComponentModel.DataAnnotations.Key]
    public DateTime Timestamp { get; set; }
    public float BxGSM { get; set; }
    public float ByGSM { get; set; }
    public float BzGSM { get; set; }
    public float Bt { get; set; }
    public float Density { get; set; }
    public float Speed { get; set; }
    public float Temperature { get; set; }

}
