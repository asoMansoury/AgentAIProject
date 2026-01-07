/* Steps: 
 * 1: Download Olama + Model(https://ollama.com/)
 * 2: Add Nuget Packages(OllamaSharp + Microsoft.Agents.AI)
 * Create An OllamaApiClient and store it as IChatClient for an ChatClientAgent
 * 4: Call RunAsync or RunStreamAsync on the Agent
 */

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

IChatClient client = new OllamaApiClient("http://localhost:11434", "llama3.2:1b");
AIAgent agent = new ChatClientAgent(client);
AgentRunResponse response = await agent.RunAsync("Write a poem about AI in C#.");
Console.WriteLine(response);
await foreach(AgentRunResponseUpdate update in agent.RunStreamingAsync("Write a poem about AI in C#.")) 
{
    Console.Write(update);
}
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
