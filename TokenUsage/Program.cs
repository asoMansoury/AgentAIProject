// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

Console.WriteLine("Hello, World!");


AzureOpenAIClient client = new AzureOpenAIClient(new Uri(""), new System.ClientModel.ApiKeyCredential(""));
AIAgent agent = client.GetChatClient("sdfsdf").CreateAIAgent("You are a Friendly AI Bot, answering questions");

string question = "What is the capital of France and how many people live there?";

AgentRunResponse response = await agent.RunAsync(question);
Console.WriteLine($"- Input Tokens : {response.Usage?.InputTokenCount}");
Console.WriteLine($"- Output Tokens : {response.Usage?.OutputTokenCount}");

List<AgentRunResponseUpdate> updates = new List<AgentRunResponseUpdate>();
await foreach(AgentRunResponseUpdate update in agent.RunStreamingAsync(question))
{
    updates.Add(update);
    Console.Write(update);
}