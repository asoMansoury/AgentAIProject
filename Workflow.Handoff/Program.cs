// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Console.WriteLine("Hello, World!");

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));



ChatClient chatClientMini = client.GetChatClient("gpt-4-mini");
ChatClient chatClient = client.GetChatClient("gpt-4");
Console.Write("> ");

AIAgent intentAgent = chatClientMini.AsAIAgent(name: "IntentAgent", instructions: "Detemine what type of question was asked. Never answer yourself");

AIAgent musicNerdAgent = chatClient.AsAIAgent(name: "MusicNerd", instructions: "You are a Music Nerd(Give a question on max 200 chars)");
AIAgent movieNerdAgent = chatClient.AsAIAgent(name: "MovieNerd", instructions: "You are a Movie Nerd(Give a question on max 200 chars)");


while (true)
{
    List<Microsoft.Extensions.AI.ChatMessage> messages = new();
    Workflow workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(intentAgent)
        .WithHandoffs(intentAgent, [movieNerdAgent,musicNerdAgent])
        .WithHandoffs([movieNerdAgent, musicNerdAgent],intentAgent)
        .Build();
    Console.Write("> ");
    messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, Console.ReadLine() ?? string.Empty));

}



static async Task<List<ChatMessage>> RunWorkflowAsync(Workflow workflow, List<ChatMessage> messages)
{
    string? lastExecutorId = null;

    StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages);
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
    await foreach (WorkflowEvent @event in run.WatchStreamAsync())
    {
        switch (@event)
        {
            case AgentResponseUpdateEvent e:
                {
                    if (e.ExecutorId != lastExecutorId)
                    {
                        lastExecutorId = e.ExecutorId;
                        Console.WriteLine();
                        Utils.WriteLineGreen(e.Update.AuthorName ?? e.ExecutorId);
                    }

                    Console.Write(e.Update.Text);
                    if (e.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is FunctionCallContent call)
                    {
                        Console.WriteLine();
                        Utils.WriteLineYellow($"Call '{call.Name}' with arguments: {JsonSerializer.Serialize(call.Arguments)}]");
                    }

                    break;
                }
            case WorkflowOutputEvent output:
                Utils.Separator();
                return output.As<List<ChatMessage>>()!;
            case ExecutorFailedEvent failedEvent:
                if (failedEvent.Data is Exception ex)
                {
                    Utils.WriteLineRed($"Error in agent {failedEvent.ExecutorId}: " + ex);
                }

                break;
        }
    }

    return [];
}