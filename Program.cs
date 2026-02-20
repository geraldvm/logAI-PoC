using LogSummarizer.Blazor.Services;
using LogSummarizer.Blazor.Services.AI;
using LogSummarizer.Blazor.Services.IO;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Bind configuration options
builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection(OpenAiOptions.SectionName));

builder.Services.Configure<LogsOptions>(
    builder.Configuration.GetSection(LogsOptions.SectionName));

// Register HttpClient for OpenAiClient
builder.Services.AddHttpClient<OpenAiClient>((serviceProvider, client) =>
{
    var config = builder.Configuration;
    var baseUrl = config["OpenAI:BaseUrl"] ?? "https://api.openai.com/";
    client.BaseAddress = new Uri(baseUrl);
});

// Register IO services
builder.Services.AddSingleton<ILogReader, FileSystemLogReader>();
builder.Services.AddSingleton<ITextSanitizer, TextSanitizer>();
builder.Services.AddSingleton<IChunker, SimpleChunker>();

// Register AI services
builder.Services.AddSingleton<IAiSummarizer, AiSummarizer>();

// Register orchestrator
builder.Services.AddScoped<SummaryOrchestrator>();

var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
