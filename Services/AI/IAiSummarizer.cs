using LogSummarizer.Blazor.Models;

namespace LogSummarizer.Blazor.Services.AI;

/// <summary>
/// Abstraction for AI-powered log summarization.
/// </summary>
public interface IAiSummarizer
{
    /// <summary>
    /// Builds the header section of the prompt containing context (service, date, environment).
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="date">Date in YYYY-MM-DD format.</param>
    /// <param name="environment">Environment (e.g., production, staging).</param>
    /// <returns>Formatted header string.</returns>
    string BuildHeader(string serviceName, string date, string environment);

    /// <summary>
    /// Summarizes a single chunk of log text.
    /// </summary>
    /// <param name="header">Context header.</param>
    /// <param name="chunk">Log chunk text.</param>
    /// <param name="modelOverride">Optional model override.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Partial summary as JSON.</returns>
    Task<PartialSummary> SummarizeChunkAsync(
        string header,
        string chunk,
        string? modelOverride = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Merges multiple partial summaries into a final comprehensive analysis.
    /// </summary>
    /// <param name="header">Context header.</param>
    /// <param name="partials">Partial summaries from chunks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Final analysis result.</returns>
    Task<AnalysisResult> MergeAsync(
        string header,
        IEnumerable<PartialSummary> partials,
        CancellationToken cancellationToken = default);
}
