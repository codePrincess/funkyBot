public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log, out object output)
{
    dynamic data = req.Content.ReadAsAsync<object>().Result;

    log.Info(req.Content.ReadAsStringAsync().Result);

    string displayText = Convert.ToString(data?.DisplayText);
    displayText = $"{displayText} vom {DateTime.UtcNow:g}";
    object licenseNumber = data?.LicenseNumber;
    decimal km = Convert.ToDecimal(data?.Km);

    log.Info(displayText + ", " + licenseNumber + ", " + km);

    if (displayText == null || licenseNumber == null || km == null)
    {
        throw new Exception("Please pass a DisplayText, LicenceNumber and Km in the request body");
    }

    output = new
        {
            displayText,
            licenseNumber,
            km,
        };

    return req.CreateResponse();
}
