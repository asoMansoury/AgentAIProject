// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Threading;
using UsingRagInAgentFramework;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Console.WriteLine("Hello, World!");

String jsonWithMovies = await File.ReadAllTextAsync("made_up_movies.json");
Movie[] movieDataForRag = JsonSerializer.Deserialize<Movie[]>(jsonWithMovies)!;

Microsoft.Extensions.AI.ChatMessage question = new(ChatRole.User, "What is the 3 highest reated adventure movies(list their titles,plots and ratings)");

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

ChatClientAgent agent = (ChatClientAgent)client.GetChatClient(secrets.ChatDeploymentName)
    .AsAIAgent(
        instructions: "You are an expert a set of made up movies given to you(aka don't consider movies from your world-knowlede"
    )
    .AsBuilder()
    .Build();

#region Let's give the model all data upfront
Utils.WriteLineYellow("Sample 1");

List<Microsoft.Extensions.AI.ChatMessage> preloadEverythingChatMessages = [
        new(ChatRole.Assistant,"Here are all the movies")
    ];
foreach (Movie movie in movieDataForRag)
{
    preloadEverythingChatMessages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, movie.GetTitleAndDetails()));
}
preloadEverythingChatMessages.Add(question);
AgentResponse response1 = await agent.RunAsync(preloadEverythingChatMessages);
Console.WriteLine(response1);
#endregion

Console.Clear();

Utils.WriteLineYellow("Sample 2");

IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = client.GetEmbeddingClient("text-embedding-3-small")
                                                                         .AsIEmbeddingGenerator();

InMemoryVectorStore vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions
{
    EmbeddingGenerator = embeddingGenerator
});

InMemoryCollection<Guid, MovieVectorStoreRecord> collection = vectorStore.GetCollection<Guid, MovieVectorStoreRecord>("movies");
await collection.EnsureCollectionExistsAsync();

int counter = 0;
foreach (Movie movie in movieDataForRag)
{
    counter++;
    Console.Write($"\rEmbedding Movies : {counter}/{movieDataForRag.Length}");
    await collection.UpsertAsync(new MovieVectorStoreRecord
    {
        Id = Guid.NewGuid(),
        Title = movie.Title,
        Plot = movie.Plot,
        Rating = movie.Rating
    });
}

Console.WriteLine();
Console.WriteLine("\rEmbedding complete...Let's as the question again using RAG");

List<ChatMessage> ragPreloadChatMessages =
    [
    new(ChatRole.Assistant,"Here are the most relevant movies")
    ];

await foreach (VectorSearchResult<MovieVectorStoreRecord> searchResult in collection.SearchAsync(question.Text, 10,
        new VectorSearchOptions<MovieVectorStoreRecord>
        {
            IncludeVectors = false
        }
    ))
{
    MovieVectorStoreRecord record = searchResult.Record;
    ragPreloadChatMessages.Add(new ChatMessage(ChatRole.Assistant, record.GetTitleAndDetails()));
} ;
ragPreloadChatMessages.Add(question);

AgentResponse response2 = await agent.RunAsync(ragPreloadChatMessages);
Console.WriteLine(response2);


#region Lets do the same as above but as function calling[Smart if the use example just say 'Hello' we do not preload any movies
Utils.WriteLineYellow("Sample 3");

SearchTool searchTool = new(collection);
AIAgent agentWithTools = client.GetChatClient(secrets.ChatDeploymentName)
                                .AsAIAgent(
                                            instructions: "You are an expert a set of made up movies given to you(aka don't consider movies from your world-knwoledge",
                                            tools: [AIFunctionFactory.Create(searchTool.SearchVectorStore)]
                                            )
                                .AsBuilder()
                                .Use(FunctionCallMiddleware)
                                .Build();

AgentResponse response3 = await agentWithTools.RunAsync(question);
Console.WriteLine(response3);


#endregion



async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
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
class SearchTool(InMemoryCollection<Guid, MovieVectorStoreRecord> collection)
{
    public async Task<List<string>> SearchVectorStore(string question)
    {
        List<string> result = [];
        await foreach (VectorSearchResult<MovieVectorStoreRecord> searchResult in collection.SearchAsync(question, 10,
                           new VectorSearchOptions<MovieVectorStoreRecord>
                           {
                               IncludeVectors = false
                           }))
        {
            MovieVectorStoreRecord record = searchResult.Record;
            result.Add(record.GetTitleAndDetails());
        }

        return result;
    }
}