using System;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;

namespace Cad3PLogBrowser.AI.Context
{
    /// <summary>
    /// Provides context from user-selected lines in the log viewer.
    /// </summary>
    public class SelectedLinesContextProvider : ContextProviderBase
    {
        private readonly Func<string> _getSelectedText;

        public SelectedLinesContextProvider(
            Func<string> getSelectedText,
            ITokenEstimator tokenEstimator = null)
            : base(tokenEstimator)
        {
            _getSelectedText = getSelectedText ?? throw new ArgumentNullException(nameof(getSelectedText));
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
