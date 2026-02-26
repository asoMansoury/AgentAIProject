// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ToolCalling.FromAnMcpServer.Models;

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

string question = "What are the top 10 Movies according to IMDB?";

//Without structured output, the model will return a string that we have to parse to get the information we need
AIAgent agent1 = client.GetChatClient(secrets.ChatDeploymentName)
                        .AsAIAgent(
                            instructions: "You are a helpful assistant.",
                            tools: new List<AITool>() // No tools for this agent
                        )
                        .AsBuilder()
                        .Build();

var response1 = await agent1.RunAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, question));
Console.WriteLine("Response without structured output:");
Console.WriteLine(response1.ToString());
Utils.Separator();


//With structured output, we can define a schema for the model's response, making it easier to extract the information we need
//ChatClientAgent agent2 = client.GetChatClient(secrets.ChatDeploymentName)
//                        .CreateAIAgent(instructions: "You are an expert in IMDB lists");

//AgentResponse response2 = await agent2.RunAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, question));
//MovieResult? structuredOutput = response2.Deserialize<MovieResult>();
//DisplayMovies(structuredOutput!)    ;
void DisplayMovies(MovieResult movieResult)
{
    int counter = 1;
    Console.WriteLine(movieResult.MessageBack);

    foreach (var movie in movieResult.Top10Movies)
    {
        Console.WriteLine($"{counter}. {movie.Title} ({movie.YearOfRelease}) - Genre: {movie.Genre} - Rating: {movie.ImdbScore}");
        counter++;
    }
}

//More combersome but sometimes needed way
JsonSerializerOptions jsonSerializerOptions = new()
{
    PropertyNameCaseInsensitive = true,
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    Converters = { new JsonStringEnumConverter() }
};

AIAgent agent3 = client.GetChatClient(secrets.ChatDeploymentName)
                        .AsAIAgent(
                            instructions: "You are an expert in IMDB lists",
                            tools: new List<AITool>() // No tools for this agent
                        )
                        .AsBuilder()
                        .Build();
AgentResponse response3 = await agent3.RunAsync(question, options: new ChatClientAgentRunOptions()
{
    ChatOptions = new ChatOptions
    {
        ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema<MovieResult>(jsonSerializerOptions)
    }
});

MovieResult? structuredOutput2 = response3.Deserialize<MovieResult>(jsonSerializerOptions);
DisplayMovies(structuredOutput2!);