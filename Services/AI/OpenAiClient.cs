using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LogSummarizer.Blazor.Services.AI;

/// <summary>
/// Low-level client for OpenAI Responses API (/v1/responses).
/// </summary>
public sealed class OpenAiClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiClient> _logger;

    public OpenAiClient(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        IConfiguration configuration,
        ILogger<OpenAiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends a chat completion request to OpenAI API with JSON-only response format.
    /// </summary>
    /// <param name="systemPrompt">System message content.</param>
    /// <param name="userPrompt">User message content.</param>
    /// <param name="modelOverride">Optional model override.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON string from the AI response.</returns>
    public async Task<string> ChatCompletionAsync(
        string systemPrompt,
        string userPrompt,
        string? modelOverride = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt))
            throw new ArgumentException("System prompt cannot be null or empty.", nameof(systemPrompt));

        if (string.IsNullOrWhiteSpace(userPrompt))
            throw new ArgumentException("User prompt cannot be null or empty.", nameof(userPrompt));

        var apiKey = _configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key is not configured. Set it via user-secrets or OPENAI_API_KEY environment variable.");

        var model = modelOverride ?? _options.Model;

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            response_format = new { type = "json_object" },
            temperature = 0.3
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Content = content
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        _logger.LogDebug("Sending chat completion request to OpenAI with model {Model}", model);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenAI API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
            throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        var messageContent = responseObject
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(messageContent))
            throw new InvalidOperationException("OpenAI response did not contain valid content.");

        _logger.LogDebug("Received chat completion response ({Length} chars)", messageContent.Length);

        return messageContent;
    }
}
