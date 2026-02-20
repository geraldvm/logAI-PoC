using Microsoft.Extensions.Options;
using System.Text;

namespace LogSummarizer.Blazor.Services.IO;

/// <summary>
/// Reads log files from the file system organized by date folders.
/// </summary>
public sealed class FileSystemLogReader : ILogReader
{
    private readonly LogsOptions _options;
    private readonly ILogger<FileSystemLogReader> _logger;

    public FileSystemLogReader(IOptions<LogsOptions> options, ILogger<FileSystemLogReader> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<string> ReadLogsAsync(string date, string? logsRoot = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(date))
            throw new ArgumentException("Date cannot be null or empty.", nameof(date));

        var root = logsRoot ?? _options.Root;
        var dateFolder = Path.Combine(root, date);

        if (!Directory.Exists(dateFolder))
        {
            _logger.LogWarning("Log directory does not exist: {DateFolder}", dateFolder);
            return string.Empty;
        }

        var logFiles = Directory.GetFiles(dateFolder, "*.log", SearchOption.TopDirectoryOnly);

        if (logFiles.Length == 0)
        {
            _logger.LogWarning("No log files found in: {DateFolder}", dateFolder);
            return string.Empty;
        }

        _logger.LogInformation("Reading {Count} log file(s) from {DateFolder}", logFiles.Length, dateFolder);

        var sb = new StringBuilder();
        foreach (var file in logFiles.OrderBy(f => f))
        {
            var content = await File.ReadAllTextAsync(file, cancellationToken);
            sb.AppendLine(content);
        }

        var result = sb.ToString();
        _logger.LogInformation("Read {Length} characters from logs for {Date}", result.Length, date);

        return result;
    }
}
