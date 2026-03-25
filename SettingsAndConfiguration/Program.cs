// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Shared;
using System;
using System.ClientModel;

Console.WriteLine("Hello, World!");

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

AIAgent noSettingAgent = client.GetChatClient(secrets.ChatDeploymentName)
                                .AsAIAgent(name: "NoSettingAgent", instructions: "You are an agent with no settings. Answer questions to the best of your ability.")
                                .AsBuilder()
                                .Build();

AIAgent aiAgent = client.GetChatClient(secrets.ChatDeploymentName)
                                .AsAIAgent(name: "NoSettingAgent", 
                                            instructions: "You are a cool surfer dude",
                                            tools: []
                                            )
                                .AsBuilder()
                                .Build();

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton(new MySpecialService());
ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();


// OpenTelemetry
string sourceName = Guid.NewGuid().ToString("N");
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(sourceName)
    .AddConsoleExporter()
    .Build();

AIAgent agentWithAllSettings = client.GetChatClient("gpt-4.1").AsAIAgent
    (
        //Optional system instructions that define the agent's behavior and personality.
        instructions: "Speak like a Pirate",

        //***************************************************************************************************************************

        //Optional name for the agent for identification purposes.
        name: "My Agent",

        //***************************************************************************************************************************

        //Optional description of the agent's capabilities and purpose.
        description: "Description that is not used by the AI, but some of the online Agent Framework have a description",

        //***************************************************************************************************************************

        //Optional collection of AI tools that the agent can use during conversations.
        tools: [], //Will be covered in a separate video

        //***************************************************************************************************************************

        //Provides a way to customize the creation of the underlying IChatClient used by the agent.
        clientFactory: chatClient =>
        {
            return new ConfigureOptionsChatClient(chatClient, options =>
            {
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                options.RawRepresentationFactory = _ => new ChatCompletionOptions
                {
                    ReasoningEffortLevel = ChatReasoningEffortLevel.High,
                };
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            });
        },

        //***************************************************************************************************************************

        //Optional logger factory for enabling logging within the agent.
        loggerFactory: LoggerFactory.Create(loggingBuilder => { loggingBuilder.AddConsole(); }),

        //***************************************************************************************************************************

        //An optional IServiceProvider to use for resolving services required by the AIFunction instances being invoked.
        services: serviceProvider
    )
    .AsBuilder()
    .UseOpenTelemetry(sourceName) //Middleware
    .Build();


AgentResponse response = await agentWithAllSettings.RunAsync("What is the capital of France?");
Console.WriteLine(response);




#region Even more options via ChatClientAgentOptions

ChatClientAgent advancedAgent = client.GetChatClient("gpt-4.1").AsAIAgent(
    new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            Instructions = "Speak like a Pirate"
        },
        AIContextProviders = [], //Option to intercept before and after LLM call
        ChatHistoryProvider = null, //Option to inject a store for session conversations,
        Name = "My Agent",
        Description = "Description that is not used by the AI, but some of the online Agent Framework have a description",
        Id = "1234",
        UseProvidedChatClientAsIs = false
    },
    clientFactory: chatClient =>
    {
        return new ConfigureOptionsChatClient(chatClient, options =>
        {
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            options.RawRepresentationFactory = _ => new ChatCompletionOptions
            {
                ReasoningEffortLevel = ChatReasoningEffortLevel.Low,
            };
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        });
    },
    loggerFactory: LoggerFactory.Create(loggingBuilder => { loggingBuilder.AddConsole(); }),
    services: serviceProvider
);

#endregion
public class MySpecialService
{

}