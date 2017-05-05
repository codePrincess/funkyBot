using Newtonsoft.Json;

namespace funkyBot.APIs
{
    public class StopTrekkInput
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}