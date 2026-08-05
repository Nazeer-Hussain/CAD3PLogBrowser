using System;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;
using Cad3PLogBrowser.AI.Security;

namespace Cad3PLogBrowser.AI.Context
{
    /// <summary>
    /// Provides context from user-selected lines in the log viewer.
    /// </summary>
    public class SelectedLinesContextProvider : ContextProviderBase
    {
        private readonly Func<string> _getSelectedText;
        private readonly bool _redactSensitiveData;
        private readonly DataRedactor _redactor;

        public SelectedLinesContextProvider(
            Func<string> getSelectedText,
            ITokenEstimator tokenEstimator = null,
            bool redactSensitiveData = true)
            : base(tokenEstimator)
        {
            _getSelectedText = getSelectedText ?? throw new ArgumentNullException(nameof(getSelectedText));
            _redactSensitiveData = redactSensitiveData;
            _redactor = redactSensitiveData ? new DataRedactor() : null;
        }

        public override string ContextType => "SelectedLines";

        public override string Description => "Selected Log Lines";

        public override bool HasContext
        {
            get
            {
                string text = _getSelectedText();
                return !string.IsNullOrWhiteSpace(text);
            }
        }

        public override Task<string> GetContextAsync()
        {
            if (!HasContext)
                return Task.FromResult(string.Empty);

            string selected = _getSelectedText();

            // DEF-AI01: redact here, at the source, rather than trusting every eventual
            // caller to redact the assembled prompt downstream.
            if (_redactSensitiveData && _redactor != null)
                selected = _redactor.Redact(selected);

            return Task.FromResult($"```\n{selected}\n```\n");
        }

        public override Task<string> GetSummaryAsync()
        {
            if (!HasContext)
                return Task.FromResult("No selection");

            string text = _getSelectedText();
            int lineCount = text.Split(new[] { '\n' }, StringSplitOptions.None).Length;

            return Task.FromResult($"{lineCount} selected line(s), {text.Length} characters");
        }
    }
}
