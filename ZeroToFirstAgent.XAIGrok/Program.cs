
/*Steps : 
 * 1: Get a Grok Api Key(https://x.ai/api)
 * 2: Add Nuget Package (OpenAI + Microsoft.Agents.AI.OpenAI)
 * 3: Create an OpenAIClient
 * 3: Get a ChatClient and Create an AI Agent from it.
 * 4: Call RunAsync or RunStreamingAsync
 */
using Microsoft.Agents.AI;
using OpenAI.Chat;
using System.ClientModel;

const string apiKey = "";
const string model = "";

OpenAI.OpenAIClient client = new OpenAI.OpenAIClient(new ApiKeyCredential(apiKey), new OpenAI.OpenAIClientOptions
{
    Endpoint = new Uri("https://api.x.ai/")
});

AIAgent agent = client.GetChatClient(model).CreateAIAgent();

AgentRunResponse response = await agent.RunAsync("What is the Capital of Germany");
Console.WriteLine(response);
// Streaming Example
await foreach (var chunk in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(chunk);
};

