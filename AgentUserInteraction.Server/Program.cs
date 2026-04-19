// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");
Utils.WriteLineDarkGray("Initializing");
Utils.WriteLineDarkGray("- Waiting 1 sec for the server to be ready");
await Task.Delay(1000);
Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

ChatClientAgent agent = client
    .GetChatClient("gpt-4.1")
    .AsAIAgent(tools: [AIFunctionFactory.Create(GetWeather, name: "get_weather")]);


//AG-UI Part begin
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddAGUI();
WebApplication app = builder.Build();

app.MapAGUI("/", agent);
await app.RunAsync();


//Server-Tool
static string GetWeather(string city)
{
    return "It is sunny and 19 degrees";
}
