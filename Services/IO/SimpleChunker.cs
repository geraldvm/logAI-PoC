namespace LogSummarizer.Blazor.Services.IO;

/// <summary>
/// Simple character-based chunker that splits text into approximately equal-sized chunks.
/// </summary>
public sealed class SimpleChunker : IChunker
{
    /// <inheritdoc />
    public IEnumerable<string> Chunk(string input, int maxChunkSize = 6000)
    {
        if (string.IsNullOrEmpty(input))
            yield break;

        if (maxChunkSize <= 0)
            throw new ArgumentException("Chunk size must be positive.", nameof(maxChunkSize));

        var position = 0;
        while (position < input.Length)
        {
            var length = Math.Min(maxChunkSize, input.Length - position);

            // Try to break at a newline to avoid splitting lines
            if (position + length < input.Length)
            {
                var lastNewline = input.LastIndexOf('\n', position + length, length);
                if (lastNewline > position)
                    length = lastNewline - position + 1;
            }

            yield return input.Substring(position, length);
            position += length;
        }
    }
}
