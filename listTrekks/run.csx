#r "Newtonsoft.Json"
using System.Net;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    MobileServiceClient client = new MobileServiceClient("https://mymobiletrekka.azurewebsites.net");
    var trekkaTable = await client.GetTable("Trekka").ReadAsync("");

    var responseData = new List<Dictionary<string, object>>();
    foreach (var record in trekkaTable)
    {

        var endTimestamp = record.Value<string>("stopedAt");
        if (endTimestamp != null)
        {
            continue;
        }

        Dictionary<string, object> trekka = new Dictionary<string, object>();
        trekka["id"] = record.Value<string>("id");
        trekka["displayText"] = record.Value<string>("displayText");

        responseData.Add(trekka);

        log.Info(trekka.ToString());
    }

    log.Info($"Got {trekkaTable.Count()} record(s).");

    return req.CreateResponse(HttpStatusCode.OK, new
        {
            data = responseData
        });
}
