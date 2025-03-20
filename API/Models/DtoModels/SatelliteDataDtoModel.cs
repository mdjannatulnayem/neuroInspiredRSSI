﻿namespace RSSI_Nuro.Models.DtoModels;

public class SatelliteDataDtoModel
{
    public DateTime Time { get; set; }
    public double Bt { get; set; }
    public double BxGSM { get; set; }
    public double ByGSM { get; set; }
    public double BzGSM { get; set; }
    public double Density { get; set; }
    public double Speed { get; set; }
    public double Temperature { get; set; }

}
