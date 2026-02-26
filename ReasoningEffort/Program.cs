using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey), new AzureOpenAIClientOptions
{
    NetworkTimeout = TimeSpan.FromSeconds(30) // Set the network timeout to 30 seconds
});


//Let's try and use reasoning to answer a question that requires multiple steps to arrive at the final answer
AIAgent agentDefault = client.GetChatClient("gpt-5-mini")
                        .AsAIAgent(
                            instructions: "You are a helpful assistant that tries to answer the user's question to the best of your abilities."
                        )
                        .AsBuilder()
                        .Build();

AgentResponse response1 = await agentDefault.RunAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "What is the Capital of France and how many people are living in this country?"));
Console.WriteLine("Response without reasoning effort:");
Console.WriteLine(response1.ToString());
Utils.Separator();

ChatOptions options = new ChatOptions();

//Now let's see if we can get a better answer by asking the model to use reasoning to arrive at the final answer
AIAgent agentWithReasoningEffort = client.GetChatClient("gpt-5-mini")
                        .AsAIAgent(
                            instructions: "You are a helpful assistant that provides accurate and structured answers."
                        )
                        
                        .AsBuilder()
                        
                        .Build();

