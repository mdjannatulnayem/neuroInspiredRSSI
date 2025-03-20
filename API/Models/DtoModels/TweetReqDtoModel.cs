using Newtonsoft.Json;

namespace RSSI_Nuro.Models.DtoModels;

public class TweetReqDtoModel
{
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;
}
