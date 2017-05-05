using System.Collections.Generic;
using Newtonsoft.Json;

namespace funkyBot.APIs
{
    public class ListTrekksOutput
    {
        [JsonProperty("data")]
        public Dictionary<string, object>[] Data { get; set; }
    }
}