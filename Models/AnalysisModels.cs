namespace LogSummarizer.Blazor.Models;

/// <summary>
/// Complete analysis result for a given date's logs.
/// </summary>
public sealed class AnalysisResult
{
    public string Date { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public Kpis Kpis { get; set; } = new();
    public List<TopEvent> TopEvents { get; set; } = new();
    public List<RootCause> RootCauses { get; set; } = new();
    public List<ActionItem> Actions { get; set; } = new();
}

/// <summary>
/// Key performance indicators extracted from logs.
/// </summary>
public sealed class Kpis
{
    public int TotalLines { get; set; }
    public int ErrorCount { get; set; }
    public int WarnCount { get; set; }
    public int UniqueErrorTypes { get; set; }
}

/// <summary>
/// Represents a frequently occurring event type with examples.
/// </summary>
public sealed class TopEvent
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<string> Examples { get; set; } = new();
}

/// <summary>
/// Hypothesized root cause for a specific error type.
/// </summary>
public sealed class RootCause
{
    public string ErrorType { get; set; } = string.Empty;
    public string Hypothesis { get; set; } = string.Empty;
}

/// <summary>
/// Prioritized action item derived from log analysis.
/// </summary>
public sealed class ActionItem
{
    public string Priority { get; set; } = string.Empty; // "high", "medium", "low"
    public string Title { get; set; } = string.Empty;
    public string Why { get; set; } = string.Empty;
    public string OwnerHint { get; set; } = string.Empty;
}

/// <summary>
/// Wrapper for partial summary JSON from chunk-level analysis.
/// </summary>
public sealed class PartialSummary
{
    public string Json { get; set; } = string.Empty;
}
