// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using NAudio.Wave;
using OpenAI.Audio;
using Shared;
using System.ClientModel;

Console.WriteLine("Hello, World!");

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));


AudioClient audioClient = client.GetAudioClient("tts");
GeneratedSpeechVoice voice = new GeneratedSpeechVoice("echo");
string text = "Hi, Welcome to this Video about openAI's client. I'm an AI Speaking the words Rasmus entered in his program.";
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
ClientResult<BinaryData> result = audioClient.GenerateSpeech(text, voice, new SpeechGenerationOptions
{
    Instructions = "Speak like a pirate",
    ResponseFormat = new GeneratedSpeechFormat("mp3"),
    SpeedRatio = 1
});
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

byte[] bytes = result.Value.ToArray();

//Save To Disk
File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "test.mp3"), bytes);

//Play directly (NAudio nuget package (Windows Only))
WaveStream waveStream = new Mp3FileReader(new MemoryStream(bytes));
IWavePlayer player = new WaveOutEvent();
player.Init(waveStream);
player.Play();

Console.WriteLine("Playing audio. Press Enter to exit...");
Console.ReadLine();