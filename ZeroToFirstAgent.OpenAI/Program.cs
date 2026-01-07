/*
 * 1: Create an 'OpenAI API Account
 * 2: Add Nuget Packages (OpenAi + Microsoft.Agents.AI.OpenAI)
 * 3: Create an OpenAIClient
 * 4 : Get a ChatClient and create an AI Agent from it
 * 5 : Call RunAsync or RunStreamingAsync on the Agent.
 */



using Microsoft.Agents.AI;
using OpenAI.Chat;

const string apiCode = "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";
const string modelName = "gpt-5-nano";

OpenAI.OpenAIClient openAIClient = new OpenAI.OpenAIClient(apiCode);
var agent = openAIClient.GetChatClient(modelName).CreateAIAgent();
AgentRunResponse response = await agent.RunAsync("What is the capital of Germany?");
Console.WriteLine(response);
await foreach (var item in agent.RunStreamingAsync("How to make a salad?"))
{
    Console.Write(item);
}