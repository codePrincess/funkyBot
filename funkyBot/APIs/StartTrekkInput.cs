using Newtonsoft.Json;

namespace funkyBot.APIs
{
    public class StartTrekkInput
    {
        [JsonProperty("DisplayText")]
        public string DisplayText { get; set; }

        [JsonProperty("LicenseNumber")]
        public string LicenseNumber { get; set; }

        [JsonProperty("Km")]
        public int Km { get; set; }
    }
}