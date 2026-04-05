// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Responses;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");
Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
AIAgent agent = client.GetResponsesClient("gpt-5")
                        .AsAIAgent();
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
Utils.WriteLineGreen("SimpleQuestion-Begin");
AgentResponse response1 = await agent.RunAsync("What is the capital of France?");
Console.WriteLine(response1);
Utils.WriteLineGreen("SimpleQuestion-End");
Console.Clear();

Utils.WriteLineGreen("BigQuestion-Begin");
AgentResponse response2 = await agent.RunAsync("Write a 1000 word essay on Pigs in Space");
Console.WriteLine(response2);
Utils.WriteLineGreen("BigQuestion-END");

Console.Clear();


Utils.WriteLineGreen("BigQuestion-BACKGROUND-BEGIN;");
AgentSession agentSession =await agent.CreateSessionAsync();
ChatClientAgentRunOptions options = new ChatClientAgentRunOptions
{
    AllowBackgroundResponses = true
};
AgentResponse response3 = await agent.RunAsync("Write a 2000 word essay on Pigs in space", agentSession, options);
Utils.WriteLineGreen("BigQuestion-BACKGROUND-END");

int counter = 0;
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
while (response3.ContinuationToken is not null)
{
    await Task.Delay(TimeSpan.FromSeconds(2));
    counter++;
    Utils.WriteLineDarkGray($"-- Waited:{(counter * 2)} seconds");

    options.ContinuationToken = response3.ContinuationToken;
    response3 = await agent.RunAsync(agentSession, options);
}
#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

Console.WriteLine(response3.Text);