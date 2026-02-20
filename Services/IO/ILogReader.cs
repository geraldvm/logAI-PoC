namespace LogSummarizer.Blazor.Services.IO;

/// <summary>
/// Abstraction for reading log files.
/// </summary>
public interface ILogReader
{
    /// <summary>
    /// Reads all log lines for the specified date from the configured root directory.
    /// </summary>
    /// <param name="date">Date in YYYY-MM-DD format.</param>
    /// <param name="logsRoot">Optional override for logs root directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All log lines concatenated.</returns>
    Task<string> ReadLogsAsync(string date, string? logsRoot = null, CancellationToken cancellationToken = default);
}
