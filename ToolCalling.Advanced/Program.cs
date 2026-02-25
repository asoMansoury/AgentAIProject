// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Reflection;
using System.Text;
using ToolCalling.Advanced.Tools;


Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));


Console.WriteLine("Hello, World!");
MethodInfo[] methods = typeof(FileSystemTools).GetMethods(BindingFlags.Instance | BindingFlags.Public);
List<AITool> listOfTools = methods.Select(x => AIFunctionFactory.Create(x, new FileSystemTools())).Cast<AITool>().ToList();

//Approval Tools
#pragma warning disable MEAI001
listOfTools.Add(new ApprovalRequiredAIFunction(AIFunctionFactory.Create(DangerousTools.SomethingDangerous)));
#pragma warning restore MEAI001

AIAgent agent = client
                .GetChatClient(secrets.ChatDeploymentName)
                .AsAIAgent(
                            instructions: "You are a File Expert. When working with files you need to provide the full path; not just the filename",
                            tools: listOfTools
                            )
                .AsBuilder()
                .Use(FunctionCallMiddleware)
                .Build();

AgentThread thread =await agent.GetNewThreadAsync();

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    Microsoft.Extensions.AI.ChatMessage message = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, input);
    AgentResponse response = await agent.RunAsync(message, thread);
    #pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    List<UserInputRequestContent> userInputRequests = response.UserInputRequests.ToList();
#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    while (userInputRequests.Count > 0)
    {
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        List<Microsoft.Extensions.AI.ChatMessage> userInputResponses = userInputRequests
                                                .OfType<FunctionApprovalRequestContent>()
                                                .Select(approvalRequest =>
                                                {
                                                    Console.WriteLine($"The agent would like to invoke the following function, please reply Y to approve: Name {approvalRequest.FunctionCall}");
                                                    return new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, [approvalRequest.CreateResponse(Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)]);
                                                }).ToList();
#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        response = await agent.RunAsync(userInputResponses, thread);
        userInputRequests = response.UserInputRequests.ToList();
    }

    Console.WriteLine(response);

    Utils.Separator();
}

async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent,FunctionInvocationContext context,Func<FunctionInvocationContext,CancellationToken,ValueTask<object>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");

    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());


    return await next(context, cancellationToken);
}



