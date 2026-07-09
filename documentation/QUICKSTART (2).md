# Quick Start Guide: Adding Ollama to CAD3PLogBrowser

## Overview
This guide shows you how to integrate the Ollama provider into your CAD3PLogBrowser application to add AI-powered log analysis capabilities.

## Step 1: Install Ollama Server (One-time Setup)

### Option A: Local Testing (on your development machine)
1. Download Ollama: https://ollama.ai/download
2. Install and run
3. Pull a model: `ollama pull llama3`
4. Verify: `curl http://localhost:11434/api/tags`

### Option B: Corporate Server (for your team)
1. Set up Ollama on a Windows/Linux server in your network
2. Configure for network access (see SETUP_GUIDE.md)
3. All team members will use this server URL

## Step 2: Update AIProviderType Enum (Already Done ?)

The `AIProviderType` enum has been updated to include `Ollama = 6`.

## Step 3: Update AISettingsService

Add Ollama configuration to your existing AI settings:

```csharp
// In AI/Security/AISettingsService.cs

public class AISettings
{
    // ... existing properties ...

    // Add these properties for Ollama
    public string OllamaServerUrl { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "llama3";
}
```

## Step 4: Update AIService to Support Ollama

In `AI/Services/AIService.cs`, update the provider factory:

```csharp
using Cad3PLogBrowser.AI.Providers.Ollama;

private IAIProvider CreateProvider(AIProviderType providerType)
{
    switch (providerType)
    {
        case AIProviderType.Ollama:
            var settings = _settingsService.GetSettings();
            return new OllamaProvider(
                settings.OllamaServerUrl ?? "http://localhost:11434",
                settings.OllamaModel ?? "llama3"
            );

        case AIProviderType.GitHubCopilot:
            // ... existing code ...

        // ... other cases ...
    }
}
```

## Step 5: Update AI Settings Dialog

In `UI/AI/AISettingsDialog.cs`, add Ollama configuration UI:

```csharp
// Add to your form designer or in InitializeComponent()

// Ollama Server URL TextBox
var ollamaUrlLabel = new Label { Text = "Ollama Server URL:", Location = new Point(10, 150) };
var ollamaUrlTextBox = new TextBox 
{ 
    Location = new Point(150, 150), 
    Width = 300,
    Text = "http://localhost:11434"
};

// Ollama Model ComboBox
var ollamaModelLabel = new Label { Text = "Ollama Model:", Location = new Point(10, 180) };
var ollamaModelComboBox = new ComboBox 
{ 
    Location = new Point(150, 180), 
    Width = 300,
    DropDownStyle = ComboBoxStyle.DropDownList
};
ollamaModelComboBox.Items.AddRange(new[] { "llama3", "codellama", "mistral", "phi3" });
ollamaModelComboBox.SelectedIndex = 0;

// Add a "Test Connection" button
var testButton = new Button 
{ 
    Text = "Test Ollama Connection", 
    Location = new Point(150, 210), 
    Width = 150 
};
testButton.Click += async (s, e) => 
{
    var provider = new OllamaProvider(ollamaUrlTextBox.Text, ollamaModelComboBox.Text);
    var result = await provider.TestConnectionAsync();
    if (result == null)
        MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    else
        MessageBox.Show(result, "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
};

// Add controls to form
Controls.Add(ollamaUrlLabel);
Controls.Add(ollamaUrlTextBox);
Controls.Add(ollamaModelLabel);
Controls.Add(ollamaModelComboBox);
Controls.Add(testButton);
```

## Step 6: Update Provider Selection

In your settings dialog, add Ollama to the provider dropdown:

```csharp
providerComboBox.Items.AddRange(new[] 
{ 
    "None",
    "OpenAI",
    "Azure OpenAI",
    "Anthropic",
    "Google Gemini",
    "GitHub Copilot",
    "Ollama"  // Add this
});
```

## Step 7: Test the Integration

### Simple Test
```csharp
using Cad3PLogBrowser.AI.Configuration;
using Cad3PLogBrowser.AI.Models;

// Create provider
var provider = OllamaConfigurationHelper.CreateLocalProvider("llama3");

// Test connection
var testResult = await provider.TestConnectionAsync();
if (testResult == null)
{
    MessageBox.Show("Ollama is ready to use!");
}
```

### Test with Real Logs
```csharp
// In your log viewer, add a context menu item "Analyze with AI"
private async void AnalyzeWithAI_Click(object sender, EventArgs e)
{
    var selectedLines = GetSelectedLogLines();
    if (selectedLines.Length == 0)
    {
        MessageBox.Show("Please select log lines to analyze.");
        return;
    }

    var provider = OllamaConfigurationHelper.CreateLocalProvider();

    var request = new AIRequest
    {
        SystemPrompt = OllamaConfigurationHelper.GetDefaultSystemPrompt(),
        Prompt = $"Analyze these logs:\\n\\n{string.Join("\\n", selectedLines)}",
        MaxTokens = 4096
    };

    var response = await provider.SendRequestAsync(request);

    if (response.Success)
    {
        // Show in a dialog or panel
        var resultForm = new Form { Text = "AI Analysis", Width = 600, Height = 400 };
        var textBox = new TextBox 
        { 
            Multiline = true, 
            ScrollBars = ScrollBars.Both, 
            Dock = DockStyle.Fill,
            Text = response.Content
        };
        resultForm.Controls.Add(textBox);
        resultForm.ShowDialog();
    }
    else
    {
        MessageBox.Show($"Analysis failed: {response.ErrorMessage}", "Error");
    }
}
```

## Step 8: Add to Main Menu

In your MainForm, add a menu item:

```csharp
// In your menu strip
var aiMenu = new ToolStripMenuItem("AI");
var analyzeMenuItem = new ToolStripMenuItem("Analyze Selected Logs");
analyzeMenuItem.Click += AnalyzeWithAI_Click;
aiMenu.DropDownItems.Add(analyzeMenuItem);
menuStrip.Items.Add(aiMenu);
```

## Step 9: Add Keyboard Shortcut

```csharp
// Add to your form's KeyDown event or use a shortcut
analyzeMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.A;
```

## Step 10: Add Status Indicator

Show Ollama connection status in your status bar:

```csharp
private async void CheckOllamaStatus()
{
    var provider = OllamaConfigurationHelper.CreateLocalProvider();
    var result = await provider.TestConnectionAsync();

    if (result == null)
    {
        statusLabel.Text = "AI: Ready (Ollama)";
        statusLabel.ForeColor = Color.Green;
    }
    else
    {
        statusLabel.Text = "AI: Unavailable";
        statusLabel.ForeColor = Color.Red;
    }
}
```

## Complete Integration Example

Here's a complete example of adding an AI analysis panel to your form:

```csharp
using Cad3PLogBrowser.AI.Configuration;
using Cad3PLogBrowser.AI.Models;

public class AIAnalysisPanel : Panel
{
    private TextBox inputTextBox;
    private Button analyzeButton;
    private RichTextBox outputTextBox;
    private ProgressBar progressBar;

    public AIAnalysisPanel()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Input area
        var inputLabel = new Label { Text = "Enter your question:", Dock = DockStyle.Top };
        inputTextBox = new TextBox { Dock = DockStyle.Top, Height = 50, Multiline = true };
        analyzeButton = new Button { Text = "Ask AI", Dock = DockStyle.Top };
        analyzeButton.Click += async (s, e) => await AnalyzeAsync();

        // Progress
        progressBar = new ProgressBar { Dock = DockStyle.Top, Style = ProgressBarStyle.Marquee, Visible = false };

        // Output area
        var outputLabel = new Label { Text = "AI Response:", Dock = DockStyle.Top };
        outputTextBox = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };

        // Add controls
        Controls.Add(outputTextBox);
        Controls.Add(outputLabel);
        Controls.Add(progressBar);
        Controls.Add(analyzeButton);
        Controls.Add(inputTextBox);
        Controls.Add(inputLabel);
    }

    private async Task AnalyzeAsync()
    {
        if (string.IsNullOrWhiteSpace(inputTextBox.Text))
        {
            MessageBox.Show("Please enter a question.");
            return;
        }

        analyzeButton.Enabled = false;
        progressBar.Visible = true;
        outputTextBox.Clear();

        try
        {
            var provider = OllamaConfigurationHelper.CreateLocalProvider();

            var request = new AIRequest
            {
                SystemPrompt = "You are a helpful assistant analyzing CAD logs.",
                Prompt = inputTextBox.Text,
                MaxTokens = 2000
            };

            // Use streaming for real-time feedback
            await provider.SendStreamingRequestAsync(
                request,
                chunk => 
                {
                    if (outputTextBox.InvokeRequired)
                        outputTextBox.Invoke(new Action(() => outputTextBox.AppendText(chunk)));
                    else
                        outputTextBox.AppendText(chunk);
                }
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "AI Error");
        }
        finally
        {
            analyzeButton.Enabled = true;
            progressBar.Visible = false;
        }
    }
}
```

## Distribution to Your Team

### Option 1: Shared Ollama Server (Recommended)
1. Set up Ollama on a corporate server (e.g., `http://ollama.corp.ptc.com:11434`)
2. In your app settings, default to this URL
3. All users automatically connect to the same server
4. No per-user setup required!

### Option 2: Local Installation
1. Include Ollama installer instructions in your documentation
2. Users install Ollama locally
3. Each user runs their own instance

## Troubleshooting

### "Cannot connect to Ollama server"
- Ensure Ollama is running: Check Task Manager or `ps aux | grep ollama`
- Verify URL: `curl http://your-server:11434/api/tags`
- Check firewall rules

### "Model not available"
- Pull the model: `ollama pull llama3`
- List models: `ollama list`

### Slow responses
- Check if GPU is being used (much faster)
- Consider using a smaller model like `phi3`
- Increase timeout in provider configuration

## Next Steps

1. ? Test locally with `ollama pull llama3`
2. ? Integrate into your AI settings dialog
3. ? Add to your main menu
4. ? Test with real log files
5. ? Deploy to corporate server
6. ? Distribute to your team

## Summary

You now have:
- ? Free, unlimited AI capabilities
- ? Complete data privacy (logs never leave your network)
- ? Single server for all users
- ? No API keys or subscriptions needed
- ? Works offline after initial setup

Perfect for CAD3PLogBrowser! ??
