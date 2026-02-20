namespace LogSummarizer.Blazor.Services.IO;

/// <summary>
/// Abstraction for sanitizing sensitive data from text.
/// </summary>
public interface ITextSanitizer
{
    /// <summary>
    /// Sanitizes PII and secrets (emails, bearer tokens, IPv4 addresses) from input text.
    /// </summary>
    /// <param name="input">Raw text to sanitize.</param>
    /// <returns>Sanitized text with sensitive data masked.</returns>
    string Sanitize(string input);
}
