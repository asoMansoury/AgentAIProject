// See https://aka.ms/new-console-template for more information
using Shared;
using System.Text.Json;

public class CustomClientHttpHandler() : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string requestString = await request.Content?.ReadAsStringAsync(cancellationToken)!;
        Utils.WriteLineGreen($"Raw Request ({request.RequestUri})");
        Utils.WriteLineDarkGray(MakePretty(requestString));
        Utils.Separator();
        var response = await base.SendAsync(request, cancellationToken);

        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        Utils.WriteLineGreen("Raw Response");
        Utils.WriteLineDarkGray(MakePretty(responseString));
        Utils.Separator();
        return response;
    }

    private string MakePretty(string input)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
        return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
    }
}