using LogSummarizer.Blazor.Models;
using LogSummarizer.Blazor.Services.AI;
using LogSummarizer.Blazor.Services.IO;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace LogSummarizer.Blazor.Services;

/// <summary>
/// Progress report from orchestrator.
/// </summary>
/// <param name="Index">Current step index (0-based).</param>
/// <param name="Total">Total number of steps.</param>
/// <param name="Stage">Description of current stage.</param>
public record ProgressReport(int Index, int Total, string Stage);

/// <summary>
/// Orchestrates the complete log analysis workflow: read → sanitize → chunk → summarize → merge.
/// </summary>
public sealed class SummaryOrchestrator
{
    private readonly ILogReader _logReader;
    private readonly ITextSanitizer _sanitizer;
    private readonly IChunker _chunker;
    private readonly IAiSummarizer _aiSummarizer;
    private readonly LogsOptions _logsOptions;
    private readonly ILogger<SummaryOrchestrator> _logger;

    private readonly List<PartialSummary> _partialBuffer = new();

    public SummaryOrchestrator(
        ILogReader logReader,
        ITextSanitizer sanitizer,
        IChunker chunker,
        IAiSummarizer aiSummarizer,
        IOptions<LogsOptions> logsOptions,
        ILogger<SummaryOrchestrator> logger)
    {
        _logReader = logReader ?? throw new ArgumentNullException(nameof(logReader));
        _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
        _chunker = chunker ?? throw new ArgumentNullException(nameof(chunker));
        _aiSummarizer = aiSummarizer ?? throw new ArgumentNullException(nameof(aiSummarizer));
        _logsOptions = logsOptions?.Value ?? throw new ArgumentNullException(nameof(logsOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resets internal state (clears partial buffer).
    /// </summary>
    public void Reset()
    {
        _partialBuffer.Clear();
    }

    /// <summary>
    /// Executes the full analysis workflow with progress reporting.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="environment">Environment (e.g., production).</param>
    /// <param name="date">Date in YYYY-MM-DD format.</param>
    /// <param name="logsRoot">Optional override for logs root directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of progress reports, final item contains the result.</returns>
    public async IAsyncEnumerable<(ProgressReport Progress, AnalysisResult? Result)> RunAsync(
        string serviceName,
        string environment,
        string date,
        string? logsRoot = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be null or empty.", nameof(serviceName));

        if (string.IsNullOrWhiteSpace(environment))
            throw new ArgumentException("Environment cannot be null or empty.", nameof(environment));

        if (string.IsNullOrWhiteSpace(date))
            throw new ArgumentException("Date cannot be null or empty.", nameof(date));

        Reset();

        var sw = Stopwatch.StartNew();

        // Stage 1: Read logs
        yield return (new ProgressReport(0, 5, "Reading logs..."), null);
        var rawLogs = await _logReader.ReadLogsAsync(date, logsRoot, cancellationToken);

        if (string.IsNullOrWhiteSpace(rawLogs))
        {
            _logger.LogWarning("No logs found for {Date}", date);
            throw new InvalidOperationException($"No logs found for {date}");
        }

        _logger.LogInformation("Read logs ({Length} chars) in {Elapsed}ms", rawLogs.Length, sw.ElapsedMilliseconds);

        // Stage 2: Sanitize
        yield return (new ProgressReport(1, 5, "Sanitizing sensitive data..."), null);
        sw.Restart();
        var sanitized = _sanitizer.Sanitize(rawLogs);
        _logger.LogInformation("Sanitized logs in {Elapsed}ms", sw.ElapsedMilliseconds);

        // Stage 3: Chunk
        yield return (new ProgressReport(2, 5, "Chunking logs..."), null);
        sw.Restart();
        var chunks = _chunker.Chunk(sanitized, maxChunkSize: 6000).ToList();
        _logger.LogInformation("Created {Count} chunks in {Elapsed}ms", chunks.Count, sw.ElapsedMilliseconds);

        // Stage 4: Summarize chunks
        var header = _aiSummarizer.BuildHeader(serviceName, date, environment);

        for (int i = 0; i < chunks.Count; i++)
        {
            yield return (new ProgressReport(3, 5, $"Summarizing chunk {i + 1}/{chunks.Count}..."), null);
            sw.Restart();

            var partial = await _aiSummarizer.SummarizeChunkAsync(header, chunks[i], cancellationToken: cancellationToken);
            _partialBuffer.Add(partial);

            _logger.LogInformation("Summarized chunk {Index}/{Total} in {Elapsed}ms", i + 1, chunks.Count, sw.ElapsedMilliseconds);
        }

        // Stage 5: Merge
        yield return (new ProgressReport(4, 5, "Merging summaries..."), null);
        sw.Restart();
        var result = await _aiSummarizer.MergeAsync(header, _partialBuffer, cancellationToken);
        _logger.LogInformation("Merged analysis in {Elapsed}ms", sw.ElapsedMilliseconds);

        // Save summary.json
        await SaveSummaryAsync(result, date, logsRoot);

        yield return (new ProgressReport(5, 5, "Complete"), result);
    }

    /// <summary>
    /// Attempts to load summary.json from the date folder (fallback when API key is missing).
    /// </summary>
    /// <param name="date">Date in YYYY-MM-DD format.</param>
    /// <param name="logsRoot">Optional override for logs root directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Analysis result or null if file doesn't exist.</returns>
    public async Task<AnalysisResult?> LoadSummaryAsync(
        string date,
        string? logsRoot = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(date))
            throw new ArgumentException("Date cannot be null or empty.", nameof(date));

        var root = logsRoot ?? _logsOptions.Root;
        var summaryPath = Path.Combine(root, date, "summary.json");

        if (!File.Exists(summaryPath))
        {
            _logger.LogWarning("Summary file not found: {Path}", summaryPath);
            return null;
        }

        _logger.LogInformation("Loading summary from {Path}", summaryPath);

        var json = await File.ReadAllTextAsync(summaryPath, cancellationToken);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var result = JsonSerializer.Deserialize<AnalysisResult>(json, options);
        return result;
    }

    /// <summary>
    /// Saves the analysis result to summary.json in the date folder.
    /// </summary>
    private async Task SaveSummaryAsync(AnalysisResult result, string date, string? logsRoot = null)
    {
        var root = logsRoot ?? _logsOptions.Root;
        var dateFolder = Path.Combine(root, date);

        if (!Directory.Exists(dateFolder))
            Directory.CreateDirectory(dateFolder);

        var summaryPath = Path.Combine(dateFolder, "summary.json");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(result, options);
        await File.WriteAllTextAsync(summaryPath, json);

        _logger.LogInformation("Saved summary to {Path}", summaryPath);
    }
}
