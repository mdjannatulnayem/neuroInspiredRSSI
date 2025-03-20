using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RSSI_Nuro.Authorization;
using RSSI_Nuro.Models;
using RSSI_Nuro.Models.DtoModels;
using RSSI_Nuro.Repositories.Contracts;
using System.Text;
using Tweetinvi;
using Tweetinvi.Models;

namespace RSSI_Nuro.Controllers;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(AuthFilter))]
public class SatelliteDataController : ControllerBase
{
    private readonly IMapper _automap;
    private readonly IEarthDataRepository _repositoryEarthData;
    private readonly ISatelliteDataRepository _repositorySatData;
    private readonly IConfiguration _conf;
    private readonly string _apiKey;
    private readonly string _apiKeySecret;
    private readonly string _accessToken;
    private readonly string _accessTokenSecret;

    /// Dummy
    static bool dummyRequested;
    static DateTime time = new();
    static SatelliteDataDtoModel dummy = new();
    
    public SatelliteDataController
    (
        IEarthDataRepository earthData,
        ISatelliteDataRepository satData,
        IMapper mp,
        IConfiguration cfg)
    {
        _automap = mp;
        _repositoryEarthData = earthData;
        _repositorySatData = satData;
        

        _conf = cfg;
        _apiKey = _conf["Xbot:ApiKey"];
        _apiKeySecret = _conf["Xbot:ApiKeySecret"];
        _accessToken = _conf["Xbot:AccessToken"];
        _accessTokenSecret = _conf["Xbot:AccessTokenSecret"];
    }

    [HttpGet("dscovr")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetDscovrData()
    {
        if(dummyRequested){
            if((DateTime.UtcNow - time).TotalMilliseconds <= 10000)
                return Ok(dummy);
            else 
                dummyRequested = false;
        }

        var data = await _repositorySatData.GetDscovrRealtimeData();
        if (data == null)
            return NoContent();
        if (data.Error != null)
            return StatusCode(500, new { message = "Internal Server Error", error = data.Error });
        return Ok(_automap.Map<SatelliteDataDtoModel>(data));
    }

    [HttpGet("ace")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetAceData()
    {
        if(dummyRequested){
            if((DateTime.UtcNow - time).TotalMilliseconds <= 10000)
                return Ok(dummy);
            else 
                dummyRequested = false;
        }

        var data = await _repositorySatData.GetAceRealtimeData();
        if (data == null)
            return NoContent();
        if (data.Error != null)
            return StatusCode(500, new { message = "Internal Server Error", error = data.Error });
        return Ok(_automap.Map<SatelliteDataDtoModel>(data));
    }


    [HttpPost("dummy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetDummyData([FromBody] SatelliteDataDtoModel data){
        if(!ModelState.IsValid)
            return BadRequest("Invalid model state!");
        dummyRequested = true;
        time = DateTime.UtcNow;
        dummy = data;

        var nceiDataResult = await GetGeoMagneticDataNCEI();
        if (nceiDataResult != null && data.BzGSM < 0)
        {
            var earthDataVertical = nceiDataResult.Vertical;
            if (Math.Abs(earthDataVertical + data.BzGSM) <= 1000)
                return Ok(await PostTweet(new TweetReqDtoModel{Text = $"DUMMY - Magnetic Reconnection Iminent!\nTimestamp:{DateTime.UtcNow}\nGeomagnetic z-component:{earthDataVertical} | Solarwind z-component: {data.BzGSM}"}));
        }
        return Ok();
    }


    private static Action<ITwitterRequest> BuildTwitterRequest(
        TweetReqDtoModel tweet, TwitterClient client)
    {
        return (ITwitterRequest request) => 
        {
            var json = client.Json.Serialize(tweet);
            var content = new StringContent(json,Encoding.UTF8,"application/json");
            request.Query.Url = "https://api.twitter.com/2/tweets";
            request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
            request.Query.HttpContent = content;
        };
    }


    private async Task<string> PostTweet(TweetReqDtoModel newTweet)
    {
        var client = new TwitterClient(_apiKey, _apiKeySecret, _accessToken, _accessTokenSecret);
        var result = await client.Execute.AdvanceRequestAsync(
            BuildTwitterRequest(newTweet, client)
        );
        return result.Content;
    }


    private async Task<GeoMagnetDataModel?> GetGeoMagneticDataNCEI()
    {
        return await _repositoryEarthData.GetGeoMagneticDataFromNCEI();
    }
}
