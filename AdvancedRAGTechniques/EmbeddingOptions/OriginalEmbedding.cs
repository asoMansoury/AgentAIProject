using JetBrains.Annotations;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using System;
using System.Collections.Generic;
using System.Text;
using UsingRagInAgentFramework;

namespace AdvancedRAGTechniques.EmbeddingOptions
{
    public static class OriginalEmbedding
    {
        public static async Task Embed(SqlServerCollection<Guid,MovieVectorStoreRecord> collection, Movie[] movieDataForRage)
        {
            await collection.EnsureCollectionDeletedAsync();
            await collection.EnsureCollectionExistsAsync();

            int counter = 0;
            foreach(Movie movie in movieDataForRage)
            {
                counter++;
                Console.Write($"\rEmbedding Movies:{counter}/{movieDataForRage.Length}");
                await collection.UpsertAsync(new MovieVectorStoreRecord
                {
                    Id = Guid.NewGuid(),
                    Title = movie.Title,
                    Plot = movie.Plot,
                    Rating = movie.Rating
                });
            }

            Console.WriteLine();
            Console.WriteLine("\rEmbedding complete... Let's as the question again using RAG");
        }
    }
}
