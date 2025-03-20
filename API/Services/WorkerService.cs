using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RSSI_Nuro.Models;
using RSSI_Nuro.Models.DtoModels;
using System.Text;
using Tweetinvi.Models;
using Tweetinvi;


namespace RSSI_Nuro.Services;

public class WorkerService : BackgroundService
{
    readonly HttpClient _client;
    readonly ILogger<WorkerService> _log;

    SatelliteDataModel? satData = new();
    GeoMagnetDataModel? earthData = new();

    int delay;

    private readonly IConfiguration _conf;
    private readonly string _apiKey;
    private readonly string _apiKeySecret;
    private readonly string _accessToken;
    private readonly string _accessTokenSecret;

    public WorkerService(
        ILogger<WorkerService> log,
        IHttpClientFactory cf,
        IConfiguration conf
    ){
        _log = log;
        _client = cf.CreateClient();
        _conf = conf;
        _apiKey = _conf["Xbot:ApiKey"];
        _apiKeySecret = _conf["Xbot:ApiKeySecret"];
        _accessToken = _conf["Xbot:AccessToken"];
        _accessTokenSecret = _conf["Xbot:AccessTokenSecret"];
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        DateTime t = DateTime.UtcNow;
        while (!token.IsCancellationRequested)
        {
            try {
                satData = await GetSatelliteData();
                earthData = await GetGeoMagneticData();

                if (satData == null || earthData == null)
                {
                    delay = 1000;
                    continue;
                }else{
                    delay = 60000;
                }

                // check for large magnetic reconnection
                if (Math.Abs(earthData.Vertical + satData.BzGSM) <= 1000 && satData.BzGSM < 0)
                {
                    _log.LogInformation("{t} : Reconnection alert !!!", t);

                    await PostTweet(new TweetReqDtoModel
                    {
                        Text = $"Magnetic Reconnection Iminent!\nTimestamp:{t}\nGeomagnetic z-component:{earthData.Vertical} | Solarwind z-component:{satData.BzGSM}",
                    });

                    await SaveReconEventInfo(new ReconDataModel
                    {
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
                        Density = Convert.ToSingle(
                            satData.Density
                        ),
                        Speed = Convert.ToSingle(
                            satData.Speed
                        ),
                        Temperature = Convert.ToSingle(
                            satData.Temperature
                        )
                    });

                }

                // Done!
            } catch (Exception ex) {
                _log.LogError(ex.Message);
            } finally{
                await Task.Delay(3 * delay, token);
            }

            /// WHILE ///
        }
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


    private async Task PostTweet(TweetReqDtoModel newTweet)
    {
        var client = new TwitterClient(_apiKey, _apiKeySecret, _accessToken, _accessTokenSecret);
        await client.Execute.AdvanceRequestAsync(
            BuildTwitterRequest(newTweet, client)
        );

    }


    private static Action<ITwitterRequest> BuildTwitterRequest(
    TweetReqDtoModel tweet, TwitterClient client)
    {
        return (ITwitterRequest request) =>
        {
            var json = client.Json.Serialize(tweet);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Query.Url = "https://api.twitter.com/2/tweets";
            request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
            request.Query.HttpContent = content;
        };
    }

    private async Task SaveReconEventInfo(ReconDataModel data)
    {
        string url = @"https://app-rssi-new-api-sea-dev.azurewebsites.net/api/feedback/feedback";
        string apiKey = _conf["Auth:apiKey"];

        var json = JsonConvert.SerializeObject(data);
        var payload = new StringContent(json, Encoding.UTF8, "application/json");

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var response = await _client.PostAsync(url,payload);

        if (response.IsSuccessStatusCode)
        {
            _log.LogInformation("Reconnect event saved to database successfully.");
        }
        else
        {
            _log.LogError($"Failed to save reconnect event: {response.StatusCode}");
        }
    }

    /// End
}