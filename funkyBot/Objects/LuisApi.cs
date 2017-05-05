using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace funkyBot.Objects
{
    public class LuisApi
    {
        public static async Task<LuisResult> GetLuisResult(string query)
        {
            string modelId = "";
            string subscriptionKey = "";

            string luisUrl = $"https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{modelId}?subscription-key={subscriptionKey}&staging=true&verbose=true&q={HttpUtility.HtmlEncode(query)}";

            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync(luisUrl);

            LuisResult luisResponse = JsonConvert.DeserializeObject<LuisResult>(response);

            return luisResponse;
        }
    }
}