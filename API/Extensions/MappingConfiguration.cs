using AutoMapper;
using RSSI_Nuro.Models;
using RSSI_Nuro.Models.DtoModels;

namespace RSSI_Nuro.Extensions;

public class MappingConfiguration : Profile
{
    public MappingConfiguration()
    {
        CreateMap<SatelliteDataModel, SatelliteDataDtoModel>()
            .ReverseMap();
    }
}
