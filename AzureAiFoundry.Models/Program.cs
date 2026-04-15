// See https://aka.ms/new-console-template for more information
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AzureAI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");
Secrets secrets = SecretManager.GetSecrets();
PersistentAgentsClient client = new(secrets.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

string questionToAsk = "What is the capital of France?";
await CreateAndCallNormalClientAgent("gpt-4o", questionToAsk);
await CreateAndCallNormalClientAgent("DeepSeek-R1-0528", questionToAsk);


await CreateAndCallFoundryAgent("gpt-4o", questionToAsk);
await CreateAndCallFoundryAgent("DeepSeek-R1-0528", questionToAsk);
async Task CreateAndCallNormalClientAgent(string model, string question)
{
    AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

    ChatClientAgent agent = client.GetChatClient(model)
        .AsAIAgent(instructions: "You are a helpful assistant that answers questions.");

    var response = await agent.RunAsync(question);
    Utils.WriteLineYellow($"Answer from ChatClient using Model: '{model}'");
    Console.WriteLine($"Answer = '{response}");

}

async Task CreateAndCallFoundryAgent(string model, string question)
{
    PersistentAgentsClient client = new(secrets.AzureAiFoundryAgentEndpoint, new AzureCliCredential());
    string? agentIdToDelete = null;

    try
    {
        Response<PersistentAgent> aiFoundryAgent = await client.Administration.CreateAgentAsync(
            model: model,
            instructions: "You are a helpful assistant that answers questions."
        );
        agentIdToDelete = aiFoundryAgent.Value.Id;

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

        Utils.WriteLineYellow($"Answer from Foundry Agent using Model: '{model}'");
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
    finally
    {
        if (agentIdToDelete is not null)
        {
            await client.DeleteAgentAsync(agentIdToDelete);
        }
    }
    }