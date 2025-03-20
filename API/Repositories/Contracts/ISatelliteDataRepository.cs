using RSSI_Nuro.Models;

namespace RSSI_Nuro.Repositories.Contracts;

public interface ISatelliteDataRepository
{
    Task<SatelliteDataModel?> GetAceRealtimeData();
    Task<SatelliteDataModel?> GetDscovrRealtimeData();
}
