// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Shared;

Console.WriteLine("Hello, World!");

Secrets secrets = SecretManager.GetSecrets();

//Setup Telemetry
string sourceName = "AiSource";
var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
    .AddSource(sourceName)
    .AddConsoleExporter();
if (!string.IsNullOrWhiteSpace(secrets.ApplicationInsightsConnectionString))
{
    tracerProviderBuilder.AddAzureMonitorTraceExporter(options => options.ConnectionString = secrets.ApplicationInsightsConnectionString);
}

using TracerProvider tracerProvider = tracerProviderBuilder.Build();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new System.ClientModel.ApiKeyCredential(secrets.AzureOpenAiKey));

AIAgent agent = client
    .GetChatClient(secrets.ChatDeploymentName)
    .AsAIAgent(
        name: "MyObservedAgent",
        instructions: "You are a Friendly AI Bot, answering questions"
    )
    .AsBuilder()
    .UseOpenTelemetry(sourceName,options=>
    {
        options.EnableSensitiveData = true;//If the actual messages should be logged or not
    })
    .Build();

AgentSession session = await agent.CreateSessionAsync();
AgentResponse response = await agent.RunAsync("Hello, My name is Rasmus", session);
Utils.WriteLineYellow(response.Text);
Utils.Separator();

AgentResponse response2 = await agent.RunAsync("What is the capital of France?", session);
Utils.WriteLineYellow(response2.Text);
Utils.Separator();

AgentResponse response3 = await agent.RunAsync("What was my name?", session);
Utils.WriteLineYellow(response3.Text);
Utils.Separator();