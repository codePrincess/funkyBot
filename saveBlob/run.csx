#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
#r "System.Drawing"
 
using System.Net;
using System.IO; 
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Text;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, Stream saveBlob, Binder binder, TraceWriter log)
{
    string name = req.GetQueryNameValuePairs()
                     .FirstOrDefault(q => string.Compare(q.Key, "imgName", true) == 0)
                     .Value;
         
    string filename = name;
    
    if (filename.Contains('.')) {
        filename = name.Substring(0, name.IndexOf('.'));
    }

    var attributes = new Attribute[]
    {
        new BlobAttribute($"uploads/{filename}/{name}"),
        new StorageAccountAttribute("AzureWebJobsStorage")
    };

    var writer = await binder.BindAsync<CloudBlockBlob>(attributes).ConfigureAwait(false);
    var bytes = req.Content.ReadAsByteArrayAsync().Result;
    await writer.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
    writer.Properties.ContentType = "image/png";
    writer.SetProperties();

    var response = new HttpResponseMessage()
    {
        StatusCode = HttpStatusCode.OK
    };

    string baseURL = "https://mytrekkafunca18d.blob.core.windows.net";
    string originalBaseURL = $"{baseURL}/uploads";
    string returnValue = $"{originalBaseURL}/{filename}/{name}";

    response.Content = new StringContent(
        content: returnValue,
        encoding: Encoding.UTF8,
        mediaType: "application/json");
    
    return response;
}