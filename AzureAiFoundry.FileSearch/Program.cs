// See https://aka.ms/new-console-template for more information
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Shared;

Console.WriteLine("Hello, World!");

Secrets secrets = SecretManager.GetSecrets();
PersistentAgentsClient client = new(secrets.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

Response<PersistentAgent>? aiFoundryAgent = null;
string? vectoreStoreI = null;

try
{
    string fileName = "secretData.pdf";
    Response<PersistentAgentFileInfo> file = await client.Files.UploadFileAsync(Path.Combine("Data", fileName), PersistentAgentFilePurpose.Agents);
    Response<PersistentAgentsVectorStore> vectoreStore = await client.VectorStores.CreateVectorStoreAsync(name: "MyVectoreStore");
    await client.VectorStores.CreateVectorStoreFileAsync(vectoreStore.Value.Id,file.Value.Id);

    aiFoundryAgent = await client.Administration.CreateAgentAsync(
        secrets.ChatDeploymentName,
        "FileAgent",
        "",
        "You are a File-expert. ALWAYS  use tools to answer all questions (do not use you world-knowledge)",
        toolResources:new ToolResources
        {
            FileSearch = new FileSearchToolResource
            {
                VectorStoreIds = { vectoreStore.Value.Id }
            }
        },
        tools: new List<ToolDefinition>
        {
            new FileSearchToolDefinition
            {
                FileSearch = new FileSearchToolDefinitionDetails
                {
                    MaxNumResults = 10
                }
            }
        });

    Response<PersistentAgent> agent = await client.Administration.GetAgentAsync(aiFoundryAgent.Value.Id);
    Response<ThreadRun> runResponse = await client.CreateThreadAndRunAsync(agent.Value.Id, new ThreadAndRunOptions
    {
        ThreadOptions = new PersistentAgentThreadCreationOptions
        {
            Messages =
            {
                new ThreadMessageOptions(MessageRole.User, "What is word of the day")
            }
        }
    });
    ThreadRun run = runResponse.Value;


    // Wait for completion
    while (run.Status != RunStatus.Completed && run.Status != RunStatus.Failed)
    {
        await Task.Delay(1000);
        run = (await client.Runs.GetRunAsync(run.ThreadId, run.Id)).Value;
    }

    await foreach (var message in client.Messages.GetMessagesAsync(run.ThreadId))
    {
        foreach (var content in message.ContentItems)
        {
            if (content is MessageTextContent textContent)
            {
                Console.WriteLine($"Message from {message.Role}: {textContent.Text}");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally {

    if (vectoreStoreI != null)
    {
        await client.VectorStores.DeleteVectorStoreAsync(vectoreStoreI);
    }

    if (aiFoundryAgent != null)
    {
        await client.Administration.DeleteAgentAsync(aiFoundryAgent.Value.Id);
    }
}
