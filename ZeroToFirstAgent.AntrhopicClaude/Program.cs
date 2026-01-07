/*
 * 1: Get an Anthropic Claude API Key (https://docs.claude.com/en/api/admin-api/apikeys/get-api-key)
 * 2: Add Nuget Packages( Anthropic.SDK + Microsoft.Agents.AI) 
 * 3: Create an AnthropicClient for an ChatClientAgent
 * 4: Call RunAsync or RunStreamAsync on the Agent
 */

using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

const string apiKey = "";
const string model = AnthropicModels.Claude35Sonnet;
IChatClient client = new AnthropicClient(new APIAuthentication(apiKey)).Messages.AsBuilder().Build();
ChatClientAgentRunOptions chatClientAgentRunOptions = new ChatClientAgentRunOptions(new Microsoft.Extensions.AI.ChatOptions()
{
    ModelId = model,
    MaxOutputTokens = 1024,
});

AIAgent agent = new ChatClientAgent(client);
AgentRunResponse result = await agent.RunAsync("What is the capital of France?",options: chatClientAgentRunOptions );
Console.WriteLine(result);

Console.WriteLine("----");
await foreach(AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?", options: chatClientAgentRunOptions))
{
       Console.Write(update);
}