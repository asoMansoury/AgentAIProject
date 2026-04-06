// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.ClientModel.Primitives;

Console.Clear();
using var handler = new CustomClientHttpHandler();
using var httpClient = new HttpClient(handler);

Secrets secrets = SecretManager.GetSecrets();

/*
OpenAIClient client = new(new ApiKeyCredential(secrets.OpenAiApiKey), new OpenAIClientOptions
{
    Transport = new HttpClientPipelineTransport(httpClient)
});
*/

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey), new AzureOpenAIClientOptions
{
    Transport = new HttpClientPipelineTransport(httpClient)
});

AIAgent agent = client.GetChatClient(secrets.ChatDeploymentName)
                        .AsAIAgent(instructions:"You are a Raw Agent");

AgentResponse response = await agent.RunAsync("What is the weather in Seattle?");
Utils.WriteLineGreen("The Answer is:"); 
