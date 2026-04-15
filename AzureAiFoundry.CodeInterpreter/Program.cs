using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();
PersistentAgentsClient client = new(secrets.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

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
            new CodeInterpreterToolDefinition()
        });

    Response<PersistentAgent> agent = await client.Administration.GetAgentAsync(aiFoundryAgent.Value.Id);

    var threadOptions = new PersistentAgentThreadCreationOptions();
    threadOptions.Messages.Add(new ThreadMessageOptions(MessageRole.User, "Make a jpg image with graph listing population of the top 10 US States in year 2000"));

    var options = new ThreadAndRunOptions
    {
        ThreadOptions = threadOptions
    };

    Response<ThreadRun> runResponse = await client.CreateThreadAndRunAsync(agent.Value.Id, options);
    ThreadRun run = runResponse.Value;

    // Wait for completion
    while (run.Status != RunStatus.Completed && run.Status != RunStatus.Failed)
    {
        await Task.Delay(1000);
        run = (await client.Runs.GetRunAsync(run.ThreadId, run.Id)).Value;
    }


    string? fileId = null;
    string? fileName = null;
    string? filePath = null;
    string? textToReplace = null;

    if (run.Status == RunStatus.Completed)
    {
        var messages = client.Messages.GetMessages(run.ThreadId);
        foreach(var message in messages)
        {
            foreach(var content in message.ContentItems)
            {
                if(content is MessageTextContent textContent)
                {
                    textToReplace = textContent.Text;
                }

                if(content is MessageImageFileContent imageContent)
                {
                    fileId = imageContent.FileId;
                    fileName = "chart.jpg";
                    filePath = Path.Combine(Environment.CurrentDirectory, fileName);

                    // Download the file
                    var fileStream = await client.Files.GetFileContentAsync(fileId);
                    using FileStream fs = File.Create(filePath);
                    await fileStream.Value.ToStream().CopyToAsync(fs);
                }
            }
        }

        if (fileId != null)
        {
            Response<BinaryData> fileContent = await client.Files.GetFileContentAsync(fileId);
            filePath = Path.Combine(Path.GetTempPath(), fileName!);
            await File.WriteAllBytesAsync(filePath, fileContent.Value.ToArray());
            await Task.Factory.StartNew(() =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            });
        }

    //    Console.WriteLine(
    //textToReplace != null && assistantText != null
    //    ? assistantText.Replace(textToReplace, filePath ?? "")
    //    : assistantText);
    }
    else
    {
        Console.WriteLine("Run failed");
    }

    // Cleanup
    await client.Threads.DeleteThreadAsync(run.ThreadId);
}
finally
{

    if (aiFoundryAgent != null)
    {
        await client.Administration.DeleteAgentAsync(aiFoundryAgent.Value.Id);
    }
}