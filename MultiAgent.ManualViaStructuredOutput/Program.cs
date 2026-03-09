// See https://aka.ms/new-console-template for more information
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

Console.WriteLine("Hello, World!");


Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));



ChatClient chatClientMini = client.GetChatClient("gpt-4-mini");
ChatClient chatClient = client.GetChatClient("gpt-4");
Console.Write("> ");

AIAgent intentAgent = chatClientMini.AsAIAgent(name: "IntentAgent",instructions:"Detemine what type of question was asked. Never answer yourself");
JsonSerializerOptions jsonSerializerOptions = new()
{
    PropertyNameCaseInsensitive = true,
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    Converters = { new JsonStringEnumConverter() }
};
string question = Console.ReadLine() ?? string.Empty;
AgentResponse initialResponse = await intentAgent.RunAsync(question);


// Fix: Use response.Text and System.Text.Json.JsonSerializer.Deserialize
IntentResult intentResult = System.Text.Json.JsonSerializer.Deserialize<IntentResult>(
    initialResponse.Text,
    new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new JsonStringEnumConverter() }
    }
);

switch (intentResult.Intent)
{
    case Intent.MusicQuestion:
        Utils.WriteLineRed("Music Question");
        AIAgent musicNerdAgent = chatClient.AsAIAgent(name: "MusicNerd",instructions:"You are a Music Nerd(Give a question on max 200 chars)");
        AgentResponse responseFromMusicNerd = await musicNerdAgent.RunAsync(question);
        Console.WriteLine(responseFromMusicNerd);
        break;
    case Intent.MovieQuestion:
        Utils.WriteLineYellow("Movie Questions");
        AIAgent movieNerdAgent = chatClient.AsAIAgent(name: "MovieNerd", instructions: "You are a Movie Nerd(Give a question on max 200 chars)");
        AgentResponse responseFromMovieNerd = await movieNerdAgent.RunAsync(question);

        Console.WriteLine("This is a movie question. Routing to movie agent...");
        break;
    case Intent.Other:
        Utils.WriteLineDarkGray("Other question;");
        AgentResponse otherResponse = await intentAgent.RunAsync(question);
        Console.WriteLine(otherResponse);
        Console.WriteLine("This is some other type of question. Routing to general agent...");
        break;
}


public class IntentResult
{
    [Description("What type of question is this?")]
    public required Intent Intent { get; set; }
}

public enum Intent
{
    MusicQuestion,
    MovieQuestion,
    Other
}