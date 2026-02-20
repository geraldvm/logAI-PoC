namespace LogSummarizer.Blazor.Services.IO;

/// <summary>
/// Abstraction for chunking large text into smaller pieces.
/// </summary>
public interface IChunker
{
    /// <summary>
    /// Splits input text into chunks of approximately the specified size.
    /// </summary>
    /// <param name="input">Text to chunk.</param>
    /// <param name="maxChunkSize">Maximum characters per chunk.</param>
    /// <returns>Enumerable of text chunks.</returns>
    IEnumerable<string> Chunk(string input, int maxChunkSize = 6000);
}
