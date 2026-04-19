//YouTube video that cover this sample: https://youtu.be/g72ks3rY9qQ

using A2A;
using A2A.AspNetCore;
using Anthropic.Core;
using Anthropic.Models.Messages;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ToolCalling.Advanced.Tools;
using AgentSkill = A2A.AgentSkill;

//Start with Business as Usual
Utils.WriteLineYellow("A2A Server");

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));


FileSystemTools target = new();
MethodInfo[] methods = typeof(FileSystemTools).GetMethods(BindingFlags.Public | BindingFlags.Instance);
List<AITool> listOfTools = methods.Select(x => AIFunctionFactory.Create(x, target)).Cast<AITool>().ToList();

AIAgent agent = client
    .GetChatClient("gpt-4.1-mini")
    .AsAIAgent(
        name: "FileAgent",
        instructions: "You are a File Expert. When working with files you need to provide the full path; not just the filename",
        tools: listOfTools
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

//A2A Part begin
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

AgentCard agentCard = new() //Aka the Agents Business Card
{
    Name = "FilesAgent",
    Description = "Handles requests relating to files",
    Version = "1.0.0",
    DefaultInputModes = ["text"],
    DefaultOutputModes = ["text"],
    Capabilities = new AgentCapabilities()
    {
        Streaming = false,
        PushNotifications = false,
    },
    Skills =
    [
        new AgentSkill()
        {
            Id = "my_files_agent",
            Name = "File Expert",
            Description = "Handles requests relating to files on hard disk",
            Tags = ["files", "folders"],
            Examples = ["What files are the in Folder 'Demo1'"],
        }
    ],
    Url = "http://localhost:5000"
};
TaskManager taskManager = new TaskManager();
taskManager.OnMessageReceived = async (messageSendParams, ct) =>
{
    string userText = messageSendParams.Message.Parts
        .OfType<TextPart>()
        .FirstOrDefault()?.Text ?? string.Empty;

    AgentResponse agentResponse = await agent.RunAsync(userText);

    // Fix: Set all required properties for Message
    return new Message
    {
        ID = Guid.NewGuid().ToString(),
        Content = new List<ContentBlock>
        {
            new ContentBlock(new TextBlock(agentResponse.ToString()))
        },
        Model = new ApiEnum<string, Model>(JsonSerializer.SerializeToElement("gpt-4.1-mini")),
        StopReason = null,
        StopSequence = null,
        Usage = new Usage
        {
            CacheCreation = null,
            CacheCreationInputTokens = null,
            CacheReadInputTokens = null,
            InputTokens = 0,
            OutputTokens = 0,
            ServerToolUse = null,
            ServiceTier = null
        },
        Role = JsonSerializer.SerializeToElement(MessageRole.Agent),

    };
};

// ✅ OnAgentCardQuery signature: Func<string, CancellationToken, Task<AgentCard>>
taskManager.OnAgentCardQuery = (agentUrl, ct) =>
{
    agentCard.Url = agentUrl; // inject runtime URL
    return Task.FromResult(agentCard);
};

// ✅ Correct overloads from the decompiled source you provided
app.MapA2A(taskManager, "/");
app.MapWellKnownAgentCard(taskManager, "/");

await app.RunAsync();
return;

async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}