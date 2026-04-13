// See https://aka.ms/new-console-template for more information
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Shared;

Console.WriteLine("Hello, World!");
Secrets secrets = SecretManager.GetSecrets();
PersistentAgentsClient client = new(secrets.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

PersistentAgentsFiles files = client.Files;

var vectorStores = client.VectorStores; //Vectore Store CRUD + Add/Remove Files from Vector Store

Threads threads = client.Threads; // Thread CRUD

ThreadMessages messages = client.Messages;//Message CRUD (associated with Threads)  


ThreadRuns threadRuns = client.Runs; //Thread Run CRUD (associated with Threads) + Add/Remove Messages from Thread Run

PersistentAgentsAdministrationClient administration = client.Administration; //The Agents CRUD

//Agents Details
CancellationToken cancellationToken = new CancellationTokenSource().Token;

Response<PersistentAgent> existingPersistentAgent = await administration.GetAgentAsync("my-agent", cancellationToken);
PersistentAgent persistentAgent = existingPersistentAgent.Value;
Console.WriteLine($"Loaded agent: {persistentAgent.Name}");

Response<PersistentAgent> newPersistentAgent = administration.CreateAgent(
    model: "gpt-4.1-mini",
    name: "my-agnet",
    instructions: "Instructions for my agent",
    toolResources: new ToolResources
    {
        AzureAISearch = new AzureAISearchToolResource(indexConnectionId: "my-search-connection-id", indexName: "my-search-index", topK: 1, filter: "", queryType: null),
        CodeInterpreter = new CodeInterpreterToolResource
        {
            DataSources = { },
            FileIds = { }
        },
        FileSearch = new FileSearchToolResource
        {
            VectorStoreIds = { },
            VectorStores = { }
        },
        Mcp =
        {
            new MCPToolResource(serverLabel:""),
            new MCPToolResource(serverLabel:"")
        }
    },
    tools:new List<ToolDefinition>
    {
        new CodeInterpreterToolDefinition()
    },
    temperature:1,//NB : Do not touch these, if you use a reason model
    topP:1, //NB: Do not touch these, if you use a reason model
    responseFormat:BinaryData.Empty,
    metadata:new Dictionary<string, string> { },
    cancellationToken: cancellationToken
    );

Response<PersistentAgent> agentResponse = await administration.GetAgentAsync(newPersistentAgent.Value.Id, cancellationToken);
PersistentAgent agent = agentResponse.Value;



//Example
//await DeleteAllThreads(client);//WARNING : This will delete all threads, use with caution
async Task DeleteAllThreads(PersistentAgentsClient persistentAgentsClient) 
{
    await foreach(var thread in persistentAgentsClient.Threads.GetThreadsAsync(100))
    {
        await persistentAgentsClient.Threads.DeleteThreadAsync(thread.Id, cancellationToken);
        Console.WriteLine($"Deleted thread with id: {thread.Id}");
    }
}
