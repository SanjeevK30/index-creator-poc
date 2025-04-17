using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using Azure;
using Microsoft.Extensions.Logging;
using IndexCreator.Core.Models;

namespace IndexCreator.Core.Services
{
    /// <summary>
    /// Azure Search Index Service implementation
    /// </summary>
    public class AzureSearchIndexService : ISearchIndexService
    {
        private readonly string _endpoint;
        private readonly string _key;
        private readonly ILogger _logger;

        public AzureSearchIndexService(string endpoint, string key, ILogger logger)
        {
            _endpoint = endpoint;
            _key = key;
            _logger = logger;
        }

        public async Task CreateIndexAsync(string indexName)
        {
            _logger.LogInformation($"Creating search index '{indexName}'");

            // Create search index client
            var endpoint = new Uri(_endpoint);
            var credential = new AzureKeyCredential(_key);
            var adminClient = new SearchIndexClient(endpoint, credential);

            // Define fields for the index
            var fields = new List<SearchField>
        {
            new SearchField("id", SearchFieldDataType.String)
            {
                IsKey = true,
                IsFilterable = true
            },
            new SearchField("content", SearchFieldDataType.String)
            {
                IsSearchable = true
            },
            new SearchField("contentVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
            {
                IsSearchable = true,
                VectorSearchDimensions = 1536,
                VectorSearchProfileName = "default-profile"
            },
            new SearchField("meta_json_string", SearchFieldDataType.String)
            {
                IsFilterable = true
            },
            new SearchField("filepath", SearchFieldDataType.String)
            {
                IsFilterable = true
            },
            new SearchField("title", SearchFieldDataType.String)
            {
                IsSortable = true,
                IsSearchable = true,
            },
        };

            // Define vector search configuration
            var vectorSearch = new VectorSearch
            {
                Profiles =
            {
                new VectorSearchProfile("default-profile", "default-algorithm")
            },
                Algorithms =
            {
               new HnswAlgorithmConfiguration ("default-algorithm")
            }
            };

            // Define semantic search configuration
            var semanticSearch = new SemanticSearch
            {
                Configurations =
            {
                new SemanticConfiguration("azureml-default", new()
                {
                    TitleField =  new SemanticField("title"),
                    ContentFields = { new SemanticField("content") },
                    KeywordsFields = { }
                })
            }
            };

            // Create the index
            var definition = new SearchIndex(indexName, fields)
            {
                VectorSearch = vectorSearch,
                SemanticSearch = semanticSearch
            };

            // Delete index if it exists
            try
            {
                await adminClient.DeleteIndexAsync(indexName);
                _logger.LogInformation($"Deleted existing index '{indexName}'");
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Index doesn't exist, continue
            }

            // Create new index
            await adminClient.CreateIndexAsync(definition);
            _logger.LogInformation($"Created new index '{indexName}'");
        }

        public async Task IngestDocumentsAsync(string indexName, List<ProcessedDocument> documents)
        {
            _logger.LogInformation($"Ingesting {documents.Count} documents into search index '{indexName}'");

            // Create search client
            var endpoint = new Uri(_endpoint);
            var credential = new AzureKeyCredential(_key);
            var searchClient = new SearchClient(endpoint, indexName, credential);

            // Upload documents in batches
            const int batchSize = 100;
            for (int i = 0; i < documents.Count; i += batchSize)
            {
                var batch = documents.Skip(i).Take(batchSize).ToArray();
                var indexDocuments = new IndexDocumentsBatch<ProcessedDocument>();

                // Use Add method to populate the Actions collection instead of assigning it directly
                foreach (var doc in batch)
                {
                    indexDocuments.Actions.Add(IndexDocumentsAction.Upload(doc));
                }

                _logger.LogInformation($"Uploading batch of {batch.Length} documents");
                var result = await searchClient.IndexDocumentsAsync(indexDocuments);

                if (result.Value.Results.Any(r => !r.Succeeded))
                {
                    _logger.LogWarning("Some documents failed to index");
                    foreach (var failed in result.Value.Results.Where(r => !r.Succeeded))
                    {
                        _logger.LogWarning($"Document {failed.Key} failed: {failed.ErrorMessage}");
                    }
                }
            }

            _logger.LogInformation("Document ingestion completed");
        }
    }   
}
