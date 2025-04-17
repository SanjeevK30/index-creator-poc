using System;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IndexCreator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Load configuration from appsettings.json or environment variables  
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables() 
                .Build();

            // Set up logging  
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                // Create a search index creator via factory
                var searchIndexCreator = SearchIndexCreatorFactory.Create(configuration, logger);

                // Paths to PDF files
                string[] pdfPaths = {
                        "Documents/CBP-9993.pdf",
                        "Documents/OOTLAR_pdf3.pdf",
                        "Documents/Introduction-to-the-UK-Tax-system-Lecture.pdf"
                    };
                // Use the search index creator
                var indexName = "gray-store-wnllbl32dq";                
                await searchIndexCreator.CreateSearchIndexForPdfsAsync(indexName, pdfPaths);

                logger.LogInformation("Index creation completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the prompt flow.");
            }
        }
    }
}