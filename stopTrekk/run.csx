using System.Net;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    dynamic data = req.Content.ReadAsAsync<object>().Result;
    string id = Convert.ToString(data?.id);

    MobileServiceClient client = new MobileServiceClient("https://mymobiletrekka.azurewebsites.net");
    IMobileServiceTable trekkaTable = client.GetTable("Trekka");

    var item = await trekkaTable.LookupAsync(id);

    JObject jo = new JObject();
    jo.Add("id", id);
    jo.Add("stopedAt",DateTime.UtcNow);
    var inserted = await trekkaTable.UpdateAsync(jo);

    log.Info(inserted.ToString());

    return req.CreateResponse(HttpStatusCode.OK);
}