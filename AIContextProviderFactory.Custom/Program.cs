// See https://aka.ms/new-console-template for more information
using AgentFrameworkToolkit;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");

string userId = "rwj1234";

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

ChatClientAgent memoryExtractorAgent = client
    .GetChatClient("gpt-4.1-nano")
    .AsAIAgent(
        instructions: "Look at the user's message and extract any memory that we do not already know (or non if there aren't any memories to store)"
    );

ChatClientAgent agentWithCustomMemory = client.GetChatClient("gpt-4.1").AsIChatClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            Instructions = "You are a nice AI"
        },
        AIContextProviders = [new CustomContextProvider(memoryExtractorAgent, userId)]
    });

AIAgent agentToUse = agentWithCustomMemory;

AgentSession thread =await agentToUse.CreateSessionAsync();

while (true)
{
    Console.Write("> ");

    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        Microsoft.Extensions.AI.ChatMessage message = new(ChatRole.User, input);
        AgentResponse response = await agentToUse.RunAsync(message, session: thread);
        {
            Console.WriteLine($"AI: {response}");
        }
    }
}