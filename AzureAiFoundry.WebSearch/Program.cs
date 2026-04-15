// See https://aka.ms/new-console-template for more information
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Shared;

Console.WriteLine("Hello, World!");

Secrets secrets = SecretManager.GetSecrets();
PersistentAgentsClient client = new(secrets.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

BingGroundingSearchConfiguration binToolConfiguration = new BingGroundingSearchConfiguration(secrets.BingApiKey);
BingGroundingSearchToolParameters bingToolParameters =  new BingGroundingSearchToolParameters([binToolConfiguration]);

Response<PersistentAgent>? aiFoundryAgent = null;

try
{
    aiFoundryAgent = await client.Administration.CreateAgentAsync(
                                "gpt-4o",
                                "CodeGraphAgent",
                                "",
                                "You are a Graph-expert on US States",
                                new List<ToolDefinition>
                                {
                                        new BingGroundingToolDefinition(bingToolParameters)
                                });
    Response<PersistentAgent> agent = await client.Administration.GetAgentAsync(aiFoundryAgent.Value.Id);

    Response<ThreadRun> runResponse = await client.CreateThreadAndRunAsync(agent.Value.Id, new ThreadAndRunOptions
    {
        ThreadOptions = new PersistentAgentThreadCreationOptions
        {
            Messages =
            {
                new ThreadMessageOptions(MessageRole.User, "Whatis today's news in Space Exploration (List today's date and I)")
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
        foreach(var content in message.ContentItems)
        {
            if(content is MessageTextContent textContent)
            {
                Console.WriteLine($"Message from {message.Role}: {textContent.Text}");
            }
        }
    }


}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}finally
{
    if (aiFoundryAgent != null)
    {
        await client.Administration.DeleteAgentAsync(aiFoundryAgent.Value.Id);
    }
}