using System.Threading.Tasks;

namespace IndexCreator.Core
{
    /// <summary>
    /// Interface for embedding operations
    /// </summary>
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text);
    }    
}
