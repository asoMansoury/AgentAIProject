// See https://aka.ms/new-console-template for more information
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AzureAI;
using OpenAI.Responses;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");

Secrets secrets = SecretManager.GetSecrets();
AIProjectClient client= new AIProjectClient(new Uri(secrets.AzureAiFoundryAgentEndpoint), new AzureCliCredential());

string modelDeploymentName = "gpt-4.1-mini";
string myAgentName = "myAgent";
string myInstructions = "You are a nice AI";

try
{
    await client.Deployments.GetDeploymentAsync(modelDeploymentName);
}
catch(ClientResultException ex)
{
    if(ex.Status == 404)
    {
        Console.WriteLine("The specified resource was not found. Please check the model deployment name and try again.");
        return;
    }
    else
    {
        throw;
    }
}


//Step1 : Create/update an agent if it does not exist
try
{
    ClientResult<AgentRecord> clientResult= await client.Agents.GetAgentAsync(agentName: myAgentName);
}
catch(ClientResultException ex)
{
    if(ex.Status == 404)
    {
        Console.WriteLine("The specified resource was not found. Please check the agent name and try again.");
        await CreateAgent(myInstructions);
    }
    else
    {
        throw;
    }
}

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
FoundryAgent agentByName = client.AsAIAgent(myAgentName);
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
AgentResponse response = await agentByName.RunAsync("Hi there");

response = await agentByName.RunAsync("What options do the AddCardAsync method in 'TrelloDotNet' (use tools)");
Console.WriteLine(response);

response = await agentByName.RunAsync("What is 23434343*3434343/2323232 (use tools to calculate)");
Console.WriteLine(response);

response = await agentByName.RunAsync("What is the biggest news story today?");
Console.WriteLine(response);

//Let's make a V2 with new instructions
await CreateAgent("Speak like a pirate");

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
FoundryAgent agentV2 = client.AsAIAgent(myAgentName);
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
response =await agentV2.RunAsync("Hi there");
Console.WriteLine(response);

AgentVersion agentV1 = (await client.Agents.GetAgentVersionAsync(myAgentName, "1")).Value;
var agentByVersion = client.AsAIAgent(agentV1);
response = await agentByVersion.RunAsync("Hi Agent 1");
Console.WriteLine(response);
return;


async Task CreateAgent(string instruction)
{
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    await client.Agents.CreateAgentVersionAsync(
        agentName: myAgentName,
        options:new AgentVersionCreationOptions(
            new PromptAgentDefinition(modelDeploymentName)
            {
                Tools =
                {
                    new CodeInterpreterTool(new CodeInterpreterToolContainer(new AutomaticCodeInterpreterToolContainerConfiguration())),
                    new WebSearchTool(),
                    //MCP Tools can be defined by can't be properly consumed by MS Agent Framework :-(
                    new McpTool("TrelloDotNetToolAssistant", new Uri("https://trelodotnetassis.com"))
                },
                Instructions = instruction,

                //NB: Reasoning Effort is buggy at the moment in the portal:-(
                ReasoningOptions = new ResponseReasoningOptions
                {
                    ReasoningEffortLevel = ResponseReasoningEffortLevel.Low
                }
            }
        ));
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
} 
