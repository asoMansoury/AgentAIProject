// See https://aka.ms/new-console-template for more information

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
OpenAIClient openAIClient = new(new ApiKeyCredential(secrets.OpenAiApiKey));

AIAgent azureOpenAIAgent = client.GetChatClient(secrets.ChatDeploymentName)
                        .AsAIAgent(
                            instructions: "You are a helpful assistant that tries to answer the user's question to the best of your abilities."
                        )
                        .AsBuilder()
                        .Build();

AIAgent openAiAgent = openAIClient.GetChatClient("gpt-4o")
                        .AsAIAgent(
                            instructions: "You are a helpful assistant that tries to answer the user's question to the best of your abilities."
                        )
                        .AsBuilder()
                        .Build();


Scenario scenario = Scenario.Text;

AgentResponse agentResponse;
switch (scenario)
{
    case Scenario.Text:
        {
            agentResponse = await azureOpenAIAgent.RunAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "What is the Capital of France and how many people are living in this country?"));
        }
        break;

    case Scenario.Image:
        {
            agentResponse = await azureOpenAIAgent.RunAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,
                [
                    new TextContent("What is this image"),
                    new UriContent("https://upload.wikimedia.org/wikipedia/commons/7/","image/jpeg")
                ]));
            ShowResponse(agentResponse);
            string path = Path.Combine("SamleData", "image.jpg");

            //Imagevia Base64
            string base64Pdf = Convert.ToBase64String(File.ReadAllBytes(path));
            string dataUri = $"data:image/jpeg;base64,{base64Pdf}";
            agentResponse = await azureOpenAIAgent.RunAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,

                [
                    new TextContent("What is in this image?"),
                    new DataContent(dataUri,"image/jpeg")
                ]));
            ShowResponse(agentResponse);
            //image via Memory
            ReadOnlyMemory<byte> data = File.ReadAllBytes(path).AsMemory();
            agentResponse = await azureOpenAIAgent.RunAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,
                [
                    new TextContent("What is in this image?"),
                    new DataContent(data,"image/jpeg")
                ]));
            ShowResponse(agentResponse);
            break;
        }
    case Scenario.Pdf:
        {
            string path = Path.Combine("SampleData", "catan_rules.pdf");

            //PDF as base64 
            string base64Pdf = Convert.ToBase64String(File.ReadAllBytes(path));
            string dataUri = $"data:application/pdf;base64,{base64Pdf}";
            agentResponse = await openAiAgent.RunAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,

                [
                    new TextContent("What is the winning condition in attached PDF"),
                    new DataContent(dataUri,"application/pdf")
                ]));
            ShowResponse(agentResponse);
            break;
        }
}


void ShowResponse(AgentResponse response)
{
    Console.WriteLine(response);
    
}
public enum Scenario
{
    Text,
    Image,
    Pdf
}
