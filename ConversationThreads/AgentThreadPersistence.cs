using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace ConversationThreads
{
    public static class AgentThreadPersistence
    {
        public record StoredMessage(string Role, string Text);
        private static string ConversationPath =>
            Path.Combine(Path.GetTempPath(), "conversation.json");

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        // -----------------------------
        // Resume or create new thread
        // -----------------------------
        public static async Task<AgentThread> ResumeChatIfRequestAsync(
            AIAgent agent,
            List<StoredMessage> messageHistory)
        {
            if (File.Exists(ConversationPath))
            {
                Console.Write("Restore previous conversation? (Y/N): ");
                ConsoleKeyInfo key = Console.ReadKey();
                Console.Clear();

                if (key.Key == ConsoleKey.Y)
                {
                    string json = await File.ReadAllTextAsync(ConversationPath);
                    PersistedConversation persisted =
                        JsonSerializer.Deserialize<PersistedConversation>(json, JsonOptions)!;

                    // Restore agent thread
                    AgentThread resumedThread =
                        agent.DeserializeThread(persisted.Thread);

                    // Restore console from stored messages
                    RestoreConsole(persisted.Messages, messageHistory);

                    return resumedThread;
                }
            }

            return agent.GetNewThread();
        }

        // -----------------------------
        // Console replay (SUPPORTED)
        // -----------------------------
        private static void RestoreConsole(
            IEnumerable<StoredMessage> messages,
            List<StoredMessage> messageHistory)
        {
            foreach (StoredMessage message in messages)
            {
                messageHistory.Add(message);

                if (message.Role == "user")
                {
                    Console.WriteLine($"> {message.Text}");
                }
                else
                {
                    Console.WriteLine(message.Text);
                    Console.WriteLine();
                    Console.WriteLine(new string('*', 50));
                    Console.WriteLine();
                }
            }
        }

        // -----------------------------
        // Persist thread + messages
        // -----------------------------
        public static async Task StoreThreadAsync(
            AgentThread thread,
            List<StoredMessage> messageHistory)
        {
            PersistedConversation persisted = new()
            {
                Thread = thread.Serialize(),
                Messages = messageHistory
            };

            string json = JsonSerializer.Serialize(persisted, JsonOptions);
            await File.WriteAllTextAsync(ConversationPath, json);
        }

        // -----------------------------
        // Storage DTO
        // -----------------------------
        private sealed class PersistedConversation
        {
            public JsonElement Thread { get; set; }
            public List<StoredMessage> Messages { get; set; } = new();
        }
    }
}
