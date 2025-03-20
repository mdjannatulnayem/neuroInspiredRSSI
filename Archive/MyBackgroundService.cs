// MyBackgroundService.cs

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSCOVR_Archive.Models;
using RSSI_Nuro.Models;
using System.Text;


namespace DSCOVR_Archive;

public class MyBackgroundService : BackgroundService
{
    readonly HttpClient _client;
    private readonly ILogger<MyBackgroundService> _log;

    SatelliteDataModel? satData = new();
    GeoMagnetDataModel? earthData = new();

    private readonly IConfiguration _conf;
    private readonly string? _token;

    public MyBackgroundService(ILogger<MyBackgroundService> log, IHttpClientFactory cf, IConfiguration conf)
    {
        _log = log;
        _client = cf.CreateClient();
        _conf = conf;
        _token = _conf["Auth:token"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("MyBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _log.LogInformation("MyBackgroundService is doing background work.");

            try
            {
                satData = await GetSatelliteData();
                earthData = await GetGeoMagneticData();

                await Save( new ArchiveDataModel { 
                    Timestamp = DateTime.UtcNow,
                    BxGSM = Convert.ToSingle(
                        satData.BxGSM
                    ),
                    ByGSM = Convert.ToSingle(
                        satData.ByGSM
                    ),
                    BzGSM = Convert.ToSingle(
                        satData.BzGSM
                    ),
                    Bt = Convert.ToSingle(
                        satData.Bt
                    ),
                    Speed = Convert.ToSingle(
                        satData.Speed
                    ),
                    Density = Convert.ToSingle(
                        satData.Density
                    ),
                    Temperature = Convert.ToSingle(
                        satData.Temperature
                    ),
                    Intensity = Convert.ToSingle(
                        earthData.Intensity
                    ),
                    Declination = Convert.ToSingle(
                        earthData.Declination
                    ),
                    Inclination = Convert.ToSingle(
                        earthData.Inclination
                    ),
                    North = Convert.ToSingle(
                        earthData.North
                    ),
                    East = Convert.ToSingle(
                        earthData.East
                    ),
                    Vertical = Convert.ToSingle(
                        earthData.Vertical
                    ),
                    Horizontal = Convert.ToSingle(
                        earthData.Horizontal
                    )
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
            finally
            {
                // Simulate doing some background work
                await Task.Delay(TimeSpan.FromSeconds(3600), stoppingToken);
            }

        }

        _log.LogInformation("MyBackgroundService is stopping.");
    }


    private async Task<GeoMagnetDataModel?> GetGeoMagneticData()
    {
        try
        {
            GeoMagnetDataModel? data = null;

            double northMagPoleLat = 86.50;
            double northMagPoleLon = 164.04;
            var date = DateTime.UtcNow;

            string url = $"https://www.ngdc.noaa.gov/geomag-web/calculators/calculateIgrfwmm?lat1={northMagPoleLat}&lon1={northMagPoleLon}&model=WMM&startYear={date.Year}&endYear={date.Year}&key=EAU2y&resultFormat=json";

            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                JObject jsonObject = JObject.Parse(responseBody);

                JToken? result = jsonObject["result"]?.FirstOrDefault();

                if (result != null)
                {
                    data = new GeoMagnetDataModel
                    {
                        Time = date,
                        Latitude = result["latitude"].ToObject<double>(),
                        Longitude = result["longitude"].ToObject<double>(),
                        Altitude = result["elevation"].ToObject<double>(),
                        Intensity = result["totalintensity"].ToObject<double>(),
                        Declination = result["declination"].ToObject<double>(),
                        Inclination = result["inclination"].ToObject<double>(),
                        North = result["xcomponent"].ToObject<double>(),
                        East = result["ycomponent"].ToObject<double>(),
                        Vertical = result["zcomponent"].ToObject<double>(),
                        Horizontal = result["horintensity"].ToObject<double>()
                    };
                }
            }

            return data;
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
            return null;
        }
    }

    private async Task<SatelliteDataModel?> GetSatelliteData()
    {
        try
        {
            SatelliteDataModel? model = null;

            string magUrl = @"https://services.swpc.noaa.gov/products/solar-wind/mag-5-minute.json";
            string plasmaUrl = @"https://services.swpc.noaa.gov/products/solar-wind/plasma-5-minute.json";

            // Fetch magnetometer data
            var magResponse = await _client.GetAsync(magUrl);

            if (magResponse.IsSuccessStatusCode)
            {
                string magResponseBody = await magResponse.Content.ReadAsStringAsync();
                var magData = JsonConvert.DeserializeObject<object[][]>(magResponseBody);
                var magLen = magData.Length;

                if (magLen > 1)
                {
                    model = new SatelliteDataModel
                    {
                        Time = DateTime.UtcNow, // Use the timestamp from data if preferred
                        BxGSM = Convert.ToDouble(magData[magLen - 1][1]),
                        ByGSM = Convert.ToDouble(magData[magLen - 1][2]),
                        BzGSM = Convert.ToDouble(magData[magLen - 1][3]),
                        Bt = Convert.ToDouble(magData[magLen - 1][6]),
                    };
                }
            }

            // Fetch plasma data
            var plasmaResponse = await _client.GetAsync(plasmaUrl);

            if (plasmaResponse.IsSuccessStatusCode)
            {
                string plasmaResponseBody = await plasmaResponse.Content.ReadAsStringAsync();
                var plasmaData = JsonConvert.DeserializeObject<object[][]>(plasmaResponseBody);
                var plasmaLen = plasmaData.Length;

                if (plasmaLen > 1 && model != null)
                {
                    model.Density = Convert.ToDouble(plasmaData[plasmaLen - 1][1]);
                    model.Speed = Convert.ToDouble(plasmaData[plasmaLen - 1][2]);
                    model.Temperature = Convert.ToDouble(plasmaData[plasmaLen - 1][3]);
                }
            }

            return model;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error fetching DSCOVR data");
            return new SatelliteDataModel { Error = ex.Message };
        }
    }

    private async Task Save(ArchiveDataModel data)
    {
        string url = @"https://app-rssi-new-api-sea-dev.azurewebsites.net/api/feedback/archive/";

        var json = JsonConvert.SerializeObject(data);
        var payload = new StringContent(json, Encoding.UTF8, "application/json");

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("x-api-key", _token);

        var response = await _client.PostAsync(url, payload);

        if (response.IsSuccessStatusCode)
        {
            _log.LogInformation("Saved to archive successfully.");
        }
        else
        {
            _log.LogError($"Failed to save: {response.StatusCode}");
        }
    }


    ///////////Eof
}
