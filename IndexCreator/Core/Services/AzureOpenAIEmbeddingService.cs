using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IndexCreator.Core;

namespace IndexCreator.Core.Services
{
    /// <summary>
    /// Azure OpenAI Embedding Service implementation
    /// </summary>
    public class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly string _endpoint;
        private readonly string _key;
        private readonly HttpClient _httpClient;

        public AzureOpenAIEmbeddingService(string endpoint, string key)
        {
            _endpoint = endpoint;
            _key = key;
            _httpClient = new HttpClient();
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            // Call Azure OpenAI to get embeddings
            var requestUri = $"{_endpoint}/openai/deployments/text-embedding-ada-002/embeddings?api-version=2023-07-01-preview";

            var requestBody = new
            {
                input = text,
                model = "text-embedding-ada-002"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", _key);

            var response = await _httpClient.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(jsonResponse);

            var embeddingData = result.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding");

            var embedding = new float[1536]; // Ada-002 produces 1536-dimensional embeddings
            int i = 0;
            foreach (var value in embeddingData.EnumerateArray())
            {
                embedding[i++] = value.GetSingle();
            }

            return embedding;
        }
    }

    ///// <summary>
    ///// Model class for processed documents
    ///// </summary>
    //public class ProcessedDocument
    //{
    //    public string Id { get; set; }
    //    public string Title { get; set; }
    //    public string Content { get; set; }
    //    public float[] ContentVector { get; set; }
    //    public string Meta_json_string { get; set; }
    //    public string Filepath { get; set; }
    //}

    // Example usage
    //public class Program
    //{
    //    public static async Task Main(string[] args)
    //    {
    //        // Setup logger and configuration
    //        var configuration = new ConfigurationBuilder()
    //            .AddJsonFile("appsettings.json")
    //            .Build();

    //        using var loggerFactory = LoggerFactory.Create(builder =>
    //        {
    //            builder
    //                .AddConsole()
    //                .AddDebug();
    //        });
    //        var logger = loggerFactory.CreateLogger<Program>();

    //        // Create a search index creator via factory
    //        var searchIndexCreator = SearchIndexCreatorFactory.Create(configuration, logger);

    //        // Use the search index creator
    //        string indexName = "my-pdf-index";
    //        string[] pdfFilePaths = new[] { "document1.pdf", "document2.pdf" };

    //        await searchIndexCreator.CreateSearchIndexForPdfsAsync(indexName, pdfFilePaths);
    //    }
    //}
}
