// See https://aka.ms/new-console-template for more information
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntimeGenAI;

Console.WriteLine("Hello, World!");
string? folderPath = Console.ReadLine();

if(folderPath !=null && Directory.Exists(folderPath))
{
    OnnxRuntimeGenAIChatClient client = new OnnxRuntimeGenAIChatClient(folderPath);
    ChatClientAgent agent = client.AsAIAgent();

    AgentResponse response = await agent.RunAsync("What is the Capital of Bulgaria?");
    Console.WriteLine(response);
}
else
{
    Console.WriteLine("Invalid folder path.");
}