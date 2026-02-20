# LogSummarizer - Complete Testing Guide

This guide provides step-by-step instructions for running and testing all features of the LogSummarizer application.

---

## Prerequisites Check

Before starting, ensure you have:

- [ ] .NET 8 SDK installed
- [ ] OpenAI API key (starts with `sk-`)
- [ ] Terminal/Command Prompt open
- [ ] Working directory set to `YOUR-PATH\logAI-PoC`

### Verify .NET Installation

```bash
dotnet --version
# Should output 8.0.x or higher
```

---

## Step 1: Initial Setup

### 1.1 Restore Dependencies

```bash
dotnet restore
```

**Expected Output**:
- Package restore completes successfully
- No errors displayed

### 1.2 Build the Project

```bash
dotnet build
```

**Expected Output**:
- Build succeeded with 0 warnings
- Build completed in a few seconds

### 1.3 Configure API Key

**Option A: User Secrets (Recommended)**

```bash
# Initialize user secrets
dotnet user-secrets init

# Set your OpenAI API key
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-actual-api-key-here"

# Verify it was set
dotnet user-secrets list
```

**Expected Output**:
```
OpenAI:ApiKey = sk-...
```

**Option B: Environment Variable (Alternative)**

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY="sk-your-actual-api-key-here"
```

**Windows (CMD):**
```cmd
set OPENAI_API_KEY=sk-your-actual-api-key-here
```

**Linux/Mac:**
```bash
export OPENAI_API_KEY="sk-your-actual-api-key-here"
```

---

## Step 2: Run the Application

### 2.1 Start the Server

```bash
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 2.2 Open Browser

Navigate to: **https://localhost:5001** or **http://localhost:5000**

**Expected Result**:
- Modern purple gradient interface loads
- "LogSummarizer" title with subtitle visible
- Left card shows input form
- No errors in browser console (F12)

---

## Step 3: Test Feature #1 - Basic Log Analysis

### 3.1 Configure Analysis Parameters

In the left panel, enter:

- **Service Name**: `OrderService`
- **Environment**: `Production`
- **Date**: `2025-10-01` (use date picker or type)
- **Logs Root**: Leave empty (uses default `logs/`)

### 3.2 Review Pre-Analysis State

**Verify**:
- [ ] "Analyze Logs" button is enabled (purple gradient)
- [ ] No status message displayed
- [ ] No progress bar visible
- [ ] Right panel is empty (no results)

### 3.3 Run Analysis

Click **"Analyze Logs"** button

**Expected Behavior**:

1. **Button State**:
   - Button text changes to "Analyzing..."
   - Button becomes disabled

2. **Progress Updates** (watch for these stages):
   - ✅ "Reading logs..." (0/5)
   - ✅ "Sanitizing sensitive data..." (1/5)
   - ✅ "Chunking logs..." (2/5)
   - ✅ "Summarizing chunk 1/1..." (3/5)
   - ✅ "Merging summaries..." (4/5)
   - ✅ "Complete" (5/5)

3. **Progress Bar**:
   - Purple gradient bar fills from 0% to 100%
   - Stage text updates with each step

4. **Completion** (after 10-30 seconds):
   - Status message: "Analysis complete!"
   - Button re-enables
   - Right panel populates with results

### 3.4 Verify Results Display

**Check Right Panel Contains**:

1. **Overview Section**:
   - [ ] Clear paragraph summarizing the day's logs
   - [ ] Mentions key issues (timeouts, errors, etc.)

2. **KPIs Section**:
   - [ ] Total Lines (should be > 0)
   - [ ] Error Count (should show ~12 errors from sample)
   - [ ] Warning Count (should show ~7 warnings from sample)
   - [ ] Unique Error Types (should be > 0)

3. **Top Events Section**:
   - [ ] Multiple event types listed
   - [ ] Each has a count badge (purple pill with number)
   - [ ] Events are collapsible (▶ arrow)
   - [ ] Click an event to expand examples
   - [ ] Examples show as code-formatted text

4. **Root Causes Section**:
   - [ ] Lists hypotheses for major errors
   - [ ] Should mention things like:
     - Inventory service timeouts
     - Database connection issues
     - Payment gateway errors
     - Null reference exceptions

5. **Actions Section**:
   - [ ] Numbered list of action items
   - [ ] Each has priority badge:
     - 🔴 HIGH (red)
     - 🟠 MEDIUM (orange)
     - 🟢 LOW (green)
   - [ ] Actions sorted by priority (high → low)
   - [ ] Each action has:
     - Title
     - "Why" explanation
     - Owner hint (when provided)

### 3.5 Verify File Generation

**Check file system**:

```bash
# Navigate to logs folder
cd logs/2025-10-01

# List files
dir  # Windows
ls   # Linux/Mac
```

**Expected Files**:
- `app.log` (original)
- `errors.log` (original)
- `summary.json` ✨ (newly generated)

**Inspect summary.json**:

```bash
# Windows
type summary.json

# Linux/Mac
cat summary.json
```

**Verify JSON Structure**:
```json
{
  "date": "2025-10-01",
  "overview": "...",
  "kpis": {
    "totalLines": ...,
    "errorCount": ...,
    "warnCount": ...,
    "uniqueErrorTypes": ...
  },
  "topEvents": [...],
  "rootCauses": [...],
  "actions": [...]
}
```

---

## Step 4: Test Feature #2 - PII Sanitization

### 4.1 Verify Sensitive Data Removal

**Check the sample logs contain**:
- Email: `user@example.com`, `customer@domain.com`
- IP Address: `192.168.1.50`, `203.0.113.42`
- Bearer Token: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9`

**Test Sanitization**:

1. Open browser DevTools (F12)
2. Go to Network tab
3. Click "Analyze Logs" again
4. Find the OpenAI API request (filter by "chat")
5. Click on it → Request Payload
6. Verify the sent content contains:
   - `[EMAIL_REDACTED]` instead of emails
   - `[IP_REDACTED]` instead of IPs
   - `Bearer [TOKEN_REDACTED]` instead of tokens

**✅ Pass Criteria**: No actual PII sent to OpenAI

---

## Step 5: Test Feature #3 - Offline Fallback Mode

### 5.1 Remove API Key

```bash
# Option A: Clear user secret
dotnet user-secrets remove "OpenAI:ApiKey"

# Option B: Unset environment variable (if using that method)
# Windows PowerShell
Remove-Item Env:OPENAI_API_KEY

# Linux/Mac
unset OPENAI_API_KEY
```

### 5.2 Refresh Browser

Press **F5** to reload the page

### 5.3 Attempt Analysis

1. Enter same parameters:
   - Service: `OrderService`
   - Environment: `Production`
   - Date: `2025-10-01`

2. Click **"Analyze Logs"**

**Expected Behavior**:

1. **Initial Error Message**:
   - Status shows: "API key not configured. Attempting to load cached summary..."

2. **Fallback Success**:
   - Status changes to: "Loaded cached summary (API key not configured)."
   - Results panel shows the **same data** from `summary.json`
   - All sections render correctly

**✅ Pass Criteria**: App gracefully falls back to cached data

### 5.4 Test Missing Cache Scenario

```bash
# Delete summary.json
cd logs/2025-10-01
rm summary.json  # Linux/Mac
del summary.json # Windows

# Or rename it
mv summary.json summary.json.backup  # Linux/Mac
ren summary.json summary.json.backup # Windows
```

**Refresh browser and try analysis again**:

**Expected Behavior**:
- Error message: "No cached summary found. Please configure OpenAI API key."
- Status shows as error (red background)
- No results displayed

**✅ Pass Criteria**: Clear error message when both API key and cache missing

### 5.5 Restore API Key

```bash
# Re-add your API key
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-actual-api-key-here"
```

---

## Step 6: Test Feature #4 - Custom Logs Directory

### 6.1 Create Alternate Log Location

```bash
# From project root
mkdir custom-logs
mkdir custom-logs/2025-10-02
```

### 6.2 Create Custom Log File

**Windows (PowerShell):**
```powershell
@"
2025-10-02 10:00:00 [INFO] Custom log test started
2025-10-02 10:01:00 [ERROR] Test error message
2025-10-02 10:02:00 [WARN] Test warning message
2025-10-02 10:03:00 [INFO] Custom log test completed
"@ | Out-File -FilePath custom-logs/2025-10-02/test.log -Encoding UTF8
```

**Linux/Mac:**
```bash
cat > custom-logs/2025-10-02/test.log << EOF
2025-10-02 10:00:00 [INFO] Custom log test started
2025-10-02 10:01:00 [ERROR] Test error message
2025-10-02 10:02:00 [WARN] Test warning message
2025-10-02 10:03:00 [INFO] Custom log test completed
EOF
```

### 6.3 Analyze Custom Logs

In the UI:
- **Service Name**: `TestService`
- **Environment**: `Development`
- **Date**: `2025-10-02`
- **Logs Root**: `custom-logs`

Click **"Analyze Logs"**

**Expected Behavior**:
- Analysis completes successfully
- Results reflect the custom log content
- `summary.json` created in `custom-logs/2025-10-02/`

**✅ Pass Criteria**: Custom directory override works

---

## Step 7: Test Feature #5 - Error Handling

### 7.1 Test Invalid Date

In the UI:
- **Date**: `2025-12-31` (non-existent logs)

Click **"Analyze Logs"**

**Expected Behavior**:
- Error status appears (red background)
- Message: "No logs found for 2025-12-31" or similar
- No crash or unhandled exception

### 7.2 Test Empty Inputs

1. Clear **Service Name**
2. Click **"Analyze Logs"**

**Expected Behavior**:
- Error message: "Please fill in all required fields."
- Analysis does not start

### 7.3 Test Network Error Simulation

**Modify appsettings.json temporarily**:
```json
{
  "OpenAI": {
    "BaseUrl": "https://invalid-url-that-does-not-exist.com/",
    "Model": "gpt-4.1-mini"
  }
}
```

**Restart app** (Ctrl+C, then `dotnet run`)

**Try analysis**:

**Expected Behavior**:
- Error status shows network/connection error
- Graceful failure message
- App remains functional

**Restore correct BaseUrl**:
```json
"BaseUrl": "https://api.openai.com/"
```

---

## Step 8: Test Feature #6 - Responsive UI

### 8.1 Test Desktop Layout

**Browser width > 1024px**:
- [ ] Two-column layout (form left, results right)
- [ ] Cards side-by-side
- [ ] Comfortable spacing

### 8.2 Test Mobile Layout

**Resize browser to < 1024px width** or use DevTools device emulation:

- [ ] Single column layout (form above results)
- [ ] Cards stack vertically
- [ ] Inputs remain full-width
- [ ] Readable on small screens

### 8.3 Test Expandable Events

1. Run analysis
2. Click on a Top Event summary

**Expected Behavior**:
- ▶ changes to ▼
- Examples section slides open
- Code-formatted examples visible
- Click again to collapse

---

## Step 9: Test Feature #7 - Large Log Handling

### 9.1 Generate Large Log File

```bash
# Create a large log for today's date
mkdir logs/2025-10-02

# Windows PowerShell
1..1000 | ForEach-Object {
  "2025-10-02 12:$("{0:D2}" -f ($_ % 60)):00 [ERROR] Error message $_"
} | Out-File -FilePath logs/2025-10-02/large.log -Encoding UTF8

# Linux/Mac
for i in {1..1000}; do
  echo "2025-10-02 12:$(printf '%02d' $((i % 60))):00 [ERROR] Error message $i"
done > logs/2025-10-02/large.log
```

### 9.2 Analyze Large Logs

In UI:
- **Date**: `2025-10-02`
- Click **"Analyze Logs"**

**Expected Behavior**:
- Progress shows multiple chunks (e.g., "Summarizing chunk 1/2...")
- Analysis completes without timeout
- Results merge all chunks

**✅ Pass Criteria**: Chunking works for large files

---

## Step 10: Verify Console Logging

### 10.1 Check Application Logs

During analysis, watch the console where `dotnet run` is active.

**Expected Log Entries**:

```
info: LogSummarizer.Blazor.Services.IO.FileSystemLogReader[0]
      Reading 2 log file(s) from logs\2025-10-01

info: LogSummarizer.Blazor.Services.IO.FileSystemLogReader[0]
      Read 3456 characters from logs for 2025-10-01

info: LogSummarizer.Blazor.Services.SummaryOrchestrator[0]
      Read logs (3456 chars) in 15ms

info: LogSummarizer.Blazor.Services.SummaryOrchestrator[0]
      Sanitized logs in 5ms

info: LogSummarizer.Blazor.Services.SummaryOrchestrator[0]
      Created 1 chunks in 2ms

dbug: LogSummarizer.Blazor.Services.AI.AiSummarizer[0]
      Summarizing chunk (3456 chars)

info: LogSummarizer.Blazor.Services.SummaryOrchestrator[0]
      Summarized chunk 1/1 in 2345ms

info: LogSummarizer.Blazor.Services.SummaryOrchestrator[0]
      Merged analysis in 3210ms

info: LogSummarizer.Blazor.Services.SummaryOrchestrator[0]
      Saved summary to logs\2025-10-01\summary.json
```

**✅ Pass Criteria**: Timing logs show performance metrics

---

## Step 11: Full Feature Checklist

Run through this complete checklist:

### Core Functionality
- [ ] App builds without errors
- [ ] App runs on localhost:5001
- [ ] UI loads with proper styling (purple gradient)
- [ ] Sample logs exist in logs/2025-10-01/
- [ ] OpenAI API key configured via user-secrets

### Analysis Workflow
- [ ] Form inputs accept text/dates
- [ ] "Analyze Logs" button triggers analysis
- [ ] Progress bar shows 5 stages
- [ ] Each stage updates with descriptive text
- [ ] Analysis completes in < 60 seconds for sample logs
- [ ] Status message shows "Analysis complete!"

### Results Display
- [ ] Overview section renders with summary text
- [ ] KPIs show 4 numeric metrics
- [ ] Top Events list with count badges
- [ ] Events expand/collapse on click
- [ ] Examples render as code blocks
- [ ] Root Causes section lists hypotheses
- [ ] Actions section shows prioritized items
- [ ] Priority badges color-coded (red/orange/green)
- [ ] Actions sorted by priority

### Privacy & Security
- [ ] Emails sanitized to [EMAIL_REDACTED]
- [ ] IPs sanitized to [IP_REDACTED]
- [ ] Tokens sanitized to Bearer [TOKEN_REDACTED]
- [ ] API key not visible in UI
- [ ] API key not logged in console

### File Operations
- [ ] summary.json created after analysis
- [ ] JSON is valid and well-formatted
- [ ] JSON contains all required fields
- [ ] File saved to correct date folder

### Offline Mode
- [ ] Works without API key if summary.json exists
- [ ] Shows helpful error if no API key and no cache
- [ ] Fallback message displayed clearly

### Custom Directories
- [ ] Accepts custom logs root path
- [ ] Reads from custom directory
- [ ] Saves summary to custom directory

### Error Handling
- [ ] Handles missing log files gracefully
- [ ] Validates required form fields
- [ ] Shows error status (red background)
- [ ] Network errors display user-friendly messages
- [ ] App doesn't crash on errors

### Performance
- [ ] Large logs (1000+ lines) process successfully
- [ ] Chunking activates for large files
- [ ] No UI freezing during analysis
- [ ] Progress updates in real-time

### Responsive Design
- [ ] Desktop layout (2 columns)
- [ ] Mobile layout (1 column stack)
- [ ] Text readable on all screen sizes
- [ ] Buttons/inputs properly sized

### Logging & Observability
- [ ] Console shows structured logs
- [ ] Timing metrics logged for each stage
- [ ] Info/Debug/Error levels used appropriately
- [ ] No sensitive data in logs

---

## Troubleshooting Common Issues

### Issue: "Port already in use"

**Solution**:
```bash
# Find process using port 5001
netstat -ano | findstr :5001  # Windows
lsof -i :5001                 # Linux/Mac

# Kill the process or change port in Properties/launchSettings.json
```

### Issue: "API key is not configured"

**Solution**:
```bash
# Verify secret is set
dotnet user-secrets list

# Re-set if missing
dotnet user-secrets set "OpenAI:ApiKey" "sk-..."
```

### Issue: Browser shows 404

**Solution**:
- Ensure you're navigating to the root: `https://localhost:5001/`
- Clear browser cache (Ctrl+Shift+Delete)
- Restart app

### Issue: Analysis times out

**Solution**:
- Check internet connection
- Verify OpenAI API key is valid
- Try with smaller log files first
- Check OpenAI API status: https://status.openai.com/

### Issue: Styles not loading

**Solution**:
```bash
# Verify CSS file exists
ls wwwroot/css/site.css

# Clear browser cache and hard reload (Ctrl+Shift+R)
# Check browser console (F12) for 404 errors
```

---

## Performance Benchmarks

**Expected performance on sample logs** (2025-10-01):

| Stage | Duration | Notes |
|-------|----------|-------|
| Read logs | < 100ms | 2 files, ~3.5KB total |
| Sanitize | < 50ms | Regex replacements |
| Chunk | < 10ms | Single chunk for sample |
| Summarize chunk | 2-5s | OpenAI API call |
| Merge | 3-6s | OpenAI API call |
| **Total** | **5-15s** | Depends on API latency |

**For larger logs** (1000+ lines):
- Multiple chunks = multiple API calls
- Expect 10-30 seconds for 10KB logs
- 30-60 seconds for 50KB+ logs

---

## Success Criteria Summary

✅ **Application is working correctly if**:

1. Builds and runs without errors
2. Analyzes sample logs (2025-10-01) successfully
3. Displays complete results with all sections
4. Generates valid summary.json
5. Sanitizes PII before sending to AI
6. Falls back to cache when API key missing
7. Handles errors gracefully without crashing
8. UI is responsive and interactive
9. Console logs show timing metrics
10. All checkboxes in Step 11 are checked

---

## Next Steps After Testing

Once all tests pass:

1. **Customize** for your use case:
   - Adjust prompts in `AiSummarizer.cs`
   - Modify sanitization patterns in `TextSanitizer.cs`
   - Update UI styling in `site.css`

2. **Deploy** to production:
   - Configure API key via environment variables
   - Set up reverse proxy (nginx/IIS)
   - Enable HTTPS
   - Configure logging to file/service

3. **Extend** functionality:
   - Add authentication
   - Implement ticket creation APIs
   - Build background job scheduler
   - Add email notifications

---

## Support

If you encounter issues not covered in this guide:

1. Check console logs for detailed error messages
2. Enable debug logging in `appsettings.json`:
   ```json
   "LogLevel": { "Default": "Debug" }
   ```
3. Review inline code documentation
4. Verify all prerequisites are met

Happy testing! 🚀
