/*
 * 1: Create an Azure Ai Foundry' Resource(or legacy 'Azure OpenAI Resource')
 * 2 : Add Nuget Package 'Azure.AI.OpenAI' version * Microsoft Agents AI:openAI'1.4.0' or later
 * 3:  Create an AzureOpenAIClient (Api key or Azure Identity)
 * 4 : Get a ChatClient and Create an AI Agent from it.
 * 5 : Call RunAsync or RunStreamingAsync on the Agent.
 */


using Microsoft.Agents.AI;
using OpenAI.Chat;

const string endpoint = "https://asomansoury-resource.openai.azure.com/";
const string apiKey = "EJbrTO84Pw93ox7uJlWRkLaNkw6HlSID7ynu4MZyptfB7zEpLOTzJQQJ99CAACHYHv6XJ3w3AAAAACOGXRg9";
const string model = "gpt-5-mini";


Azure.AI.OpenAI.AzureOpenAIClient client = new Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint),new System.ClientModel.ApiKeyCredential(apiKey));
AIAgent agent =  client.GetChatClient(model).CreateAIAgent() ;
AgentRunResponse response = await agent.RunAsync("What is the capital of France?");
Console.WriteLine(response);

await foreach (var item in agent.RunStreamingAsync("How to make a soup?"))
{
    Console.Write(item);
}