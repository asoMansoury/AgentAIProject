// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using ConversationThreads;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

Console.WriteLine("Hello, World!");

AzureOpenAIClient client = new AzureOpenAIClient(new Uri(""), new System.ClientModel.ApiKeyCredential(""));
AIAgent agent = client.GetChatClient("").CreateAIAgent("You are a Friendly AI Bot, answering questions");

AgentThread thread = agent.GetNewThread();
const bool optionToResume = false; //Set this to true to test resume of previous conversations
if (optionToResume)
{
    thread = await AgentThreadPersistence.ResumeChatIfRequestAsync(agent);
}

while (true)
{
    Console.Write("> ");

    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        Microsoft.Extensions.AI.ChatMessage message = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, input);
        await foreach(AgentRunResponseUpdate update in agent.RunStreamingAsync(message, thread))
        {
            Console.Write(update);
        }
    }

    Console.WriteLine();
    Console.WriteLine(string.Empty.PadLeft(50,'*'));
    Console.WriteLine();

    if (optionToResume)
    {
        await AgentThreadPersistence.StoreThreadAsync(thread);
    }
};
