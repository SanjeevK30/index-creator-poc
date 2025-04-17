using System.Collections.Generic;
using System.Threading.Tasks;
using IndexCreator.Core.Models;

namespace IndexCreator.Core
{
    /// <summary>
    /// Interface for Document Intelligence operations
    /// </summary>
    public interface IDocumentProcessingService
    {
        Task<List<ProcessedDocument>> ProcessDocumentsAsync(List<string> documentUrls);
    }   
}
