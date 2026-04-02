#pragma warning disable OPENAI001
// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Containers;
using OpenAI.Responses;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");
Secrets secrets = SecretManager.GetSecrets();
OpenAIClient client = new(secrets.OpenAiApiKey);
//NB: I was unable to get this to work with Azure OpenAI in regard to downloading files from Code Interpreter
AIAgent agent = client
    .GetResponsesClient("gpt-4.1").AsAIAgent(tools: [new HostedCodeInterpreterTool()]);

string question = "Find Top 10 Countries in the world and make a Bar chart should each countries population in millions";
AgentResponse response = await agent.RunAsync(question);

foreach (ChatMessage message in response.Messages)
{
    foreach (AIContent content in message.Contents)
    {
        foreach (AIAnnotation annotation in content.Annotations ?? [])
        {
            if (annotation.RawRepresentation is ContainerFileCitationMessageAnnotation containerFileCitation)
            {
                ContainerClient containerClient = client.GetContainerClient();
                ClientResult<BinaryData> fileContent = await containerClient.DownloadContainerFileAsync(containerFileCitation.ContainerId, containerFileCitation.FileId);
                string path = Path.Combine(Path.GetTempPath(), containerFileCitation.Filename);
                await File.WriteAllBytesAsync(path, fileContent.Value.ToArray());
                await Task.Factory.StartNew(() =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                });
            }
        }
    }
}

Console.Write(response);

#pragma warning restore OPENAI001
