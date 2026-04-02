// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

AIAgent agent = client
    .GetChatClient("gpt-4.1")
    //.GetOpenAIResponseClient("gpt-4.1")
    .AsAIAgent(
            instructions:"You are a Space News AI Reporter",
            tools: [new HostedWebSearchTool()]
        );

List<AgentResponseUpdate> updated = [];
string question = "What is today's new in Space Exploration(List today's date at the top";
await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(question))
{
    updated.Add(update);
    Console.Clear();
    Console.WriteLine($"Question: {question}");
    Console.WriteLine("Answer:");
    foreach (AgentResponseUpdate u in updated)
    {
        Console.Write(u);
    }
}