using System.Collections.Generic;
using System.Text;
using IndexCreator.Core;

namespace IndexCreator.Core.Services
{

    /// <summary>
    /// Document Chunking Service implementation
    /// </summary>
    public class DocumentChunkingService : IDocumentChunkingService
    {
        public List<(string Text, int ChunkNumber)> ChunkContent(List<string> pageTexts, int maxChunkSize, int overlap)
        {
            var chunks = new List<(string, int)>();
            var chunkNumber = 1;
            var currentChunk = new StringBuilder();

            foreach (var pageText in pageTexts)
            {
                // If adding this page would exceed the chunk size, start a new chunk
                if (currentChunk.Length + pageText.Length > maxChunkSize && currentChunk.Length > 0)
                {
                    chunks.Add((currentChunk.ToString(), chunkNumber++));

                    // Start new chunk with overlap
                    var lastText = currentChunk.ToString();
                    currentChunk.Clear();

                    if (lastText.Length > overlap)
                    {
                        currentChunk.Append(lastText.Substring(lastText.Length - overlap));
                    }
                }

                currentChunk.AppendLine(pageText);

                // If the current chunk is larger than maxChunkSize, split it
                while (currentChunk.Length > maxChunkSize)
                {
                    var chunkText = currentChunk.ToString(0, maxChunkSize);
                    chunks.Add((chunkText, chunkNumber++));

                    // Keep overlap for next chunk
                    var remainingText = currentChunk.ToString(maxChunkSize - overlap,
                        currentChunk.Length - (maxChunkSize - overlap));
                    currentChunk.Clear();
                    currentChunk.Append(remainingText);
                }
            }

            // Add the last chunk if it has content
            if (currentChunk.Length > 0)
            {
                chunks.Add((currentChunk.ToString(), chunkNumber));
            }

            return chunks;
        }
    }    
}
