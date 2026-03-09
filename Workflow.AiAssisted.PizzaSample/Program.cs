// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Workflow.AiAssisted.PizzaSample;
using Workflow.AiAssisted.PizzaSample.Executors;
using Workflow.AiAssisted.PizzaSample.Models;

Console.WriteLine("Hello, World!");
JsonSerializerOptions jsonSerializerOptions = new()
{
    PropertyNameCaseInsensitive = true,
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    Converters = { new JsonStringEnumConverter() }
};


Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

AgentFactory agentFactory = new AgentFactory(secrets.ChatDeploymentName, secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);

PizzaOrderParseExecutor orderParser = new PizzaOrderParseExecutor(agentFactory.CreateLorderTakerAgent());
PizzaStockCheckerExecutor stockChecker = new();
PizzaSuccessExecutor endSuccess = new PizzaSuccessExecutor();
PizzaWarningExecutor endWarning = new PizzaWarningExecutor(agentFactory.CreateWarningToCustomerAgent());

WorkflowBuilder builder = new WorkflowBuilder(orderParser);

builder.AddEdge(
    source: orderParser,
    target: stockChecker
    );

builder.AddSwitch(
    source: stockChecker,
    swithBuilder =>
        {
            swithBuilder.AddCase<PizzaOrder>(x => x!.Warnings.Count == 0, endSuccess);
            swithBuilder.AddCase<PizzaOrder>(x => x!.Warnings.Count > 0, endWarning);
        }
    );

var workflow = builder.Build();

Console.OutputEncoding = System.Text.Encoding.UTF8;
const string input = "Make a big Pepperoni Pizza with mushrooms and onions";

StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, input);
await foreach(WorkflowEvent evt in run.WatchStreamAsync()) { 
    if(evt is ExecutorCompletedEvent executorComplete)
    {
        Utils.WriteLineGreen($"{executorComplete.ExecutorId} Completed");
    }
}