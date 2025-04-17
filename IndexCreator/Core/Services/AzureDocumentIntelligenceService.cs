using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using Microsoft.Extensions.Logging;
using IndexCreator.Core;
using IndexCreator.Core.Models;

namespace IndexCreator.Core.Services
{
    /// <summary>
    /// Azure Document Intelligence Service implementation
    /// </summary>
    public class AzureDocumentIntelligenceService : IDocumentProcessingService
    {
        private readonly string _endpoint;
        private readonly string _key;
        private readonly ILogger _logger;
        private readonly IDocumentChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;

        public AzureDocumentIntelligenceService(
            string endpoint,
            string key,
            ILogger logger,
            IDocumentChunkingService chunkingService,
            IEmbeddingService embeddingService)
        {
            _endpoint = endpoint;
            _key = key;
            _logger = logger;
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
        }

        public async Task<List<ProcessedDocument>> ProcessDocumentsAsync(List<string> documentUrls)
        {
            _logger.LogInformation("Processing documents with Document Intelligence");

            // Create Document Intelligence client
            var credential = new AzureKeyCredential(_key);
            var client = new DocumentAnalysisClient(new Uri(_endpoint), credential);

            // Process each document and collect results
            var processedDocuments = new List<ProcessedDocument>();

            foreach (var documentUrl in documentUrls)
            {
                _logger.LogInformation($"Processing document at {documentUrl}");

                // Start document analysis
                var operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-layout", new Uri(documentUrl));
                var result = operation.Value;

                // Extract document title (use filename as fallback)
                string documentTitle = Path.GetFileNameWithoutExtension(new Uri(documentUrl).LocalPath);

                // Try to extract a better title from the document if possible
                documentTitle = ExtractDocumentTitle(result, documentTitle);

                // Extract content from document
                var pageTexts = ExtractPageTexts(result);

                // Generate chunks from document text
                var documentChunks = _chunkingService.ChunkContent(pageTexts, 2000, 200); // 2000 char chunks with 200 char overlap

                // Process each chunk
                foreach (var (chunkText, chunkNumber) in documentChunks)
                {
                    // Get embeddings for the chunk
                    var embedding = await _embeddingService.GetEmbeddingAsync(chunkText);

                    // Create chunk title
                    string chunkTitle = documentTitle;
                    if (documentChunks.Count > 1)
                    {
                        chunkTitle = $"{documentTitle} - Part {chunkNumber}";
                    }

                    // Create metadata
                    var metadata = new Dictionary<string, string>
                    {
                        ["source"] = Path.GetFileName(new Uri(documentUrl).LocalPath),
                        ["chunk_number"] = chunkNumber.ToString(),
                        ["document_url"] = documentUrl,
                        ["document_title"] = documentTitle,
                        ["chunk_title"] = chunkTitle
                    };

                    // Add processed document
                    processedDocuments.Add(new ProcessedDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = chunkTitle,
                        Content = chunkText,
                        ContentVector = embedding,
                        Meta_json_string = JsonSerializer.Serialize(metadata),
                        Filepath = Path.GetFileName(new Uri(documentUrl).LocalPath)
                    });
                }
            }

            return processedDocuments;
        }

        private string ExtractDocumentTitle(AnalyzeResult result, string defaultTitle)
        {
            // Try to extract a better title from the document if possible
            if (result.Pages.Any())
            {
                // Simply use the first line of the document as the title
                // This is a simplification, but works reasonably well for most documents
                var firstPage = result.Pages[0];
                if (firstPage.Lines.Any())
                {
                    var firstLine = firstPage.Lines[0].Content.Trim();

                    // Only use the first line as title if it's not too long and looks like a title
                    if (firstLine.Length > 0 && firstLine.Length < 100 && !firstLine.EndsWith("."))
                    {
                        return firstLine;
                    }
                    // If the first line doesn't look like a title, check the first few lines
                    else
                    {
                        foreach (var line in firstPage.Lines.Take(5))
                        {
                            var content = line.Content.Trim();
                            if (content.Length > 0 && content.Length < 100 &&
                                !content.EndsWith(".") &&
                                !char.IsDigit(content[0])) // Not likely to start with a number
                            {
                                return content;
                            }
                        }
                    }
                }
            }

            return defaultTitle;
        }

        private List<string> ExtractPageTexts(AnalyzeResult result)
        {
            var pageTexts = new List<string>();
            foreach (var page in result.Pages)
            {
                var pageText = new StringBuilder();
                foreach (var line in page.Lines)
                {
                    pageText.AppendLine(line.Content);
                }
                pageTexts.Add(pageText.ToString());
            }
            return pageTexts;
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
