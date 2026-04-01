// See https://aka.ms/new-console-template for more information
using AdvancedRAGTechniques;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using Shared;
using System.ClientModel;
using System.Text.Json;
using UsingRagInAgentFramework;

Console.WriteLine("Hello, World!");

String jsonWithMovies = await File.ReadAllTextAsync("made_up_movies.json");
Movie[] movieDataForRag = JsonSerializer.Deserialize<Movie[]>(jsonWithMovies)!;

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));


IEmbeddingGenerator<string,Embedding<float>> embeddingGenerator = client
    .GetEmbeddingClient("text-embedding-3-small")
    .AsIEmbeddingGenerator();

//InMemoryVectorStore vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions
//{
//    EmbeddingGenerator = embeddingGenerator
//});

SqlServerVectorStore vectorStore = new SqlServerVectorStore("dbConnectionString",new SqlServerVectorStoreOptions
{
    EmbeddingGenerator = embeddingGenerator
});

SqlServerCollection<Guid, MovieVectorStoreRecord> collection = vectorStore.GetCollection<Guid, MovieVectorStoreRecord>("movies");

bool importData = false;
if(!await collection.CollectionExistsAsync())
{
    importData = true;
}
else
{
    Console.WriteLine("Re-import data? y/n"); 
    ConsoleKeyInfo key = Console.ReadKey();
    if(key.KeyChar == 'y' || key.KeyChar == 'Y')
    {
        importData = true;
    }
}

ChatMessage question  = new(ChatRole.User, "Whatis the 3 highest rated adventure movies(list their titles,plots and ratings)?");

await Option1RephraseQuestion.Run(importData, movieDataForRag, question, client, collection, secrets);
await Option2EnhanceEmbeddings.Run(importData, movieDataForRag, question, client, collection, secrets);
await Option3CommonSense.Run(importData, movieDataForRag, question, client, collection,secrets);