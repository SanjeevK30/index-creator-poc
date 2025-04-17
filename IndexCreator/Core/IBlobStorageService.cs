using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndexCreator.Core
{
    // Interfaces

    /// <summary>
    /// Interface for Azure Blob Storage operations
    /// </summary>
    public interface IBlobStorageService
    {
        Task<List<string>> UploadFilesToStorageAsync(string containerName, string[] filePaths);
    }   
}
