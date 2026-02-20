# LogSummarizer - AI-Powered Log Analysis

A production-grade **.NET 8 Blazor Server** application that ingests application logs, sanitizes sensitive data, chunks them, and summarizes via **OpenAI API** to produce actionable insights.

## Features

- 🔒 **Privacy-First**: Automatically sanitizes PII and secrets (emails, tokens, IPs) before sending to AI
- 📊 **Intelligent Chunking**: Handles large log files by chunking and progressive summarization
- 🎯 **Actionable Insights**: Generates KPIs, top events, root causes, and prioritized action items
- 💾 **Offline Fallback**: Saves `summary.json` for offline viewing when API key is unavailable
- 🎨 **Modern UI**: Clean, responsive dashboard with detailed analysis visualization
- ⚙️ **SOLID Architecture**: Follows SOLID principles with DI, testable interfaces, and clean separation of concerns

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- OpenAI API Key (or compatible API endpoint)

## Quick Start

### 1. Clone/Download the Project

```bash
cd logAI-PoC
```

### 2. Configure OpenAI API Key

**Option A: User Secrets (Recommended for Development)**

```bash
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-api-key-here"
```

**Option B: Environment Variable**

```bash
# Windows (PowerShell)
$env:OPENAI_API_KEY="sk-your-api-key-here"

# Linux/Mac
export OPENAI_API_KEY="sk-your-api-key-here"
```

### 3. Run the Application

```bash
dotnet run
```

Navigate to `https://localhost:5001` or `http://localhost:5000`

## Project Structure

```
LogSummarizer.Blazor/
├── Program.cs                          # Composition root, DI setup
├── appsettings.json                    # Configuration
├── App.razor                           # Blazor app root
├── _Imports.razor                      # Global imports
├── Models/
│   └── AnalysisModels.cs              # Data contracts (AnalysisResult, KPIs, etc.)
├── Services/
│   ├── Options.cs                      # Configuration options (OpenAI, Logs)
│   ├── SummaryOrchestrator.cs         # Main workflow orchestration
│   ├── IO/
│   │   ├── ILogReader.cs              # Log reading abstraction
│   │   ├── FileSystemLogReader.cs     # File system implementation
│   │   ├── ITextSanitizer.cs          # PII/secret sanitization abstraction
│   │   ├── TextSanitizer.cs           # Regex-based sanitizer
│   │   ├── IChunker.cs                # Text chunking abstraction
│   │   └── SimpleChunker.cs           # Character-based chunker
│   └── AI/
│       ├── OpenAiClient.cs            # Low-level OpenAI API client
│       ├── IAiSummarizer.cs           # AI summarization abstraction
│       └── AiSummarizer.cs            # Chunk & merge summarization logic
├── Pages/
│   ├── _Host.cshtml                   # Blazor host page
│   └── Index.razor                    # Main dashboard UI
├── wwwroot/
│   └── css/
│       └── site.css                   # Application styling
└── logs/                              # Log files organized by date
    └── YYYY-MM-DD/
        ├── *.log                      # Log files
        └── summary.json               # Generated analysis (cached)
```

## Usage

### 1. Prepare Log Files

Place your log files in the `logs/YYYY-MM-DD/` directory structure:

```
logs/
  2025-10-01/
    app.log
    errors.log
```

Sample logs are included in `logs/2025-10-01/` for testing.

### 2. Run Analysis

1. Enter **Service Name** (e.g., "OrderService")
2. Enter **Environment** (e.g., "Production")
3. Select **Date** (YYYY-MM-DD format)
4. Optionally override **Logs Root** path
5. Click **Analyze Logs**

### 3. View Results

The dashboard displays:

- **Overview**: High-level summary of log analysis
- **KPIs**: Total lines, error count, warning count, unique error types
- **Top Events**: Most frequent events with expandable examples
- **Root Causes**: Hypothesized causes for major error types
- **Actions**: Prioritized action items (High/Medium/Low) with owner hints

### 4. Offline Mode

If the OpenAI API key is not configured, the app automatically attempts to load the cached `summary.json` from the date folder (if it exists from a previous run).

## Configuration

Edit `appsettings.json` to customize:

```json
{
  "OpenAI": {
    "BaseUrl": "https://api.openai.com/",
    "Model": "gpt-4.1-mini"
  },
  "Logs": {
    "Root": "logs"
  }
}
```

## Architecture Principles

This application follows **SOLID** design principles:

- **Single Responsibility**: Each class has one clear purpose
- **Open/Closed**: Extend via interfaces, not modification
- **Liskov Substitution**: Abstractions are fully substitutable
- **Interface Segregation**: Small, focused interfaces
- **Dependency Inversion**: Depends on abstractions, not concretions

Additional standards:
- Async/await throughout
- Strongly-typed configuration via `IOptions<T>`
- Defensive coding with null guards and input validation
- Clear separation: IO → AI → Orchestration → UI

## Security & Privacy

- **Never sends raw secrets**: All PII (emails, bearer tokens, IPs) are sanitized before AI processing
- **Regex-based sanitization**: Configurable patterns for different secret types
- **API key protection**: Stored in user-secrets or environment variables, never in code

## Testing

### Unit Testing Hooks

All services use interfaces for easy mocking:
- `ILogReader` for file system operations
- `ITextSanitizer` for sanitization logic
- `IChunker` for chunking algorithms
- `IAiSummarizer` for AI integration

### Manual Testing

1. Use the provided sample logs in `logs/2025-10-01/`
2. Run analysis and verify results
3. Test offline mode by removing API key and ensuring cached summary loads

## Troubleshooting

### "OpenAI API key is not configured"

- Ensure you've set the API key via user-secrets or environment variable
- Verify the key starts with `sk-`
- Check `appsettings.json` has correct `OpenAI` section

### "No logs found for {date}"

- Verify logs exist in `logs/YYYY-MM-DD/` directory
- Check date format is exactly `YYYY-MM-DD`
- Ensure log files have `.log` extension

### "AI returned invalid JSON"

- This may occur with large/complex logs
- Try analyzing a smaller date range
- Check OpenAI API status
- Review model configuration in `appsettings.json`

## Future Enhancements

- ✅ Ticket creation payloads (Jira/GitLab) from actions
- ✅ Event classification by domain (app/infrastructure/dependency)
- ✅ RAG over internal runbooks for guided remediation
- ✅ Background job for nightly auto-analysis with notifications
- ✅ Anomaly detection pre-stage (z-score/Isolation Forest)

## License

This is a demonstration project. Modify and use as needed.

## Support

For issues or questions, please refer to the inline code documentation and SOLID architecture principles outlined in `CLAUDE.md`.
