using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Manages application theming (Light/Dark mode).
    /// Applies consistent colors across all forms and controls.
    /// </summary>
    public static class ThemeManager
    {
        public enum Theme
        {
            Light,
            Dark
        }

        // ?? Light Theme Colors ????????????????????????????????????????????????
        private static readonly Color LightBackground = SystemColors.Window;
        private static readonly Color LightForeground = SystemColors.WindowText;
        private static readonly Color LightControlBackground = SystemColors.Control;
        private static readonly Color LightControlForeground = SystemColors.ControlText;
        private static readonly Color LightMenuBackground = SystemColors.Menu;
        private static readonly Color LightMenuForeground = SystemColors.MenuText;
        private static readonly Color LightHighlight = SystemColors.Highlight;
        private static readonly Color LightHighlightText = SystemColors.HighlightText;
        private static readonly Color LightErrorBackground = Color.FromArgb(255, 220, 220);
        private static readonly Color LightWarningBackground = Color.FromArgb(255, 243, 205);

        // ?? Dark Theme Colors - Professional VS Code/Visual Studio Style ??????
        private static readonly Color DarkBackground = Color.FromArgb(30, 30, 30);           // Main editor background
        private static readonly Color DarkForeground = Color.FromArgb(212, 212, 212);        // Main text
        private static readonly Color DarkControlBackground = Color.FromArgb(37, 37, 38);    // Control backgrounds
        private static readonly Color DarkControlForeground = Color.FromArgb(241, 241, 241); // Control text
        private static readonly Color DarkMenuBackground = Color.FromArgb(45, 45, 48);       // Menu/toolbar background
        private static readonly Color DarkMenuForeground = Color.FromArgb(241, 241, 241);    // Menu text
        private static readonly Color DarkHighlight = Color.FromArgb(0, 122, 204);           // Selection/highlight blue
        private static readonly Color DarkHighlightText = Color.White;                       // Selected text
        private static readonly Color DarkErrorBackground = Color.FromArgb(80, 30, 30);      // Error row background
        private static readonly Color DarkWarningBackground = Color.FromArgb(80, 70, 30);    // Warning row background
        private static readonly Color DarkBorder = Color.FromArgb(63, 63, 70);               // Border color
        private static readonly Color DarkInputBackground = Color.FromArgb(51, 51, 51);      // TextBox/ComboBox background
        private static readonly Color DarkButtonBackground = Color.FromArgb(62, 62, 64);     // Button background
        private static readonly Color DarkButtonHover = Color.FromArgb(82, 82, 84);          // Button hover
        private static readonly Color DarkSplitter = Color.FromArgb(45, 45, 48);             // Splitter color

        // ?? Current Theme ?????????????????????????????????????????????????????
        private static Theme _currentTheme = Theme.Light;

        public static Theme CurrentTheme => _currentTheme;

        public static void SetTheme(Theme theme)
        {
            _currentTheme = theme;
            DisposeTreeGdiObjects(); // invalidate cached brushes/pens so colours rebuild for new theme
        }

        // ?? Color Accessors ???????????????????????????????????????????????????
        public static Color BackgroundColor => _currentTheme == Theme.Dark ? DarkBackground : LightBackground;
        public static Color ForegroundColor => _currentTheme == Theme.Dark ? DarkForeground : LightForeground;
        public static Color ControlBackgroundColor => _currentTheme == Theme.Dark ? DarkControlBackground : LightControlBackground;
        public static Color ControlForegroundColor => _currentTheme == Theme.Dark ? DarkControlForeground : LightControlForeground;
        public static Color MenuBackgroundColor => _currentTheme == Theme.Dark ? DarkMenuBackground : LightMenuBackground;
        public static Color MenuForegroundColor => _currentTheme == Theme.Dark ? DarkMenuForeground : LightMenuForeground;
        public static Color HighlightColor => _currentTheme == Theme.Dark ? DarkHighlight : LightHighlight;
        public static Color HighlightTextColor => _currentTheme == Theme.Dark ? DarkHighlightText : LightHighlightText;
        public static Color ErrorBackgroundColor => _currentTheme == Theme.Dark ? DarkErrorBackground : LightErrorBackground;
        public static Color WarningBackgroundColor => _currentTheme == Theme.Dark ? DarkWarningBackground : LightWarningBackground;
        public static Color BorderColor => _currentTheme == Theme.Dark ? DarkBorder : SystemColors.ControlDark;
        public static Color InputBackgroundColor => _currentTheme == Theme.Dark ? DarkInputBackground : SystemColors.Window;
        public static Color ButtonBackgroundColor => _currentTheme == Theme.Dark ? DarkButtonBackground : SystemColors.Control;
        public static Color ButtonHoverColor => _currentTheme == Theme.Dark ? DarkButtonHover : SystemColors.ControlLight;
        public static Color SplitterColor => _currentTheme == Theme.Dark ? DarkSplitter : SystemColors.Control;

        // ?? Apply Theme ???????????????????????????????????????????????????????
        public static void ApplyTheme(Form form)
        {
            if (form == null) return;

            form.BackColor = BackgroundColor;
            form.ForeColor = ForegroundColor;

            ApplyThemeToControls(form.Controls);

            // Walk ALL controls recursively to find every MenuStrip / ToolStrip / StatusStrip
            ApplyThemeToStrips(form.Controls);

            // Apply renderer to every ContextMenuStrip attached to controls on this form
            ApplyThemeToContextMenus(form);
        }

        /// <summary>
        /// Walks every control and applies the dark/light renderer to any ContextMenuStrip
        /// assigned to a ContextMenuStrip property (ContextMenu is in components, not Controls).
        /// </summary>
        private static void ApplyThemeToContextMenus(Form form)
        {
            // Collect all ContextMenuStrips via the component container
            if (form.Container == null) return;
            foreach (System.ComponentModel.IComponent comp in form.Container.Components)
            {
                if (comp is ContextMenuStrip cms)
                    ApplyThemeToToolStrip(cms);
            }
        }

        private static void ApplyThemeToStrips(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is MenuStrip menuStrip)
                    ApplyThemeToMenuStrip(menuStrip);
                else if (control is StatusStrip statusStrip)
                    ApplyThemeToStatusStrip(statusStrip);
                else if (control is ToolStrip toolStrip)
                    ApplyThemeToToolStrip(toolStrip);

                if (control.Controls.Count > 0)
                    ApplyThemeToStrips(control.Controls);
            }
        }

        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // Skip certain control types that manage their own colors
                if (control is Button button)
                {
                    button.ForeColor = ControlForegroundColor;
                    button.BackColor = ButtonBackgroundColor;
                    button.FlatStyle = _currentTheme == Theme.Dark ? FlatStyle.Flat : FlatStyle.Standard;
                    if (_currentTheme == Theme.Dark)
                    {
                        button.FlatAppearance.BorderColor = BorderColor;
                        button.FlatAppearance.MouseOverBackColor = ButtonHoverColor;
                    }
                }
                else if (control is CheckBox || control is RadioButton)
                {
                    control.ForeColor = ControlForegroundColor;
                }
                else if (control is TextBox textBox)
                {
                    textBox.BackColor = InputBackgroundColor;
                    textBox.ForeColor = ForegroundColor;
                    textBox.BorderStyle = _currentTheme == Theme.Dark ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                }
                else if (control is RichTextBox richTextBox)
                {
                    richTextBox.BackColor = BackgroundColor;
                    richTextBox.ForeColor = ForegroundColor;
                    richTextBox.BorderStyle = _currentTheme == Theme.Dark ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                }
                else if (control is ListView listView)
                {
                    listView.BackColor = BackgroundColor;
                    listView.ForeColor = ForegroundColor;
                    listView.BorderStyle = _currentTheme == Theme.Dark ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                }
                else if (control is TreeView treeView)
                {
                    treeView.BackColor = BackgroundColor;
                    treeView.ForeColor = ForegroundColor;
                    treeView.LineColor = BorderColor;
                    treeView.BorderStyle = _currentTheme == Theme.Dark
                        ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;

                    if (_currentTheme == Theme.Dark)
                    {
                        // OwnerDrawText: Windows draws the full tree structure (indent lines,
                        // +/- glyphs, icons) using TreeView.BackColor (already dark).
                        // We only override the text-label area for selection + text colour.
                        treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
                        treeView.DrawNode -= TreeView_DrawNode;
                        treeView.DrawNode += TreeView_DrawNode;
                    }
                    else
                    {
                        // Back to Normal so light mode uses the built-in Windows rendering.
                        treeView.DrawMode = TreeViewDrawMode.Normal;
                        treeView.DrawNode -= TreeView_DrawNode;
                    }
                }
                else if (control is TabControl tabControl)
                {
                    // For dark theme, use DrawMode to custom draw tabs
                    if (_currentTheme == Theme.Dark)
                    {
                        tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                            // Remove any existing handler to avoid duplicates
                            tabControl.DrawItem -= TabControl_DrawItem;
                            tabControl.DrawItem += TabControl_DrawItem;
                            // OwnerDrawFixed forces all tabs to ItemSize.Width.
                            // Measure the widest tab text + 16px icon + padding so nothing gets clipped.
                            int maxW = 80; // minimum width
                            using (var g = tabControl.CreateGraphics())
                            {
                                foreach (TabPage tp in tabControl.TabPages)
                                {
                                    int w = (int)g.MeasureString(tp.Text, tabControl.Font).Width + 16 + 16; // icon + gaps
                                    if (w > maxW) maxW = w;
                                }
                            }
                            tabControl.ItemSize = new Size(maxW, 26);
                    }
                    else
                    {
                        tabControl.DrawMode = TabDrawMode.Normal;
                        tabControl.DrawItem -= TabControl_DrawItem;
                    }

                    // Ensure single-row scrollable tabs regardless of theme or DrawMode
                    tabControl.Multiline = false;
                    tabControl.SizeMode  = TabSizeMode.Normal;

                    tabControl.BackColor = ControlBackgroundColor;
                    tabControl.ForeColor = ControlForegroundColor;

                    // Apply to tab pages
                    foreach (TabPage tabPage in tabControl.TabPages)
                    {
                        tabPage.BackColor = BackgroundColor;
                        tabPage.ForeColor = ControlForegroundColor;
                        ApplyThemeToControls(tabPage.Controls);
                    }
                }
                else if (control is SplitContainer splitContainer)
                {
                    splitContainer.BackColor = SplitterColor;
                    splitContainer.Panel1.BackColor = BackgroundColor;
                    splitContainer.Panel2.BackColor = BackgroundColor;
                    ApplyThemeToControls(splitContainer.Panel1.Controls);
                    ApplyThemeToControls(splitContainer.Panel2.Controls);
                }
                else if (control is GroupBox groupBox)
                {
                    groupBox.ForeColor = ControlForegroundColor;
                }
                else if (control is Label label)
                {
                    label.ForeColor = ControlForegroundColor;
                }
                else if (control is Panel panel)
                {
                    // Don't override panel background if it's used for color preview or special panels
                    if (panel.Name != "panelColorPreview" && panel.GetType().Name != "CallGraphPanel")
                    {
                        panel.BackColor = BackgroundColor;
                    }
                    panel.ForeColor = ControlForegroundColor;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.BackColor = InputBackgroundColor;
                    comboBox.ForeColor = ForegroundColor;
                    comboBox.FlatStyle = _currentTheme == Theme.Dark ? FlatStyle.Flat : FlatStyle.Standard;
                }
                else if (control is NumericUpDown numericUpDown)
                {
                    numericUpDown.BackColor = InputBackgroundColor;
                    numericUpDown.ForeColor = ForegroundColor;
                    numericUpDown.BorderStyle = _currentTheme == Theme.Dark ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                }
                else if (control is DateTimePicker dateTimePicker)
                {
                    dateTimePicker.BackColor = InputBackgroundColor;
                    dateTimePicker.ForeColor = ForegroundColor;
                }
                else
                {
                    control.BackColor = BackgroundColor;
                    control.ForeColor = ForegroundColor;
                }

                // Recursively apply to child controls
                // Skip custom visualization panels that handle their own theming
                string typeName = control.GetType().Name;
                if (control.Controls.Count > 0 && 
                    typeName != "CallGraphPanel" && 
                    typeName != "FlameGraphPanel" && 
                    typeName != "TimelinePanel")
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }

        private static void ApplyThemeToMenuStrip(MenuStrip menuStrip)
        {
            menuStrip.BackColor = MenuBackgroundColor;
            menuStrip.ForeColor = MenuForegroundColor;

            if (_currentTheme == Theme.Dark)
            {
                menuStrip.Renderer = new DarkMenuStripRenderer();

                // Apply text color to all menu items
                foreach (ToolStripMenuItem item in menuStrip.Items)
                {
                    ApplyThemeToMenuItem(item);
                }
            }
            else
            {
                menuStrip.Renderer = new ToolStripProfessionalRenderer();

                // Reset to default colors
                foreach (ToolStripMenuItem item in menuStrip.Items)
                {
                    ResetMenuItemColors(item);
                }
            }
        }

        private static void ApplyThemeToMenuItem(ToolStripMenuItem menuItem)
        {
            if (menuItem == null) return;

            menuItem.ForeColor = MenuForegroundColor;

            // Recursively apply to dropdown items
            foreach (ToolStripItem item in menuItem.DropDownItems)
            {
                item.ForeColor = MenuForegroundColor;

                if (item is ToolStripMenuItem subMenuItem)
                {
                    ApplyThemeToMenuItem(subMenuItem);
                }
            }
        }

        private static void ResetMenuItemColors(ToolStripMenuItem menuItem)
        {
            if (menuItem == null) return;

            menuItem.ForeColor = SystemColors.MenuText;

            // Recursively reset dropdown items
            foreach (ToolStripItem item in menuItem.DropDownItems)
            {
                item.ForeColor = SystemColors.MenuText;

                if (item is ToolStripMenuItem subMenuItem)
                {
                    ResetMenuItemColors(subMenuItem);
                }
            }
        }

        private static void ApplyThemeToToolStrip(ToolStrip toolStrip)
        {
            toolStrip.BackColor = MenuBackgroundColor;
            toolStrip.ForeColor = MenuForegroundColor;

            if (_currentTheme == Theme.Dark)
            {
                toolStrip.Renderer = new DarkToolStripRenderer();

                // Apply text color to all toolbar items
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    item.ForeColor = MenuForegroundColor;
                }
            }
            else
            {
                toolStrip.Renderer = new ToolStripProfessionalRenderer();

                // Reset to default colors
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    item.ForeColor = SystemColors.ControlText;
                }
            }
        }

        private static void ApplyThemeToStatusStrip(StatusStrip statusStrip)
        {
            statusStrip.BackColor = MenuBackgroundColor;
            statusStrip.ForeColor = MenuForegroundColor;

            if (_currentTheme == Theme.Dark)
            {
                statusStrip.Renderer = new DarkToolStripRenderer();

                // Apply text color to all status strip items
                foreach (ToolStripItem item in statusStrip.Items)
                {
                    item.ForeColor = MenuForegroundColor;
                }
            }
            else
            {
                statusStrip.Renderer = new ToolStripProfessionalRenderer();

                // Reset to default colors
                foreach (ToolStripItem item in statusStrip.Items)
                {
                    item.ForeColor = SystemColors.ControlText;
                }
            }
        }

        // ?? Custom Renderers for Dark Theme ???????????????????????????????????
        private class DarkMenuStripRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuStripRenderer() : base(new DarkColorTable()) { }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                // Force light text color for dark theme - professional VS style
                e.TextColor = Color.FromArgb(241, 241, 241);
                base.OnRenderItemText(e);
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                // Draw custom checkmark for dark theme visibility
                var g = e.Graphics;
                var rect = new Rectangle(e.ImageRectangle.X, e.ImageRectangle.Y, 
                                        e.ImageRectangle.Width, e.ImageRectangle.Height);

                // Draw checkmark background
                using (var brush = new SolidBrush(Color.FromArgb(0, 122, 204)))
                {
                    g.FillRectangle(brush, rect);
                }

                // Draw checkmark symbol
                using (var pen = new Pen(Color.White, 2))
                {
                    // Draw a checkmark path
                    int x = rect.X + rect.Width / 2 - 3;
                    int y = rect.Y + rect.Height / 2;

                    g.DrawLine(pen, x, y, x + 2, y + 3);
                    g.DrawLine(pen, x + 2, y + 3, x + 6, y - 3);
                }
            }
        }

        private class DarkToolStripRenderer : ToolStripProfessionalRenderer
        {
            public DarkToolStripRenderer() : base(new DarkColorTable()) { }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                // Force light text color for dark theme - professional VS style
                e.TextColor = Color.FromArgb(241, 241, 241);
                base.OnRenderItemText(e);
            }
        }

        // ?? Custom Tab Drawing for Dark Theme - Professional VS Code Style ????
        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl == null) return;

            Graphics g = e.Graphics;
            TabPage tabPage = tabControl.TabPages[e.Index];
            Rectangle tabBounds = tabControl.GetTabRect(e.Index);

            // Colors for professional dark theme tabs (VS Code style)
            Color tabBackColor = (e.State == DrawItemState.Selected) 
                ? Color.FromArgb(30, 30, 30)   // Selected tab - matches editor background
                : Color.FromArgb(45, 45, 48);  // Unselected tab

            Color tabForeColor = (e.State == DrawItemState.Selected)
                ? Color.FromArgb(241, 241, 241)  // Selected text - bright
                : Color.FromArgb(170, 170, 170); // Unselected text - dimmed

            Color tabBorderColor = (e.State == DrawItemState.Selected)
                ? Color.FromArgb(0, 122, 204)    // Selected border - blue accent
                : Color.FromArgb(63, 63, 70);    // Unselected border

            // Draw tab background
            using (SolidBrush brush = new SolidBrush(tabBackColor))
            {
                g.FillRectangle(brush, tabBounds);
            }

            // Draw top border for selected tab (accent color)
            if (e.State == DrawItemState.Selected)
            {
                using (Pen accentPen = new Pen(tabBorderColor, 2))
                {
                    g.DrawLine(accentPen, tabBounds.Left, tabBounds.Top, tabBounds.Right, tabBounds.Top);
                }
            }

            // Draw subtle border
            using (Pen borderPen = new Pen(Color.FromArgb(63, 63, 70)))
            {
                g.DrawRectangle(borderPen, tabBounds.X, tabBounds.Y, tabBounds.Width - 1, tabBounds.Height - 1);
            }

            // Center the icon + text block as a unit within the tab
            using (SolidBrush textBrush = new SolidBrush(tabForeColor))
            {
                const int iconSize = 16;
                const int iconGap  = 4;  // pixels between icon and text

                bool hasIcon = tabControl.ImageList != null
                    && !string.IsNullOrEmpty(tabPage.ImageKey)
                    && tabControl.ImageList.Images.ContainsKey(tabPage.ImageKey);

                SizeF textSize = g.MeasureString(tabPage.Text, tabControl.Font);

                // Total content width: icon (optional) + gap + text
                float contentW = hasIcon ? iconSize + iconGap + textSize.Width : textSize.Width;
                float startX   = tabBounds.X + (tabBounds.Width - contentW) / 2f;
                float midY     = tabBounds.Y + tabBounds.Height / 2f;

                if (hasIcon)
                {
                    Image img = tabControl.ImageList.Images[tabPage.ImageKey];
                    g.DrawImage(img,
                        (int)startX,
                        (int)(midY - iconSize / 2f),
                        iconSize, iconSize);
                    startX += iconSize + iconGap;
                }

                var sf = new StringFormat
                {
                    Alignment     = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags   = StringFormatFlags.NoWrap
                };
                g.DrawString(tabPage.Text, tabControl.Font, textBrush,
                    new RectangleF(startX, tabBounds.Y, textSize.Width + 2, tabBounds.Height), sf);
            }
        }

        // ?? Dark-mode TreeView owner-draw ?????????????????????????????????????
        // ?? Cached GDI objects for dark-mode TreeView drawing ?????????????????
        // Allocated once, reused on every DrawNode call, disposed on theme change.
        private static SolidBrush _treeBackBrush;
        private static SolidBrush _treeSelBrush;
        private static SolidBrush _treeHotBrush;
        private static SolidBrush _treeTextBrush;
        private static SolidBrush _treeSelTextBrush;
        private static Pen        _treeGlyphPen;
        private static StringFormat _treeSf;

        private static void EnsureTreeGdiObjects()
        {
            if (_treeBackBrush != null) return;   // already built
            _treeBackBrush    = new SolidBrush(DarkBackground);
            _treeSelBrush     = new SolidBrush(Color.FromArgb(0, 122, 204));
            _treeHotBrush     = new SolidBrush(Color.FromArgb(62, 62, 64));
            _treeTextBrush    = new SolidBrush(DarkForeground);
            _treeSelTextBrush = new SolidBrush(Color.White);
            _treeGlyphPen     = new Pen(Color.FromArgb(160, 160, 160));
            _treeSf = new StringFormat
            {
                Alignment     = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                FormatFlags   = StringFormatFlags.NoWrap | StringFormatFlags.NoClip
            };
        }

        private static void DisposeTreeGdiObjects()
        {
            _treeBackBrush?.Dispose();    _treeBackBrush    = null;
            _treeSelBrush?.Dispose();     _treeSelBrush     = null;
            _treeHotBrush?.Dispose();     _treeHotBrush     = null;
            _treeTextBrush?.Dispose();    _treeTextBrush    = null;
            _treeSelTextBrush?.Dispose(); _treeSelTextBrush = null;
            _treeGlyphPen?.Dispose();     _treeGlyphPen     = null;
            _treeSf?.Dispose();           _treeSf           = null;
        }

        // In OwnerDrawText mode Windows paints tree structure (indent, lines, glyphs, icons)
        // using TreeView.BackColor (already DarkBackground). We only override the label area.
        private static void TreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var tv = (TreeView)sender;
            if (e.Bounds.Width <= 0 || e.Bounds.Height <= 0) return;
            EnsureTreeGdiObjects();
            bool selected = (e.State & TreeNodeStates.Selected) != 0 || (e.State & TreeNodeStates.Focused) != 0;
            bool hot = (e.State & TreeNodeStates.Hot) != 0;
            SolidBrush bg = selected ? _treeSelBrush : hot ? _treeHotBrush : _treeBackBrush;
            e.Graphics.FillRectangle(bg, e.Bounds);
            bool hasNodeColor = e.Node.ForeColor != Color.Empty && e.Node.ForeColor != tv.ForeColor;
            SolidBrush textBrush; SolidBrush tempBrush = null;
            if (selected) textBrush = _treeSelTextBrush;
            else if (hasNodeColor) textBrush = tempBrush = new SolidBrush(e.Node.ForeColor);
            else textBrush = _treeTextBrush;
            e.Graphics.DrawString(e.Node.Text, tv.Font, textBrush, e.Bounds, _treeSf);
            tempBrush?.Dispose();
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            // Professional VS Code/Visual Studio Dark Theme Colors
            public override Color MenuItemSelected => Color.FromArgb(51, 51, 52);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(51, 51, 52);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(51, 51, 52);
            public override Color MenuItemBorder => Color.FromArgb(63, 63, 70);
            public override Color MenuBorder => Color.FromArgb(63, 63, 70);
            public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 48);
            public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 48);
            public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(0, 122, 204);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(0, 122, 204);
            public override Color CheckBackground => Color.FromArgb(51, 51, 52);
            public override Color CheckSelectedBackground => Color.FromArgb(51, 51, 52);
            public override Color CheckPressedBackground => Color.FromArgb(51, 51, 52);
            public override Color ButtonSelectedBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonSelectedHighlight => Color.FromArgb(51, 51, 52);
            public override Color ButtonSelectedHighlightBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedHighlight => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedHighlightBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonCheckedHighlight => Color.FromArgb(51, 51, 52);
            public override Color ButtonCheckedHighlightBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedGradientBegin => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedGradientEnd => Color.FromArgb(0, 122, 204);
            public override Color ButtonSelectedGradientBegin => Color.FromArgb(62, 62, 64);
            public override Color ButtonSelectedGradientEnd => Color.FromArgb(62, 62, 64);
            public override Color ButtonCheckedGradientBegin => Color.FromArgb(62, 62, 64);
            public override Color ButtonCheckedGradientEnd => Color.FromArgb(62, 62, 64);
            public override Color ToolStripBorder => Color.FromArgb(63, 63, 70);
            public override Color SeparatorDark => Color.FromArgb(63, 63, 70);
            public override Color SeparatorLight => Color.FromArgb(45, 45, 48);

            // Menu and toolbar backgrounds
            public override Color MenuStripGradientBegin => Color.FromArgb(45, 45, 48);
            public override Color MenuStripGradientEnd => Color.FromArgb(45, 45, 48);
            public override Color ToolStripGradientBegin => Color.FromArgb(45, 45, 48);
            public override Color ToolStripGradientMiddle => Color.FromArgb(45, 45, 48);
            public override Color ToolStripGradientEnd => Color.FromArgb(45, 45, 48);
            public override Color StatusStripGradientBegin => Color.FromArgb(45, 45, 48);
            public override Color StatusStripGradientEnd => Color.FromArgb(45, 45, 48);

            // Additional professional touches
            public override Color ToolStripContentPanelGradientBegin => Color.FromArgb(30, 30, 30);
            public override Color ToolStripContentPanelGradientEnd => Color.FromArgb(30, 30, 30);
            public override Color ToolStripPanelGradientBegin => Color.FromArgb(45, 45, 48);
            public override Color ToolStripPanelGradientEnd => Color.FromArgb(45, 45, 48);
        }
    }
}
