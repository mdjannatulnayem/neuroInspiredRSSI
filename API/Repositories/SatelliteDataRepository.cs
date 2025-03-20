using Newtonsoft.Json;
using RSSI_Nuro.Models;
using RSSI_Nuro.Repositories.Contracts;

namespace RSSI_Nuro.Repositories;

public class SatelliteDataRepository : ISatelliteDataRepository
{
    private readonly ILogger _log;
    private readonly HttpClient _client;

    public SatelliteDataRepository(IHttpClientFactory cf, ILogger<SatelliteDataRepository> lg)
    {
        _log = lg;
        _client = cf.CreateClient();
    }


    public async Task<SatelliteDataModel?> GetDscovrRealtimeData()
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



    public async Task<SatelliteDataModel?> GetAceRealtimeData()
    {
        try
        {
            SatelliteDataModel? model = null;

            // URLs for magnetometer and plasma data
            string magnetometerUrl = @"https://services.swpc.noaa.gov/text/ace-magnetometer.txt";
            string plasmaUrl = @"https://services.swpc.noaa.gov/text/ace-swepam.txt";

            // Fetch magnetometer data
            var magnetometerResponse = await _client.GetAsync(magnetometerUrl);
            if (magnetometerResponse.IsSuccessStatusCode)
            {
                string magnetometerResponseBody = await magnetometerResponse.Content.ReadAsStringAsync();
                string[] magnetometerLines = magnetometerResponseBody.Split('\n');
                string magnetometerLine = magnetometerLines[magnetometerLines.Length - 2].Trim();
                string[] magnetometerFields = magnetometerLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (magnetometerFields.Length == 13)
                {
                    // Extract hours from HHMM
                    int hour = int.Parse(magnetometerFields[3].Substring(0, 2));
                    // Extract minutes from HHMM
                    int minute = int.Parse(magnetometerFields[3].Substring(2, 2));
                    double bx = double.Parse(magnetometerFields[7]);
                    double by = double.Parse(magnetometerFields[8]);
                    double bz = double.Parse(magnetometerFields[9]);
                    double bt = double.Parse(magnetometerFields[10]);
                    int year = int.Parse(magnetometerFields[0]);
                    int month = int.Parse(magnetometerFields[1]);
                    int day = int.Parse(magnetometerFields[2]);

                    model = new SatelliteDataModel
                    {
                        Time = DateTime.UtcNow,
                        // Time = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc),
                        Bt = bt,
                        BxGSM = bx,
                        ByGSM = by,
                        BzGSM = bz
                    };
                }
            }

            // Fetch plasma data
            var plasmaResponse = await _client.GetAsync(plasmaUrl);
            if (plasmaResponse.IsSuccessStatusCode)
            {
                string plasmaResponseBody = await plasmaResponse.Content.ReadAsStringAsync();
                string[] plasmaLines = plasmaResponseBody.Split('\n');
                string plasmaLine = plasmaLines[plasmaLines.Length - 2].Trim();
                string[] plasmaFields = plasmaLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (plasmaFields.Length == 10)
                {
                    double density = double.Parse(plasmaFields[7]);
                    double speed = double.Parse(plasmaFields[8]);
                    double temperature = double.Parse(plasmaFields[9]);

                    // Check for missing data values
                    if (density == -9999.9) density = double.NaN;
                    if (speed == -9999.9) speed = double.NaN;
                    if (temperature == -1.00e+05) temperature = double.NaN;

                    if (model == null)
                    {
                        model = new SatelliteDataModel
                        {
                            Time = DateTime.UtcNow
                        };
                    }

                    model.Density = density;
                    model.Speed = speed;
                    model.Temperature = temperature;
                }
            }

            return model;
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
            SatelliteDataModel data = new()
            {
                Error = ex.Message
            };
            return data;
        }
    }


}
