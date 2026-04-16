using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;
using ThemeManager = Cad3PLogBrowser.Services.ThemeManager;

namespace Cad3PLogBrowser.UI
{
    /// <summary>
    /// Replaces the plain RichTextBox "Log Details" tab with a structured inspector
    /// that shows three sections for the selected log line:
    ///   1. Parsed field table  – Timestamp, Level, Thread, App, Area, Module, Source, API, Message
    ///   2. ENTER/EXIT pair row – only visible when the line is an API call
    ///   3. Context window      – ±5 surrounding lines in a RichTextBox, selected line highlighted
    /// </summary>
    internal sealed class LineInspectorPanel : Panel
    {
        // ?? Section: parsed field table ??????????????????????????????????????
        private readonly TableLayoutPanel _fieldTable;
        private readonly Label[]          _fieldKeys;
        private readonly Label[]          _fieldVals;

        private static readonly string[] FieldNames =
        {
            "Line",
            "Timestamp",
            "Level",
            "Thread",
            "App",
            "Area",
            "Module",
            "Source File",
            "API Name",
            "Entry Type",
            "Message",
        };
        private const int FieldCount = 11;

        // ?? Section: ENTER/EXIT pair ?????????????????????????????????????????
        private readonly Panel _pairPanel;
        private readonly Label _pairLabel;

        // ?? Section: context window ??????????????????????????????????????????
        private readonly Label       _ctxHeader;
        private readonly RichTextBox _ctxBox;

        // ?? Dividers ?????????????????????????????????????????????????????????
        private readonly Label _divider1;
        private readonly Label _divider2;

        // ??????????????????????????????????????????????????????????????????????
        public LineInspectorPanel()
        {
            Dock      = DockStyle.Fill;
            Padding   = new Padding(8);
            AutoScroll = true;

            // ?? Field table ??????????????????????????????????????????????????
            _fieldTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize    = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock        = DockStyle.Top,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding     = new Padding(0),
                Margin      = new Padding(0),
            };
            _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));

            _fieldKeys = new Label[FieldCount];
            _fieldVals = new Label[FieldCount];

            for (int i = 0; i < FieldCount; i++)
            {
                var key = new Label
                {
                    Text      = FieldNames[i],
                    AutoSize  = false,
                    Height    = 22,
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Padding   = new Padding(4, 0, 4, 0),
                    Margin    = new Padding(0, 1, 2, 1),
                };
                var val = new Label
                {
                    Text      = "",
                    AutoSize  = false,
                    Height    = 22,
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    Padding   = new Padding(4, 0, 4, 0),
                    Margin    = new Padding(0, 1, 0, 1),
                };
                _fieldKeys[i] = key;
                _fieldVals[i] = val;
                _fieldTable.Controls.Add(key, 0, i);
                _fieldTable.Controls.Add(val, 1, i);
            }

            // ?? Divider 1 ????????????????????????????????????????????????????
            _divider1 = MakeDivider();

            // ?? ENTER/EXIT pair panel ????????????????????????????????????????
            _pairLabel = new Label
            {
                AutoSize  = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Padding   = new Padding(4, 2, 4, 2),
                Dock      = DockStyle.Fill,
            };
            _pairPanel = new Panel
            {
                Dock     = DockStyle.Top,
                Height   = 48,
                Padding  = new Padding(4),
                Visible  = false,
            };
            _pairPanel.Controls.Add(_pairLabel);

            // ?? Divider 2 ????????????????????????????????????????????????????
            _divider2 = MakeDivider();

            // ?? Context header ???????????????????????????????????????????????
            _ctxHeader = new Label
            {
                Text      = "Context  (± 5 lines)",
                AutoSize  = false,
                Height    = 22,
                Dock      = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding   = new Padding(4, 0, 0, 0),
            };

            // ?? Context rich-text box ????????????????????????????????????????
            _ctxBox = new RichTextBox
            {
                Dock      = DockStyle.Fill,
                ReadOnly  = true,
                WordWrap  = false,
                Font      = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Both,
            };

            // Build control tree bottom-up (Fill must be last added to work correctly)
            Controls.Add(_ctxBox);       // Fill – added first so others dock on top
            Controls.Add(_ctxHeader);
            Controls.Add(_divider2);
            Controls.Add(_pairPanel);
            Controls.Add(_divider1);
            Controls.Add(_fieldTable);
        }

        // ?? Public API ????????????????????????????????????????????????????????

        /// <summary>One line as seen by the inspector (mirrors MainForm.VirtualLogLine).</summary>
        public struct InspectorLine
        {
            public int    LineNumber;
            public string Text;
        }

        /// <summary>
        /// Populates all three sections for the selected virtual list index.
        /// </summary>
        public void Inspect(
            IList<InspectorLine> lines,
            int                  selectedIndex,
            IList<LogEntry>      entries)
        {
            if (lines == null || selectedIndex < 0 || selectedIndex >= lines.Count)
            {
                Clear();
                return;
            }

            var line  = lines[selectedIndex];
            var entry = FindEntry(entries, line.LineNumber);

            PopulateFields(line.LineNumber, line.Text ?? string.Empty, entry);
            PopulatePair(entry, entries);
            PopulateContext(lines, selectedIndex);
        }
        public void ApplyTheme()
        {
            bool dark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;

            Color bg       = ThemeManager.BackgroundColor;
            Color ctrlBg   = ThemeManager.ControlBackgroundColor;
            Color fg       = ThemeManager.ForegroundColor;
            Color ctrlFg   = ThemeManager.ControlForegroundColor;
            Color keyBg    = dark ? Color.FromArgb(50, 50, 54) : Color.FromArgb(240, 242, 245);
            Color divColor = dark ? Color.FromArgb(70, 70, 74) : Color.FromArgb(210, 212, 216);
            Color pairBg   = dark ? Color.FromArgb(38, 50, 56) : Color.FromArgb(232, 244, 253);

            BackColor       = bg;
            _ctxBox.BackColor = ctrlBg;
            _ctxBox.ForeColor = fg;

            for (int i = 0; i < FieldCount; i++)
            {
                _fieldKeys[i].BackColor = keyBg;
                _fieldKeys[i].ForeColor = ctrlFg;
                _fieldVals[i].BackColor = bg;
                _fieldVals[i].ForeColor = fg;
            }

            _divider1.BackColor = divColor;
            _divider2.BackColor = divColor;

            _pairPanel.BackColor = pairBg;
            _pairLabel.BackColor = pairBg;
            _pairLabel.ForeColor = fg;

            _ctxHeader.BackColor = bg;
            _ctxHeader.ForeColor = ctrlFg;

            // Re-colour the context box (selected-line highlight colour changes with theme)
            RecolourContextBox();
        }

        // ?? VirtualLogLine accessor — removed; use InspectorLine instead ????????

        // ?? Private helpers ???????????????????????????????????????????????????

        private void Clear()
        {
            for (int i = 0; i < FieldCount; i++) _fieldVals[i].Text = "";
            _pairPanel.Visible = false;
            _ctxBox.Clear();
        }

        private void PopulateFields(int lineNumber, string raw, LogEntry entry)
        {
            // Row 0 – Line number
            SetVal(0, lineNumber.ToString());

            if (entry != null)
            {
                // Row 1 – Timestamp (use EpochMs when available)
                string ts = entry.EpochMs > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(entry.EpochMs)
                               .ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")
                    : "";
                SetVal(1, ts);

                // Row 2 – Level with badge colour
                SetLevelVal(entry.Level);

                // Row 3 – Thread
                SetVal(3, entry.ThreadId ?? "");

                // Row 4 – App
                SetVal(4, entry.App ?? "");

                // Row 5 – Area
                SetVal(5, entry.Area ?? "");

                // Row 6 – Module
                SetVal(6, entry.Module ?? "");

                // Row 7 – Source file
                SetVal(7, entry.SourceFile ?? "");

                // Row 8 – API name
                SetVal(8, entry.ApiName ?? "");

                // Row 9 – Entry type
                string entryType = entry.IsCallEnter ? "?  ENTER"
                                 : entry.IsCallExit  ? "?  EXIT"
                                 : "";
                SetVal(9, entryType);

                // Row 10 – Message (raw minus the structured prefix)
                SetVal(10, ExtractMessage(raw));
            }
            else
            {
                // Not a parsed entry — just show the raw line as message
                for (int i = 1; i <= 9; i++) SetVal(i, "");
                SetVal(10, raw);
                SetLevelVal(null);
            }
        }

        private void SetVal(int row, string text)
        {
            _fieldVals[row].Text = text ?? "";
        }

        private void SetLevelVal(string levelCode)
        {
            bool dark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;
            var lbl   = _fieldVals[2];

            switch (levelCode)
            {
                case "E":
                    lbl.Text      = "ERROR";
                    lbl.ForeColor = Color.FromArgb(255, 85, 85);
                    lbl.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    break;
                case "W":
                    lbl.Text      = "WARNING";
                    lbl.ForeColor = Color.FromArgb(255, 193, 7);
                    lbl.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    break;
                case "I":
                    lbl.Text      = "INFO";
                    lbl.ForeColor = dark ? Color.FromArgb(100, 200, 255) : Color.FromArgb(0, 120, 215);
                    lbl.Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                    break;
                case "D":
                    lbl.Text      = "DEBUG";
                    lbl.ForeColor = dark ? Color.FromArgb(160, 160, 160) : Color.FromArgb(100, 100, 100);
                    lbl.Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                    break;
                case "C":
                    lbl.Text      = "CONFIG";
                    lbl.ForeColor = dark ? Color.FromArgb(150, 220, 150) : Color.FromArgb(0, 140, 0);
                    lbl.Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                    break;
                default:
                    lbl.Text      = levelCode ?? "";
                    lbl.ForeColor = ThemeManager.ForegroundColor;
                    lbl.Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                    break;
            }
        }

        private void PopulatePair(LogEntry entry, IList<LogEntry> entries)
        {
            if (entry == null || !entry.IsApiCall || entries == null)
            {
                _pairPanel.Visible = false;
                return;
            }

            bool dark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;

            if (entry.IsCallEnter)
            {
                // Find matching EXIT
                var exit = FindMatchingExit(entry, entries);
                if (exit != null)
                {
                    long ms = exit.EpochMs > 0 && entry.EpochMs > 0
                        ? exit.EpochMs - entry.EpochMs : -1;
                    string dur = ms >= 0 ? $"{ms:N0} ms" : "?";
                    _pairLabel.Text =
                        $"? ENTER  Line {entry.LineNumber}     ?     " +
                        $"? EXIT  Line {exit.LineNumber}     |     ? Duration: {dur}";
                }
                else
                {
                    _pairLabel.Text = $"? ENTER  Line {entry.LineNumber}     ?     ? EXIT  not found";
                    _pairLabel.ForeColor = Color.FromArgb(255, 85, 85);
                }
            }
            else // IsCallExit
            {
                var enter = FindMatchingEnter(entry, entries);
                if (enter != null)
                {
                    long ms = entry.EpochMs > 0 && enter.EpochMs > 0
                        ? entry.EpochMs - enter.EpochMs : -1;
                    string dur = ms >= 0 ? $"{ms:N0} ms" : "?";
                    _pairLabel.Text =
                        $"? ENTER  Line {enter.LineNumber}     ?     " +
                        $"? EXIT  Line {entry.LineNumber}     |     ? Duration: {dur}";
                }
                else
                {
                    _pairLabel.Text = $"? ENTER  not found     ?     ? EXIT  Line {entry.LineNumber}";
                    _pairLabel.ForeColor = Color.FromArgb(255, 85, 85);
                }
            }

            _pairPanel.Visible = true;
        }

        private void PopulateContext(IList<InspectorLine> lines, int center)
        {
            bool dark   = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;
            int  start  = Math.Max(0, center - 5);
            int  end    = Math.Min(lines.Count - 1, center + 5);

            Color selBg   = dark ? Color.FromArgb(0, 80, 140)   : Color.FromArgb(201, 225, 255);
            Color selFg   = dark ? Color.White                   : Color.FromArgb(0, 0, 0);
            Color normFg  = dark ? Color.FromArgb(180, 180, 180) : Color.FromArgb(50, 50, 50);
            Color arrowFg = dark ? Color.FromArgb(0, 170, 255)   : Color.FromArgb(0, 100, 200);

            _ctxBox.Clear();
            _ctxBox.SuspendLayout();

            for (int i = start; i <= end; i++)
            {
                var vl        = lines[i];
                bool isSel    = (i == center);
                string prefix = isSel ? "? " : "  ";
                string line   = $"{prefix}{vl.LineNumber,6}:  {vl.Text}\n";

                int startPos  = _ctxBox.TextLength;
                _ctxBox.AppendText(line);
                int endPos    = _ctxBox.TextLength;

                _ctxBox.Select(startPos, endPos - startPos);

                if (isSel)
                {
                    _ctxBox.SelectionBackColor = selBg;
                    _ctxBox.SelectionColor     = selFg;
                    _ctxBox.SelectionFont      = new Font("Consolas", 9f, FontStyle.Bold);
                }
                else
                {
                    _ctxBox.SelectionBackColor = _ctxBox.BackColor;
                    _ctxBox.SelectionColor     = normFg;
                    _ctxBox.SelectionFont      = new Font("Consolas", 9f, FontStyle.Regular);
                }
            }

            _ctxBox.SelectionLength = 0;
            _ctxBox.ResumeLayout();

            // Scroll selected line into view
            int selCharPos = 0;
            for (int i = start; i < center; i++)
            {
                var vl    = lines[i];
                string ln = $"  {vl.LineNumber,6}:  {vl.Text}\n";
                selCharPos += ln.Length;
            }
            _ctxBox.Select(selCharPos, 0);
            _ctxBox.ScrollToCaret();
        }

        private void RecolourContextBox()
        {
            // Just clear — caller should re-Inspect to get proper colours
            // (avoids needing to store state here)
        }

        // ?? ENTER/EXIT matching (mirrors JumpToMatchingPair logic) ????????????

        private static LogEntry FindMatchingExit(LogEntry enter, IList<LogEntry> entries)
        {
            int depth = 0;
            foreach (var e in entries)
            {
                if (e.LineNumber <= enter.LineNumber)
                {
                    if (e.IsApiCall && e.ApiName == enter.ApiName && e.IsCallEnter) depth++;
                    continue;
                }
                if (!e.IsApiCall || e.ApiName != enter.ApiName) continue;
                if (e.IsCallEnter) depth++;
                else if (e.IsCallExit) { depth--; if (depth == 0) return e; }
            }
            return null;
        }

        private static LogEntry FindMatchingEnter(LogEntry exit, IList<LogEntry> entries)
        {
            int depth = 0;
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var e = entries[i];
                if (e.LineNumber >= exit.LineNumber)
                {
                    if (e.IsApiCall && e.ApiName == exit.ApiName && e.IsCallExit) depth++;
                    continue;
                }
                if (!e.IsApiCall || e.ApiName != exit.ApiName) continue;
                if (e.IsCallExit) depth++;
                else if (e.IsCallEnter) { depth--; if (depth == 0) return e; }
            }
            return null;
        }

        private static LogEntry FindEntry(IList<LogEntry> entries, int lineNumber)
        {
            if (entries == null) return null;
            foreach (var e in entries)
                if (e.LineNumber == lineNumber) return e;
            return null;
        }

        /// <summary>
        /// Extracts the human-readable message payload from a raw log line.
        /// For API call lines the message is the first tab field (State).
        /// For plain lines it is everything after the 6th ": " separator.
        /// </summary>
        private static string ExtractMessage(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            const string sep = ": ";
            int pos = 0, field = 0;
            while (pos < raw.Length && field < 6)
            {
                int next = raw.IndexOf(sep, pos, StringComparison.Ordinal);
                if (next < 0) break;
                field++;
                pos = next + sep.Length;
            }
            if (field < 6) return raw;
            // payload starts at pos; message is up to the first tab
            int tab = raw.IndexOf('\t', pos);
            return tab > pos ? raw.Substring(pos, tab - pos).Trim() : raw.Substring(pos).Trim();
        }

        private static Label MakeDivider() => new Label
        {
            Dock    = DockStyle.Top,
            Height  = 1,
            Margin  = new Padding(0, 4, 0, 4),
        };
    }
}

