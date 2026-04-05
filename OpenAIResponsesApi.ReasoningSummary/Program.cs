// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using Shared;

Console.WriteLine("Hello, World!");
Secrets secrets = SecretManager.GetSecrets();
OpenAIClient client = new(secrets.OpenAiApiKey);

AzureOpenAIClient azureClient = new(new Uri(secrets.AzureOpenAiEndpoint),new System.ClientModel.ApiKeyCredential(secrets.AzureOpenAiKey));

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
ChatClientAgent agent = client
    .GetResponsesClient(model: "gpt-5-mini")
    .AsAIAgent(options: new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            RawRepresentationFactory = static _ => new CreateResponseOptions //<--- Notice this is different from out ChatCompletionOptions
            {
                ReasoningOptions = new ResponseReasoningOptions
                {
                    ReasoningEffortLevel = ResponseReasoningEffortLevel.Medium,
                    ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed
                }
            }
        }
    });
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

AgentResponse response = await agent.RunAsync("What is the Capital of France?");

foreach (ChatMessage item in response.Messages)
{
    foreach (var content  in item.Contents)
    {
        if(content is TextReasoningContent textReasoningContent)
        {
            Utils.WriteLineGreen("The Reasoning");
            Utils.WriteLineDarkGray(textReasoningContent.Text);
        }
    }
}