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
AIAgent agent = client
                    .GetResponsesClient("gpt--codex")
                    .AsAIAgent(
                        instructions: "You are a C# Developer"
                    );
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

List<AgentResponseUpdate> updated = [];
string question = "Show me an C# Example of a method adding two numbers together.";
await foreach(AgentResponseUpdate response in agent.RunStreamingAsync(question))
{
    updated.Add(response);
    Console.WriteLine(response.Text);
}

AgentResponse fullResponse = updated.ToAgentResponse();
