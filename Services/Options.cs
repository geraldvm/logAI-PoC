namespace LogSummarizer.Blazor.Services;

/// <summary>
/// Configuration options for OpenAI API integration.
/// </summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string BaseUrl { get; set; } = "https://api.openai.com/";
    public string Model { get; set; } = "gpt-4.1-mini";
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Configuration options for log file storage.
/// </summary>
public sealed class LogsOptions
{
    public const string SectionName = "Logs";

    public string Root { get; set; } = "logs";
}
