using System;
using System.Drawing;
using System.Windows.Forms;
using Cad3PLogBrowser.AI.Models;
using Cad3PLogBrowser.AI.Security;
using Cad3PLogBrowser.AI.Services;
using Cad3PLogBrowser.UI;

namespace Cad3PLogBrowser.UI.AI
{
    public partial class AISettingsDialog : Form
    {
        private AISettings _settings;
        private AIService _aiService;

        // Controls
        private GroupBox grpProvider;
        private CheckBox chkEnableAI;
        private Label lblProvider;
        private ComboBox cmbProvider;
        private Label lblApiKey;
        private TextBox txtApiKey;
        private Button btnShowHideKey;
        private Label lblApiKeyHelp;
        private LinkLabel lnkGetApiKey;

        private GroupBox grpModel;
        private Label lblModel;
        private ComboBox cmbModel;
        private Label lblTemperature;
        private TrackBar trackTemperature;
        private Label lblTemperatureValue;
        private Label lblMaxTokens;
        private NumericUpDown numMaxTokens;
        private CheckBox chkStreaming;

        private GroupBox grpPrivacy;
        private CheckBox chkRedactData;
        private Label lblRedactInfo;

        private GroupBox grpConversation;
        private CheckBox chkRememberConversation;
        private Label lblMaxMessages;
        private NumericUpDown numMaxMessages;

        private Button btnTestConnection;
        private Button btnSave;
        private Button btnCancel;
        private Panel pnlButtons;

        private Label lblStatus;

        public AISettingsDialog()
        {
            InitializeComponent();
            Icon = AppIcon.Get();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "AI Settings";
            this.Size = new Size(600, 680);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F);

            int y = 10;

            // Provider Group
            grpProvider = new GroupBox { Text = "AI Provider", Location = new Point(10, y), Size = new Size(560, 180) };

            chkEnableAI = new CheckBox { Text = "Enable AI Features", Location = new Point(10, 25), Size = new Size(200, 20), Checked = true };
            chkEnableAI.CheckedChanged += chkEnableAI_CheckedChanged;

            lblProvider = new Label { Text = "Provider:", Location = new Point(10, 55), Size = new Size(80, 20) };
            cmbProvider = new ComboBox { Location = new Point(100, 53), Size = new Size(440, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProvider.Items.AddRange(new object[] { "Mock (Testing)", "Anthropic Claude", "GitHub Copilot", "OpenAI (Coming Soon)", "Azure OpenAI (Coming Soon)", "Google Gemini (Coming Soon)" });
            cmbProvider.SelectedIndex = 0;
            cmbProvider.SelectedIndexChanged += cmbProvider_SelectedIndexChanged;

            lblApiKey = new Label { Text = "API Key:", Location = new Point(10, 90), Size = new Size(80, 20) };
            txtApiKey = new TextBox { Location = new Point(100, 88), Size = new Size(390, 25), UseSystemPasswordChar = true };
            btnShowHideKey = new Button { Text = "??", Location = new Point(495, 88), Size = new Size(45, 25), FlatStyle = FlatStyle.Flat };
            btnShowHideKey.Click += btnShowHideKey_Click;

            lblApiKeyHelp = new Label { Text = "Mock provider works offline - no API key needed", Location = new Point(100, 118), Size = new Size(440, 20), ForeColor = Color.Gray };
            lnkGetApiKey = new LinkLabel { Text = "Get API Key ?", Location = new Point(100, 143), Size = new Size(150, 20), Visible = false };
            lnkGetApiKey.LinkClicked += lnkGetApiKey_LinkClicked;

            grpProvider.Controls.AddRange(new Control[] { chkEnableAI, lblProvider, cmbProvider, lblApiKey, txtApiKey, btnShowHideKey, lblApiKeyHelp, lnkGetApiKey });

            y += 190;

            // Model Group
            grpModel = new GroupBox { Text = "Model Configuration", Location = new Point(10, y), Size = new Size(560, 180) };

            lblModel = new Label { Text = "Model:", Location = new Point(10, 25), Size = new Size(80, 20) };
            cmbModel = new ComboBox { Location = new Point(100, 23), Size = new Size(440, 25) };
            cmbModel.Items.Add("(Provider not selected)");
            cmbModel.SelectedIndex = 0;

            lblTemperature = new Label { Text = "Temperature:", Location = new Point(10, 60), Size = new Size(80, 20) };
            trackTemperature = new TrackBar { Location = new Point(100, 55), Size = new Size(390, 45), Minimum = 0, Maximum = 20, Value = 7, TickFrequency = 1 };
            trackTemperature.ValueChanged += trackTemperature_ValueChanged;
            lblTemperatureValue = new Label { Text = "0.7", Location = new Point(495, 60), Size = new Size(45, 20), TextAlign = ContentAlignment.MiddleRight };

            Label lblTempHelp = new Label { Text = "Lower = more focused, Higher = more creative", Location = new Point(100, 100), Size = new Size(440, 20), ForeColor = Color.Gray, Font = new Font("Segoe UI", 8F) };

            lblMaxTokens = new Label { Text = "Max Tokens:", Location = new Point(10, 130), Size = new Size(80, 20) };
            numMaxTokens = new NumericUpDown { Location = new Point(100, 128), Size = new Size(100, 25), Minimum = 100, Maximum = 200000, Value = 4096, Increment = 100 };

            chkStreaming = new CheckBox { Text = "Enable streaming responses (recommended)", Location = new Point(210, 130), Size = new Size(330, 20), Checked = true };

            grpModel.Controls.AddRange(new Control[] { lblModel, cmbModel, lblTemperature, trackTemperature, lblTemperatureValue, lblTempHelp, lblMaxTokens, numMaxTokens, chkStreaming });

            y += 190;

            // Privacy Group
            grpPrivacy = new GroupBox { Text = "Privacy & Security", Location = new Point(10, y), Size = new Size(560, 85) };

            chkRedactData = new CheckBox { Text = "Redact sensitive data before sending to AI", Location = new Point(10, 25), Size = new Size(540, 20), Checked = true };
            lblRedactInfo = new Label { 
                Text = "Automatically removes emails, IP addresses, file paths, and other PII from logs",
                Location = new Point(10, 50), 
                Size = new Size(540, 25), 
                ForeColor = Color.Gray, 
                Font = new Font("Segoe UI", 8F) 
            };

            grpPrivacy.Controls.AddRange(new Control[] { chkRedactData, lblRedactInfo });

            y += 95;

            // Conversation Group
            grpConversation = new GroupBox { Text = "Conversation Settings", Location = new Point(10, y), Size = new Size(560, 85) };

            chkRememberConversation = new CheckBox { Text = "Remember conversation history", Location = new Point(10, 25), Size = new Size(250, 20), Checked = true };
            lblMaxMessages = new Label { Text = "Max messages:", Location = new Point(10, 55), Size = new Size(90, 20) };
            numMaxMessages = new NumericUpDown { Location = new Point(105, 53), Size = new Size(80, 25), Minimum = 5, Maximum = 100, Value = 20 };

            grpConversation.Controls.AddRange(new Control[] { chkRememberConversation, lblMaxMessages, numMaxMessages });

            y += 95;

            // Status Label
            lblStatus = new Label { 
                Text = "", 
                Location = new Point(10, y), 
                Size = new Size(560, 20), 
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 8.5F)
            };

            y += 30;

            // Buttons Panel
            pnlButtons = new Panel { Location = new Point(10, y), Size = new Size(560, 40), BackColor = SystemColors.Control };

            btnTestConnection = new Button { Text = "Test Connection", Location = new Point(0, 8), Size = new Size(130, 28) };
            btnTestConnection.Click += btnTestConnection_Click;

            btnCancel = new Button { Text = "Cancel", Location = new Point(445, 8), Size = new Size(115, 28), DialogResult = DialogResult.Cancel };
            btnSave = new Button { Text = "Save", Location = new Point(325, 8), Size = new Size(115, 28) };
            btnSave.Click += btnSave_Click;

            pnlButtons.Controls.AddRange(new Control[] { btnTestConnection, btnSave, btnCancel });

            this.Controls.AddRange(new Control[] { grpProvider, grpModel, grpPrivacy, grpConversation, lblStatus, pnlButtons });
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadSettings()
        {
            _settings = AISettingsService.Load();

            chkEnableAI.Checked = _settings.EnableAI;
            cmbProvider.SelectedIndex = (int)_settings.SelectedProvider;
            txtApiKey.Text = _settings.GetCurrentApiKey();
            cmbModel.Text = _settings.GetCurrentModel();
            trackTemperature.Value = (int)(_settings.Temperature * 10);
            numMaxTokens.Value = _settings.MaxTokens;
            chkStreaming.Checked = _settings.EnableStreaming;
            chkRedactData.Checked = _settings.RedactSensitiveData;
            chkRememberConversation.Checked = _settings.RememberConversation;
            numMaxMessages.Value = _settings.MaxConversationMessages;

            UpdateProviderFields();
            UpdateControlsState();
        }

        private void chkEnableAI_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControlsState();
        }

        private void UpdateControlsState()
        {
            bool enabled = chkEnableAI.Checked;
            cmbProvider.Enabled = enabled;
            txtApiKey.Enabled = enabled && cmbProvider.SelectedIndex > 0;
            btnShowHideKey.Enabled = txtApiKey.Enabled;
            cmbModel.Enabled = enabled;
            trackTemperature.Enabled = enabled;
            numMaxTokens.Enabled = enabled;
            chkStreaming.Enabled = enabled;
            chkRedactData.Enabled = enabled;
            chkRememberConversation.Enabled = enabled;
            numMaxMessages.Enabled = enabled && chkRememberConversation.Checked;
            btnTestConnection.Enabled = enabled;
        }

        private void cmbProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProviderFields();
            UpdateControlsState();
        }

        private void UpdateProviderFields()
        {
            var provider = (AIProviderType)cmbProvider.SelectedIndex;

            cmbModel.Items.Clear();

            switch (provider)
            {
                case AIProviderType.Mock:
                    txtApiKey.Text = "";
                    txtApiKey.Enabled = false;
                    btnShowHideKey.Enabled = false;
                    lblApiKeyHelp.Text = "Mock provider works offline - no API key needed";
                    lnkGetApiKey.Visible = false;
                    cmbModel.Items.Add("mock-model-1.0");
                    cmbModel.SelectedIndex = 0;
                    break;

                case AIProviderType.Anthropic:
                    txtApiKey.Text = _settings.AnthropicApiKey;
                    txtApiKey.Enabled = chkEnableAI.Checked;
                    btnShowHideKey.Enabled = txtApiKey.Enabled;
                    lblApiKeyHelp.Text = "Enter your Anthropic API key";
                    lnkGetApiKey.Visible = true;
                    lnkGetApiKey.Text = "Get API Key from console.anthropic.com ?";
                    cmbModel.Items.AddRange(new[] { 
                        "claude-3-5-sonnet-20241022",
                        "claude-3-opus-latest",
                        "claude-3-haiku-latest"
                    });
                    cmbModel.SelectedIndex = 0;
                    break;

                case AIProviderType.GitHubCopilot:
                    txtApiKey.Text = _settings.GitHubCopilotApiToken;
                    txtApiKey.Enabled = chkEnableAI.Checked;
                    btnShowHideKey.Enabled = txtApiKey.Enabled;
                    lblApiKeyHelp.Text = "Enter your GitHub Personal Access Token with Copilot scope";
                    lnkGetApiKey.Visible = true;
                    lnkGetApiKey.Text = "Get PAT from github.com/settings/tokens ?";
                    cmbModel.Items.AddRange(new[] { 
                        "gpt-4",
                        "gpt-4-turbo",
                        "gpt-3.5-turbo"
                    });
                    cmbModel.SelectedIndex = 0;
                    break;

                default:
                    txtApiKey.Text = "";
                    txtApiKey.Enabled = false;
                    btnShowHideKey.Enabled = false;
                    lblApiKeyHelp.Text = "This provider is coming soon";
                    lnkGetApiKey.Visible = false;
                    cmbModel.Items.Add("(Coming soon)");
                    cmbModel.SelectedIndex = 0;
                    break;
            }
        }

        private void btnShowHideKey_Click(object sender, EventArgs e)
        {
            txtApiKey.UseSystemPasswordChar = !txtApiKey.UseSystemPasswordChar;
            btnShowHideKey.Text = txtApiKey.UseSystemPasswordChar ? "??" : "??";
        }

        private void trackTemperature_ValueChanged(object sender, EventArgs e)
        {
            lblTemperatureValue.Text = (trackTemperature.Value / 10.0).ToString("0.0");
        }

        private void lnkGetApiKey_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var provider = (AIProviderType)cmbProvider.SelectedIndex;
            string url = "";

            switch (provider)
            {
                case AIProviderType.Anthropic:
                    url = "https://console.anthropic.com/";
                    break;
                case AIProviderType.GitHubCopilot:
                    url = "https://github.com/settings/tokens";
                    break;
            }

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                catch
                {
                    MessageBox.Show($"Please visit: {url}", "API Key");
                }
            }
        }

        private async void btnTestConnection_Click(object sender, EventArgs e)
        {
            SaveCurrentSettings();

            _aiService = new AIService(_settings);

            if (!_aiService.IsEnabled)
            {
                lblStatus.ForeColor = Color.DarkOrange;
                lblStatus.Text = "? AI is disabled or not configured";
                return;
            }

            btnTestConnection.Enabled = false;
            btnTestConnection.Text = "Testing...";
            lblStatus.Text = "Testing connection...";
            lblStatus.ForeColor = Color.Blue;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                var (success, message) = await _aiService.TestConnectionAsync();

                if (success)
                {
                    lblStatus.ForeColor = Color.DarkGreen;
                    lblStatus.Text = "? Connection successful!";
                    MessageBox.Show(
                        "Connection successful!\n\nYour AI provider is configured correctly and ready to use.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.ForeColor = Color.DarkRed;
                    lblStatus.Text = "? Connection failed";
                    MessageBox.Show(
                        $"Connection failed:\n\n{message}\n\nPlease check your API key and try again.",
                        "Connection Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.DarkRed;
                lblStatus.Text = "? Error occurred";
                MessageBox.Show(
                    $"Error testing connection:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnTestConnection.Enabled = true;
                btnTestConnection.Text = "Test Connection";
                this.Cursor = Cursors.Default;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate
            if (chkEnableAI.Checked && cmbProvider.SelectedIndex > 0)
            {
                if (string.IsNullOrWhiteSpace(txtApiKey.Text))
                {
                    MessageBox.Show(
                        "Please enter an API key or select Mock provider for testing.",
                        "Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtApiKey.Focus();
                    return;
                }
            }

            SaveCurrentSettings();

            if (AISettingsService.Save(_settings))
            {
                MessageBox.Show(
                    "Settings saved successfully!\n\nAI features are now configured.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "Failed to save settings.\n\nPlease try again.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SaveCurrentSettings()
        {
            _settings.EnableAI = chkEnableAI.Checked;
            _settings.SelectedProvider = (AIProviderType)cmbProvider.SelectedIndex;
            _settings.Temperature = trackTemperature.Value / 10.0;
            _settings.MaxTokens = (int)numMaxTokens.Value;
            _settings.EnableStreaming = chkStreaming.Checked;
            _settings.RedactSensitiveData = chkRedactData.Checked;
            _settings.RememberConversation = chkRememberConversation.Checked;
            _settings.MaxConversationMessages = (int)numMaxMessages.Value;

            // Save API key based on provider
            switch (_settings.SelectedProvider)
            {
                case AIProviderType.Anthropic:
                    _settings.AnthropicApiKey = txtApiKey.Text.Trim();
                    _settings.AnthropicModel = cmbModel.Text;
                    break;
                case AIProviderType.GitHubCopilot:
                    _settings.GitHubCopilotApiToken = txtApiKey.Text.Trim();
                    _settings.GitHubCopilotModel = cmbModel.Text;
                    break;
                // Add other providers when implemented
            }
        }
    }
}
