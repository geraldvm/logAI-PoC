using LogSummarizer.Blazor.Models;
using System.Text;
using System.Text.Json;

namespace LogSummarizer.Blazor.Services.AI;

/// <summary>
/// AI-powered log summarizer using OpenAI API for chunk-level and merge-level analysis.
/// </summary>
public sealed class AiSummarizer : IAiSummarizer
{
    private readonly OpenAiClient _client;
    private readonly ILogger<AiSummarizer> _logger;

    private const string SystemPrompt = """
        You are an SRE/DevOps analyst specializing in .NET application logs.
        Your task is to identify incidents, patterns, likely root causes, and actionable recommendations.
        Always respond with valid JSON matching the requested schema exactly.
        Be concise but thorough. Focus on actionable insights.
        """;

    private const string ChunkSchema = """
        {
          "errors": [{"type": "", "count": 0, "sample": ""}],
          "warnings": [{"type": "", "count": 0}],
          "observations": [""]
        }
        """;

    private const string FinalSchema = """
        {
          "date": "YYYY-MM-DD",
          "overview": "...",
          "kpis": {
            "totalLines": 0,
            "errorCount": 0,
            "warnCount": 0,
            "uniqueErrorTypes": 0
          },
          "topEvents": [
            {
              "type": "",
              "count": 0,
              "examples": [""]
            }
          ],
          "rootCauses": [
            {
              "errorType": "",
              "hypothesis": ""
            }
          ],
          "actions": [
            {
              "priority": "high|medium|low",
              "title": "",
              "why": "",
              "ownerHint": ""
            }
          ]
        }
        """;

    public AiSummarizer(OpenAiClient client, ILogger<AiSummarizer> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string BuildHeader(string serviceName, string date, string environment)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be null or empty.", nameof(serviceName));

        if (string.IsNullOrWhiteSpace(date))
            throw new ArgumentException("Date cannot be null or empty.", nameof(date));

        if (string.IsNullOrWhiteSpace(environment))
            throw new ArgumentException("Environment cannot be null or empty.", nameof(environment));

        return $"Service: {serviceName}\nDate: {date}\nEnvironment: {environment}";
    }

    /// <inheritdoc />
    public async Task<PartialSummary> SummarizeChunkAsync(
        string header,
        string chunk,
        string? modelOverride = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(header))
            throw new ArgumentException("Header cannot be null or empty.", nameof(header));

        if (string.IsNullOrWhiteSpace(chunk))
            throw new ArgumentException("Chunk cannot be null or empty.", nameof(chunk));

        var userPrompt = $"""
            {header}

            Analyze this log chunk and extract key information.
            Return JSON matching this schema:
            {ChunkSchema}

            Log chunk:
            {chunk}
            """;

        _logger.LogDebug("Summarizing chunk ({Length} chars)", chunk.Length);

        var jsonResponse = await _client.ChatCompletionAsync(
            SystemPrompt,
            userPrompt,
            modelOverride,
            cancellationToken);

        // Validate JSON
        try
        {
            JsonSerializer.Deserialize<JsonElement>(jsonResponse);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse chunk summary JSON");
            throw new InvalidOperationException("AI returned invalid JSON for chunk summary.", ex);
        }

        return new PartialSummary { Json = jsonResponse };
    }

    /// <inheritdoc />
    public async Task<AnalysisResult> MergeAsync(
        string header,
        IEnumerable<PartialSummary> partials,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(header))
            throw new ArgumentException("Header cannot be null or empty.", nameof(header));

        var partialsList = partials?.ToList() ?? throw new ArgumentNullException(nameof(partials));

        if (partialsList.Count == 0)
            throw new ArgumentException("Partials list cannot be empty.", nameof(partials));

        var sb = new StringBuilder();
        sb.AppendLine(header);
        sb.AppendLine();
        sb.AppendLine("Merge the following partial analyses into a comprehensive final report.");
        sb.AppendLine($"Return JSON matching this schema exactly:");
        sb.AppendLine(FinalSchema);
        sb.AppendLine();
        sb.AppendLine("Partial summaries:");

        for (int i = 0; i < partialsList.Count; i++)
        {
            sb.AppendLine($"--- Chunk {i + 1} ---");
            sb.AppendLine(partialsList[i].Json);
            sb.AppendLine();
        }

        _logger.LogDebug("Merging {Count} partial summaries", partialsList.Count);

        var jsonResponse = await _client.ChatCompletionAsync(
            SystemPrompt,
            sb.ToString(),
            cancellationToken: cancellationToken);

        // Deserialize and validate
        AnalysisResult? result;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            result = JsonSerializer.Deserialize<AnalysisResult>(jsonResponse, options);

            if (result == null)
                throw new InvalidOperationException("Deserialization returned null.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse final analysis JSON");
            throw new InvalidOperationException("AI returned invalid JSON for final analysis.", ex);
        }

        _logger.LogInformation("Successfully merged analysis for {Date}", result.Date);

        return result;
    }
}
