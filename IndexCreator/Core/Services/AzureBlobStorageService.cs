using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using IndexCreator.Core;

namespace IndexCreator.Core.Services
{
    // Implementations

    /// <summary>
    /// Azure Blob Storage Service implementation
    /// </summary>
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public AzureBlobStorageService(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<List<string>> UploadFilesToStorageAsync(string containerName, string[] filePaths)
        {
            _logger.LogInformation($"Uploading {filePaths.Length} files to Azure Blob Storage");

            // Create blob service client and container
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Create container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Upload each file and collect URLs
            var blobUrls = new List<string>();

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                var blobClient = containerClient.GetBlobClient(fileName);

                _logger.LogInformation($"Uploading {fileName}...");

                using (var fileStream = File.OpenRead(filePath))
                {
                    await blobClient.UploadAsync(fileStream, overwrite: true);
                }

                blobUrls.Add(blobClient.Uri.ToString());
                _logger.LogInformation($"Uploaded {fileName} to {blobClient.Uri}");
            }

            return blobUrls;
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
