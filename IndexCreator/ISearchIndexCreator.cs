using System.Threading.Tasks;

namespace IndexCreator
{
    /// <summary>
    /// Interface for search index creation coordination
    /// </summary>
    public interface ISearchIndexCreator
    {
        Task CreateSearchIndexForPdfsAsync(string indexName, string[] pdfFilePaths);
    }    
}
