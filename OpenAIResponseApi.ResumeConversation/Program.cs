// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");
Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
AIAgent agent= client
                    .GetResponsesClient("gpt-4.1")
                    .AsAIAgent(
                        instructions: "You are a nice AI"
                    );
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

AgentSession session =await agent.CreateSessionAsync();
AgentResponse response1 = await agent.RunAsync("Who is Barak Obama? (Max 5 word)", session);
Console.WriteLine(response1.Text);

AgentResponse response2 = await agent.RunAsync("How Tall is he?", session);
Console.WriteLine(response2);
String? responseId= response2.ResponseId;

AgentResponse response3 = await agent.RunAsync("What city is he from", options: new ChatClientAgentRunOptions
{
    ChatOptions = new ChatOptions
    {
        ConversationId = responseId
    }
});
Console.WriteLine(response3);

