
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MultiAgent.ManualViaStructuredOutput;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Text;

Console.WriteLine("Hello, World!");


Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

ChatClient chatClient = client.GetChatClient("gpt-4");
AIAgent stringAgent = chatClient.AsAIAgent(name: "StringAgent", 
                                           instructions: "You are string manipulator.",
                                           tools: new []
                                           {
                                                AIFunctionFactory.Create(StringTools.Reverse),
                                                AIFunctionFactory.Create(StringTools.Uppercase),
                                                AIFunctionFactory.Create(StringTools.Lowercase)
                                           })
                                 .AsBuilder().Use(FunctionCallMiddleware).Build();

AIAgent numberAgent = client.GetChatClient("gpt-4").AsAIAgent(name: "NumberAgent",
                                           instructions: "You are number manipulator.",
                                           tools: new []
                                           {
                                                AIFunctionFactory.Create(NumberTools.RandomNumber),
                                                AIFunctionFactory.Create(NumberTools.AnswerToEverythingNumber),
                                           })
                                 .AsBuilder().Use(FunctionCallMiddleware).Build();
Utils.WriteLineYellow("Testing StringAgent with Reverse tool...");

AIAgent delegationAgent = client.GetChatClient(secrets.ChatDeploymentName)
                                .AsAIAgent(
                                    name: "DelegateAgent",
                                    instructions:"Are a Delegator of string and Number tasks. Never does such work yourself",
                                    tools: [
                                        stringAgent.AsAIFunction(new AIFunctionFactoryOptions{
                                            Name="StringAgentTool"
                                        }),
                                        numberAgent.AsAIFunction(new AIFunctionFactoryOptions{
                                            Name="NumberAgentAsTool"
                                        })
                                        ]
                                )
                                .AsBuilder()
                                .Use(FunctionCallMiddleware)
                                .Build();
AgentResponse responseFromDelegate = await delegationAgent.RunAsync("Uppercase 'Hello World");
Console.WriteLine(responseFromDelegate);
//responseFromDelegate.Usage.OutputAsInformation();
Utils.Separator();
Console.WriteLine("Jack of all trade agent");





async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}' [Agent: {callingAgent.Name}]");
    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
    }

    Utils.WriteLineYellow(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}

