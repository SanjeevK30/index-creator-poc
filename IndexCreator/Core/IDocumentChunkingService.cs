using System.Collections.Generic;

namespace IndexCreator.Core
{
    /// <summary>
    /// Interface for text chunking operations
    /// </summary>
    public interface IDocumentChunkingService
    {
        List<(string Text, int ChunkNumber)> ChunkContent(List<string> pageTexts, int maxChunkSize, int overlap);
    }   
}
