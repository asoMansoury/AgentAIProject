// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");
Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
ChatClient chatClient = client.GetChatClient(secrets.ChatDeploymentName);

#pragma warning disable MEAI001
// Creates a reducer that keeps only the most recent messages,
// trimming chat history to a maximum of 4 messages.
// Useful for limiting token usage and keeping the context window small.
IChatReducer chatReducer = new MessageCountingChatReducer(targetCount: 4);

// Creates a reducer that summarizes older messages when the chat history
// reaches 4 or more messages, then replaces them with a compact summary.
// Keeps only 1 summarized context item, helping preserve important context
// while reducing the total number of messages sent to the model.
IChatReducer chatReducer2 = new SummarizingChatReducer(
    chatClient.AsIChatClient(),
    targetCount: 1,
    threshold: 4
);
#pragma warning restore MEAI001

ChatClientAgent agent = client
                .GetChatClient(secrets.ChatDeploymentName)
                .AsAIAgent(
                        new ChatClientAgentOptions
                        {
                            ChatOptions = new()
                            {
                                Instructions = "You are a Friendly AI Bot, answering questions in a friendly manner.",
                            },
                            ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions
                            {
                                ChatReducer = chatReducer,
                            })
                        }
                    );

AgentSession session = await agent.CreateSessionAsync();

while (true)
{
    Console.Write("> ");
    string input = Console.ReadLine() ?? string.Empty;
    AgentResponse response = await agent.RunAsync(input, session);
    Console.WriteLine(response);

    InMemoryChatHistoryProvider? provider = agent.GetService<InMemoryChatHistoryProvider>();
    List<Microsoft.Extensions.AI.ChatMessage> messagesInSession = provider?.GetMessages(session) ?? [];
    Utils.WriteLineDarkGray("- Number of messages in session: " + messagesInSession.Count());
    foreach(Microsoft.Extensions.AI.ChatMessage message in messagesInSession)
    {
        Utils.WriteLineDarkGray($"--{message.Role}: {message.Text}");
    }
    Utils.Separator();
}