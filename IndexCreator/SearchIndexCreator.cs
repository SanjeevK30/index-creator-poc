using System.Threading.Tasks;
using IndexCreator.Core;
using Microsoft.Extensions.Logging;

namespace IndexCreator
{
    /// <summary>
    /// Main coordinator class that creates search indices from PDF files
    /// </summary>
    public class SearchIndexCreator : ISearchIndexCreator
    {
        private readonly ILogger _logger;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDocumentProcessingService _documentProcessingService;
        private readonly ISearchIndexService _searchIndexService;

        public SearchIndexCreator(
            ILogger logger,
            IBlobStorageService blobStorageService,
            IDocumentProcessingService documentProcessingService,
            ISearchIndexService searchIndexService)
        {
            _logger = logger;
            _blobStorageService = blobStorageService;
            _documentProcessingService = documentProcessingService;
            _searchIndexService = searchIndexService;
        }

        public async Task CreateSearchIndexForPdfsAsync(string indexName, string[] pdfFilePaths)
        {
            _logger.LogInformation($"Creating search index '{indexName}' for {pdfFilePaths.Length} PDF files");

            // Step 1: Upload PDFs to Azure Blob Storage
            var blobContainerName = "pdfdocuments";
            var uploadedBlobUrls = await _blobStorageService.UploadFilesToStorageAsync(blobContainerName, pdfFilePaths);

            // Step 2: Process PDFs with Document Intelligence
            var processedDocuments = await _documentProcessingService.ProcessDocumentsAsync(uploadedBlobUrls);

            // Step 3: Create the search index with vector search capabilities
            await _searchIndexService.CreateIndexAsync(indexName);

            // Step 4: Ingest processed documents into the search index
            await _searchIndexService.IngestDocumentsAsync(indexName, processedDocuments);

            _logger.LogInformation($"Successfully created and populated index '{indexName}' with {processedDocuments.Count} documents");
        }
    }    
}
