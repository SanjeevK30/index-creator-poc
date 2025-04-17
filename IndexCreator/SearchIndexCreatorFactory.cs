using IndexCreator.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IndexCreator
{
    /// <summary>
    /// Factory for creating a SearchIndexCreator instance with proper dependencies
    /// </summary>
    public class SearchIndexCreatorFactory
    {
        public static ISearchIndexCreator Create(IConfiguration configuration, ILogger logger)
        {
            // Get configuration values
            var azureOpenAIEndpoint = configuration["AzureOpenAI:Endpoint"];
            var azureOpenAIKey = configuration["AzureOpenAI:Key"];
            var azureSearchEndpoint = configuration["AzureSearch:Endpoint"];
            var azureSearchKey = configuration["AzureSearch:Key"];
            var azureStorageConnectionString = configuration["AzureStorage:ConnectionString"];
            var documentIntelligenceEndpoint = configuration["DocumentIntelligence:Endpoint"];
            var documentIntelligenceKey = configuration["DocumentIntelligence:Key"];

            // Create services
            var blobStorageService = new AzureBlobStorageService(azureStorageConnectionString, logger);
            var chunkingService = new DocumentChunkingService();
            var embeddingService = new AzureOpenAIEmbeddingService(azureOpenAIEndpoint, azureOpenAIKey);
            var searchIndexService = new AzureSearchIndexService(azureSearchEndpoint, azureSearchKey, logger);
            var documentProcessingService = new AzureDocumentIntelligenceService(
                documentIntelligenceEndpoint,
                documentIntelligenceKey,
                logger,
                chunkingService,
                embeddingService);

            // Create and return the main coordinator
            return new SearchIndexCreator(
                logger,
                blobStorageService,
                documentProcessingService,
                searchIndexService);
        }
    }    
}
