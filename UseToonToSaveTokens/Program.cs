// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Text.Json;
using UseToonToSaveTokens;

Console.WriteLine("Hello, World!");
Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));


string json = await File.ReadAllTextAsync("famous_people.json");
List<FamousPerson> list = JsonSerializer.Deserialize<List<FamousPerson>>(json)!;

string instruction = " You answer questions about famous people. Always use tool 'get_famous_people' to get data";
string question = "Tell me about Hula Johnson";
ChatClientAgent agentWithJsonTool = client.GetChatClient(secrets.ChatDeploymentName)
    .AsAIAgent(
        instructions:instruction,
        tools: [AIFunctionFactory.Create(GetFamousPeopleAsJson, name:"get_famous_people")]
    );


ChatClientAgent agentWithToonTool = client.GetChatClient(secrets.ChatDeploymentName)
    .AsAIAgent(
        instructions: instruction,
        tools: [AIFunctionFactory.Create(GetFamousPeopleAsToon, name: "get_famous_people")]
    );

Utils.WriteLineDarkGray("Ask using JSON Tool");
AgentResponse response1 = await agentWithJsonTool.RunAsync(question);
Console.WriteLine(response1);

Utils.WriteLineDarkGray("Ask using JSON Tool");
AgentResponse response2 = await agentWithJsonTool.RunAsync(question);
Console.WriteLine(response2);

return;


List<FamousPerson> GetFamousPeopleAsJson()
{
    string json = JsonSerializer.Serialize(list); //This is what the data is converted when given to AI
    return list;
}

string GetFamousPeopleAsToon()
{
    string toon = ToonNetSerializer.ToonNet.Encode(list);
    List<FamousPerson>? decodedAgain = ToonNetSerializer.ToonNet.Decode<List<FamousPerson>>(toon);
    return toon;
}