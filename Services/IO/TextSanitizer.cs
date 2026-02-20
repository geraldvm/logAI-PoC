using System.Text.RegularExpressions;

namespace LogSummarizer.Blazor.Services.IO;

/// <summary>
/// Sanitizes sensitive information from log text using regex patterns.
/// </summary>
public sealed partial class TextSanitizer : ITextSanitizer
{
    [GeneratedRegex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\bBearer\s+[A-Za-z0-9\-._~+/]+=*", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b", RegexOptions.Compiled)]
    private static partial Regex IPv4Regex();

    /// <inheritdoc />
    public string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sanitized = EmailRegex().Replace(input, "[EMAIL_REDACTED]");
        sanitized = BearerTokenRegex().Replace(sanitized, "Bearer [TOKEN_REDACTED]");
        sanitized = IPv4Regex().Replace(sanitized, "[IP_REDACTED]");

        return sanitized;
    }
}
