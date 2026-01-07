/* Steps: 
 * 1: Get a Google API Gemini API Key (https://aistudio.google.com/api-keys)
 * 2: Add Nuget Packages( Google_GenerativeAI.Microsoft + Microsoft.Agents.AI)
 * 3: Create an GenerativeChatClient for an ChatClientAgent
 * 4: Call RunAsync or RunStreamAsync on the Agent
 */

// See https://aka.ms/new-console-template for more information
using GenerativeAI;
using GenerativeAI.Microsoft;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

Console.WriteLine("Hello, World!");

const string apiKey = "<yourApiKey>";
const string model = GoogleAIModels.Gemini25Flash;

IChatClient client = new GenerativeAIChatClient(apiKey, model);
AIAgent agent = new ChatClientAgent(client);
AgentRunResponse result = await agent.RunAsync("What is the capital of Australia?");
Console.WriteLine(result);
Console.WriteLine("Press any key to exit...");

await foreach(AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make a soup"))
{
    Console.Write(update);
}