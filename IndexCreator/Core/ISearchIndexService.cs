using System.Collections.Generic;
using System.Threading.Tasks;
using IndexCreator.Core.Models;

namespace IndexCreator.Core
{
    /// <summary>
    /// Interface for search index operations
    /// </summary>
    public interface ISearchIndexService
    {
        Task CreateIndexAsync(string indexName);
        Task IngestDocumentsAsync(string indexName, List<ProcessedDocument> documents);
    }    
}
