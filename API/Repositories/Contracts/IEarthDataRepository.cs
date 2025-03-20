using RSSI_Nuro.Models;

namespace RSSI_Nuro.Repositories.Contracts;

public interface IEarthDataRepository
{
    Task<GeoMagnetDataModel?> GetGeoMagneticDataFromBGS();
    Task<GeoMagnetDataModel?> GetGeoMagneticDataFromNCEI();
}
