# Ollama Connection Troubleshooting Guide

## Common Error: JSON Unmarshal Error

### Error Message
```
{"error":"json: cannot unmarshal string into Go struct field ChatRequest.messages of type []api.Message"}
```

### Root Cause
This error occurred because the JSON serialization wasn't properly handling nested list structures. The `messages` array was being serialized incorrectly.

### Fix Applied
Updated `JsonHelper.SerializeValue()` method to properly handle:
1. `Dictionary<string, string>` objects
2. Any `IEnumerable` collection (lists, arrays, etc.)
3. Nested structures within dictionaries

## Testing the Fix

### 1. Prerequisites
- Ollama must be installed and running
- At least one model must be pulled (e.g., `ollama pull llama3`)

### 2. Verify Ollama is Running
Open a terminal and run:
```bash
# Check if Ollama is running
ollama list

# You should see a list of installed models like:
# NAME            ID              SIZE    MODIFIED
# llama3:latest   365c0bd3c000    4.7 GB  2 days ago
```

### 3. Test from CAD3PLogBrowser
1. Open CAD3PLogBrowser
2. Go to **Settings** (or press appropriate shortcut)
3. Navigate to **AI & Integration** tab
4. Configure Ollama:
   - Check **Enable AI Features**
   - Select **Ollama (Self-Hosted)** from AI Provider dropdown
   - Ollama Server: `http://localhost:11434` (or your server URL)
   - Ollama Model: Select from dropdown (e.g., `llama3`)
5. Click **Test Connection**

### 4. Expected Results

#### Success
```
? Connection successful!
```

### 5. Confirm the AI Assistant Panel Is Actually Using Ollama

A successful **Test Connection** only confirms the settings dialog *can* reach the server — it does not by itself guarantee the AI Assistant tab is using it. After saving settings:

1. Open the **AI Assistant** tab.
2. Look at the status label at the top of the panel:
   - `Ollama ready — real analysis` ? real requests are being sent to Ollama.
   - `Sample Mode (AI not configured)` ? **Enable AI Features** is unchecked, no provider is selected, or settings weren't saved — real requests are NOT being sent.
3. Every response from the panel is either real AI output, or a clearly-labeled **?? SAMPLE RESPONSE** (a deterministic, offline summary generated from log statistics only). If you see the SAMPLE RESPONSE banner even though Test Connection succeeded, re-check that AI is enabled and Ollama is the selected provider, then reopen the AI tab (or click the panel's Settings button, which refreshes the active provider).
4. If a request that started with real Ollama analysis switches mid-stream to a Sample Response, the banner will include the underlying error (e.g. connection dropped) — use that message together with the sections below to diagnose it.

#### Common Failures

**Server Not Reachable**
```
? Connection failed: Cannot connect to Ollama at http://localhost:11434. Make sure Ollama is installed and running.
```
**Solution:** Start Ollama service
- Windows: Run `ollama serve` in terminal
- Linux/Mac: Ollama should be running as a service, or run `ollama serve`

**Model Not Available**
```
? Connection failed: Model 'llama3' is not available on the Ollama server. Please pull it first using: ollama pull llama3
```
**Solution:** Pull the model
```bash
ollama pull llama3
```

**Wrong Server URL**
```
? Connection failed: Cannot connect to Ollama server at http://wrong-url:11434. Status: NotFound
```
**Solution:** Verify your Ollama server URL. Common URLs:
- Local: `http://localhost:11434`
- Network: `http://<server-ip>:11434`
- Custom port: `http://localhost:<custom-port>`

## Debug Information

### Viewing Request Payloads
When debugging, check the **Output** window in Visual Studio. The provider now logs request payloads:
```
Ollama Request Payload: {"model":"llama3","messages":[{"role":"user","content":"Say 'Hello from Ollama!'"}],"stream":false,"options":{"temperature":0.7,"num_predict":50}}
```

This helps verify:
1. Model name is correct
2. Messages array is properly formatted
3. Options are being sent correctly

### Valid Request Format
The Ollama API expects:
```json
{
  "model": "llama3",
  "messages": [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Hello!"}
  ],
  "stream": false,
  "options": {
    "temperature": 0.7,
    "num_predict": 4096
  }
}
```

**Note:** The `messages` field must be an **array of objects**, not a string.

## Network/Firewall Issues

### Corporate Networks
If using Ollama on a corporate server:
1. Verify firewall allows connections on port 11434
2. Check if proxy settings are required
3. Ensure the server URL is accessible from your machine

### Testing Connectivity
```bash
# Test if Ollama server is reachable
curl http://localhost:11434/api/tags

# Expected response:
# {"models":[{"name":"llama3:latest",...}]}
```

## Advanced Debugging

### Enable Verbose Logging
1. Open Visual Studio
2. Run CAD3PLogBrowser in Debug mode
3. Open **View ? Output** window
4. Select **Debug** from the dropdown
5. Look for "Ollama Request Payload" entries

### Manual API Testing
Test Ollama directly using curl:
```bash
curl http://localhost:11434/api/chat -d '{
  "model": "llama3",
  "messages": [
    {"role": "user", "content": "Hello!"}
  ],
  "stream": false
}'
```

Expected response:
```json
{
  "model": "llama3",
  "created_at": "2024-01-01T00:00:00.000000Z",
  "message": {
    "role": "assistant",
    "content": "Hello! How can I help you today?"
  },
  "done": true
}
```

## Getting Help

If you continue to experience issues:
1. Check the Visual Studio Output window for detailed error messages
2. Verify your Ollama version: `ollama --version`
3. Review Ollama logs (location varies by OS)
4. Test Ollama independently before integrating with CAD3PLogBrowser

## Related Files
- `OllamaProvider.cs` - Main provider implementation
- `JsonHelper.cs` - JSON serialization utilities
- `AIService.cs` - AI service coordinator
- `SettingsForm.cs` - Settings UI and test connection button
