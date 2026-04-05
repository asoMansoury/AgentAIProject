// See https://aka.ms/new-console-template for more information
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Files;
using OpenAI.Responses;
using OpenAI.VectorStores;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");
Secrets secrets = SecretManager.GetSecrets();
OpenAIClient client = new(secrets.OpenAiApiKey);


OpenAIFileClient fileClient = client.GetOpenAIFileClient();
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
VectorStoreClient vectorStoreClient = client.GetVectorStoreClient();
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

string? fileId = null;
string? vectoreStoreId = null;
try
{
    string filename = "secretData.pdf";
    byte[] fileBytes = await File.ReadAllBytesAsync(Path.Combine("Data", filename));
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    ClientResult<OpenAIFile> uploadedFile = await fileClient.UploadFileAsync(new BinaryData(fileBytes), filename, FileUploadPurpose.UserData);
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    fileId = uploadedFile.Value.Id;

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    ClientResult<VectorStore> vectoreStore = await vectorStoreClient.CreateVectorStoreAsync(options: new VectorStoreCreationOptions()
    {
        Name = "MyVectoreStore"
    });
    vectoreStoreId = vectoreStore.Value.Id;
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    await vectorStoreClient.AddFileToVectorStoreAsync(vectoreStore.Value.Id, uploadedFile.Value.Id);

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    AIAgent agent = client.GetResponsesClient("gpt-4.1")
                          .AsAIAgent(
                            instructions: "Only use tools. Never your world-knowlege",
                            tools: [
                                new HostedFileSearchTool{
                                    Inputs = [new HostedFileContent(uploadedFile.Value.Id),new HostedVectorStoreContent(vectoreStore.Value.Id)]
                                }
                                ]
                            )
                          .AsBuilder()
                          .Build();
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    AgentResponse response = await agent.RunAsync("What is word of the day?");
    Console.Write(response);


}
finally
{
    if (vectoreStoreId != null)
    {
        await vectorStoreClient.DeleteVectorStoreAsync(vectoreStoreId);
    }

    if (fileId != null)
    {
        await fileClient.DeleteFileAsync(fileId);
    }

}