# Ollama Setup Guide for CAD3PLogBrowser

## Overview
Ollama allows you to run Large Language Models (LLMs) locally or on a corporate server without any cloud dependencies, usage limits, or token costs.

## Requirements
- **Server**: Windows, Linux, or macOS machine with sufficient RAM (8GB minimum, 16GB+ recommended)
- **Network**: Accessible from all client machines in your organization
- **No Internet Required**: Once models are downloaded, everything runs offline

## Installation Steps

### 1. Install Ollama on Your Server

#### Windows:
1. Download Ollama from: https://ollama.ai/download/windows
2. Run the installer (`OllamaSetup.exe`)
3. Ollama will start automatically and run on `http://localhost:11434`

#### Linux:
```bash
curl -fsSL https://ollama.ai/install.sh | sh
```

#### macOS:
1. Download from: https://ollama.ai/download/mac
2. Install the app
3. Run Ollama from Applications

### 2. Pull Recommended Models

Open terminal/command prompt and run:

```bash
# For general log analysis (recommended)
ollama pull llama3

# For coding and technical analysis
ollama pull codellama

# Lightweight option (faster, less RAM)
ollama pull mistral

# Small and fast (good for testing)
ollama pull phi3
```

### 3. Configure Ollama for Network Access

By default, Ollama only accepts local connections. To allow other machines to connect:

#### Windows:
1. Create/edit environment variable:
   - Variable: `OLLAMA_HOST`
   - Value: `0.0.0.0:11434`
2. Restart Ollama service

#### Linux:
```bash
# Edit systemd service
sudo systemctl edit ollama.service

# Add these lines:
[Service]
Environment="OLLAMA_HOST=0.0.0.0:11434"

# Restart service
sudo systemctl daemon-reload
sudo systemctl restart ollama
```

#### macOS:
```bash
# Add to ~/.zshrc or ~/.bash_profile
export OLLAMA_HOST=0.0.0.0:11434

# Restart Ollama app
```

### 4. Configure Firewall

Ensure port 11434 is open on your server firewall:

#### Windows Firewall:
```powershell
New-NetFirewallRule -DisplayName "Ollama" -Direction Inbound -Port 11434 -Protocol TCP -Action Allow
```

#### Linux (ufw):
```bash
sudo ufw allow 11434/tcp
```

### 5. Test the Server

From the server:
```bash
curl http://localhost:11434/api/tags
```

From another machine in your network:
```bash
curl http://YOUR_SERVER_IP:11434/api/tags
```

You should see a JSON response with available models.

## CAD3PLogBrowser Configuration

In your application, configure the Ollama provider:

```csharp
var provider = new OllamaProvider(
    baseUrl: "http://your-server-ip:11434",  // Replace with your server IP
    model: "llama3"                          // Or your preferred model
);

// Test connection
var testResult = await provider.TestConnectionAsync();
if (testResult == null)
{
    Console.WriteLine("Connected successfully!");
}
else
{
    Console.WriteLine($"Connection failed: {testResult}");
}
```

## Recommended Models for Log Analysis

| Model | Size | RAM Required | Best For |
|-------|------|--------------|----------|
| **llama3** | 4.7GB | 8GB | General log analysis, balanced performance |
| **codellama** | 3.8GB | 8GB | Technical logs, code snippets |
| **mistral** | 4.1GB | 8GB | Fast inference, good quality |
| **phi3** | 2.3GB | 4GB | Lightweight, fast responses |

## Performance Optimization

### GPU Acceleration (Recommended)
Ollama automatically uses GPU if available:
- **NVIDIA**: Requires CUDA-compatible GPU
- **AMD**: Requires ROCm support
- **Apple Silicon**: Automatically uses Metal

### CPU-Only Mode
Works on any server, but slower. Increase timeout in your code if needed:

```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromMinutes(10) // Increase for CPU-only
};
```

## Multi-User Deployment

### Single Server Approach (Recommended)
1. Install Ollama on a dedicated server (e.g., `http://log-analyzer.corp.local:11434`)
2. Configure all CAD3PLogBrowser instances to point to this server
3. Ollama handles concurrent requests automatically

### Load Balancing (Optional, for high usage)
1. Set up multiple Ollama servers
2. Use a reverse proxy (nginx/HAProxy) to distribute load
3. Each server needs its own GPU for optimal performance

## Security Considerations

### Corporate Network
- Ollama has no built-in authentication by default
- Rely on network-level security (firewall, VPN, internal network only)
- Do NOT expose to the public internet

### Optional: Add Authentication
Use a reverse proxy with authentication:

```nginx
# nginx.conf example
server {
    listen 80;
    server_name ollama.internal;

    location / {
        auth_basic "Restricted";
        auth_basic_user_file /etc/nginx/.htpasswd;
        proxy_pass http://localhost:11434;
    }
}
```

## Monitoring

Check Ollama status:
```bash
# View running models
curl http://localhost:11434/api/tags

# Check if model is loaded
curl http://localhost:11434/api/ps
```

## Troubleshooting

### Problem: "Cannot connect to Ollama server"
- Check if Ollama service is running
- Verify firewall rules
- Test with `curl http://server-ip:11434/api/tags`

### Problem: "Model not available"
- Pull the model: `ollama pull llama3`
- List available models: `ollama list`

### Problem: Slow responses
- Check if GPU is being used: `nvidia-smi` (for NVIDIA)
- Consider using a smaller model (phi3)
- Add more RAM to the server

### Problem: Out of memory
- Use a smaller model
- Reduce concurrent requests
- Add more RAM/VRAM to server

## Cost Analysis

### Total Cost: $0 (After Hardware)
- ? No API keys required
- ? No token usage fees
- ? No monthly subscriptions
- ? Unlimited requests
- ? Complete data privacy

### Hardware Investment (One-time)
- Basic server: Existing hardware (free)
- Dedicated server with GPU: $1000-$5000 (optional, for better performance)

## Alternative: GitHub Copilot (Not Recommended for Your Use Case)

Your existing `GitHubCopilotProvider` requires:
- ? GitHub Copilot Business/Enterprise license ($19-$39/user/month)
- ? Internet connectivity
- ? Data leaves corporate network (goes to GitHub/OpenAI)
- ? Usage limits and rate limiting

## Comparison: Ollama vs GitHub Copilot vs Cloud APIs

| Feature | Ollama | GitHub Copilot | OpenAI/Claude |
|---------|--------|----------------|---------------|
| **Cost** | Free | $19-39/user/mo | Pay per token |
| **Data Privacy** | 100% private | Sent to GitHub | Sent to cloud |
| **Network** | On-premises | Internet required | Internet required |
| **Usage Limits** | None | Rate limited | Token quotas |
| **Setup Time** | 10 minutes | Account setup | API key setup |
| **Best For** | Your use case! | Code completion | General AI tasks |

## Conclusion

For your requirements:
- ? Logs stay on corporate network
- ? Completely free (no tokens, no subscriptions)
- ? Single server for all users
- ? No internet dependency

**Ollama is the perfect fit for CAD3PLogBrowser!**

## Next Steps

1. Set up Ollama on a server in your network
2. Pull the `llama3` model
3. Update your application config to use the Ollama server URL
4. Distribute the updated CAD3PLogBrowser to your team
5. Everyone connects to the same Ollama server - no per-user setup needed!

## Support

- Ollama Documentation: https://github.com/ollama/ollama/tree/main/docs
- Model Library: https://ollama.ai/library
- GitHub Issues: https://github.com/ollama/ollama/issues
