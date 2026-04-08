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

        // ?? Dark Theme Colors ?????????????????????????????????????????????????
        private static readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        private static readonly Color DarkForeground = Color.FromArgb(220, 220, 220);
        private static readonly Color DarkControlBackground = Color.FromArgb(45, 45, 48);
        private static readonly Color DarkControlForeground = Color.FromArgb(220, 220, 220);
        private static readonly Color DarkMenuBackground = Color.FromArgb(37, 37, 38);
        private static readonly Color DarkMenuForeground = Color.FromArgb(220, 220, 220);
        private static readonly Color DarkHighlight = Color.FromArgb(0, 122, 204);
        private static readonly Color DarkHighlightText = Color.White;
        private static readonly Color DarkErrorBackground = Color.FromArgb(100, 30, 30);
        private static readonly Color DarkWarningBackground = Color.FromArgb(100, 90, 30);

        // ?? Current Theme ?????????????????????????????????????????????????????
        private static Theme _currentTheme = Theme.Light;

        public static Theme CurrentTheme => _currentTheme;

        public static void SetTheme(Theme theme)
        {
            _currentTheme = theme;
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

        // ?? Apply Theme ???????????????????????????????????????????????????????
        public static void ApplyTheme(Form form)
        {
            if (form == null) return;

            form.BackColor = BackgroundColor;
            form.ForeColor = ForegroundColor;

            ApplyThemeToControls(form.Controls);

            // Apply theme to MenuStrip
            foreach (Control control in form.Controls)
            {
                if (control is MenuStrip menuStrip)
                {
                    ApplyThemeToMenuStrip(menuStrip);
                }
                if (control is ToolStrip toolStrip)
                {
                    ApplyThemeToToolStrip(toolStrip);
                }
                if (control is StatusStrip statusStrip)
                {
                    ApplyThemeToStatusStrip(statusStrip);
                }
            }
        }

        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // Skip certain control types that manage their own colors
                if (control is Button || control is CheckBox || control is RadioButton)
                {
                    control.ForeColor = ControlForegroundColor;
                    control.BackColor = ControlBackgroundColor;
                }
                else if (control is TextBox textBox)
                {
                    textBox.BackColor = BackgroundColor;
                    textBox.ForeColor = ForegroundColor;
                }
                else if (control is RichTextBox richTextBox)
                {
                    richTextBox.BackColor = BackgroundColor;
                    richTextBox.ForeColor = ForegroundColor;
                }
                else if (control is ListView listView)
                {
                    listView.BackColor = BackgroundColor;
                    listView.ForeColor = ForegroundColor;
                    // Apply theme-aware border style for better dark mode appearance
                    if (_currentTheme == Theme.Dark)
                    {
                        listView.BorderStyle = BorderStyle.FixedSingle;
                    }
                }
                else if (control is TreeView treeView)
                {
                    treeView.BackColor = BackgroundColor;
                    treeView.ForeColor = ForegroundColor;
                    treeView.LineColor = ForegroundColor;
                    if (_currentTheme == Theme.Dark)
                    {
                        treeView.BorderStyle = BorderStyle.FixedSingle;
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
                    }
                    else
                    {
                        tabControl.DrawMode = TabDrawMode.Normal;
                        tabControl.DrawItem -= TabControl_DrawItem;
                    }

                    tabControl.BackColor = ControlBackgroundColor;
                    tabControl.ForeColor = ControlForegroundColor;

                    // Apply to tab pages
                    foreach (TabPage tabPage in tabControl.TabPages)
                    {
                        tabPage.BackColor = ControlBackgroundColor;
                        tabPage.ForeColor = ControlForegroundColor;
                        ApplyThemeToControls(tabPage.Controls);
                    }
                }
                else if (control is SplitContainer splitContainer)
                {
                    splitContainer.BackColor = _currentTheme == Theme.Dark ? Color.FromArgb(62, 62, 64) : Color.FromArgb(240, 240, 240);
                    splitContainer.Panel1.BackColor = BackgroundColor;
                    splitContainer.Panel2.BackColor = BackgroundColor;
                    ApplyThemeToControls(splitContainer.Panel1.Controls);
                    ApplyThemeToControls(splitContainer.Panel2.Controls);
                }
                else if (control is GroupBox groupBox)
                {
                    groupBox.ForeColor = ControlForegroundColor;
                    groupBox.BackColor = ControlBackgroundColor;
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
                    comboBox.BackColor = BackgroundColor;
                    comboBox.ForeColor = ForegroundColor;
                }
                else if (control is NumericUpDown numericUpDown)
                {
                    numericUpDown.BackColor = BackgroundColor;
                    numericUpDown.ForeColor = ForegroundColor;
                }
                else
                {
                    control.BackColor = BackgroundColor;
                    control.ForeColor = ForegroundColor;
                }

                // Recursively apply to child controls
                if (control.Controls.Count > 0 && control.GetType().Name != "CallGraphPanel")
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
                // Force white text color for dark theme
                e.TextColor = Color.FromArgb(220, 220, 220);
                base.OnRenderItemText(e);
            }
        }

        private class DarkToolStripRenderer : ToolStripProfessionalRenderer
        {
            public DarkToolStripRenderer() : base(new DarkColorTable()) { }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                // Force white text color for dark theme
                e.TextColor = Color.FromArgb(220, 220, 220);
                base.OnRenderItemText(e);
            }
        }

        // ?? Custom Tab Drawing for Dark Theme ?????????????????????????????????
        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl == null) return;

            Graphics g = e.Graphics;
            TabPage tabPage = tabControl.TabPages[e.Index];
            Rectangle tabBounds = tabControl.GetTabRect(e.Index);

            // Colors for dark theme tabs
            Color tabBackColor = (e.State == DrawItemState.Selected) 
                ? Color.FromArgb(45, 45, 48)   // Selected tab
                : Color.FromArgb(37, 37, 38);  // Unselected tab

            Color tabForeColor = (e.State == DrawItemState.Selected)
                ? Color.FromArgb(220, 220, 220)  // Selected text
                : Color.FromArgb(160, 160, 160); // Unselected text

            // Draw tab background
            using (SolidBrush brush = new SolidBrush(tabBackColor))
            {
                g.FillRectangle(brush, tabBounds);
            }

            // Draw tab border
            using (Pen borderPen = new Pen(Color.FromArgb(62, 62, 64)))
            {
                g.DrawRectangle(borderPen, tabBounds);
            }

            // Draw tab text
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            using (SolidBrush textBrush = new SolidBrush(tabForeColor))
            {
                g.DrawString(tabPage.Text, tabControl.Font, textBrush, tabBounds, stringFormat);
            }
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(62, 62, 64);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(62, 62, 64);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(62, 62, 64);
            public override Color MenuItemBorder => Color.FromArgb(62, 62, 64);
            public override Color MenuBorder => Color.FromArgb(51, 51, 55);
            public override Color ToolStripDropDownBackground => Color.FromArgb(37, 37, 38);
            public override Color ImageMarginGradientBegin => Color.FromArgb(37, 37, 38);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(37, 37, 38);
            public override Color ImageMarginGradientEnd => Color.FromArgb(37, 37, 38);
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(0, 122, 204);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(0, 122, 204);
            public override Color CheckBackground => Color.FromArgb(62, 62, 64);
            public override Color CheckSelectedBackground => Color.FromArgb(62, 62, 64);
            public override Color CheckPressedBackground => Color.FromArgb(62, 62, 64);
            public override Color ButtonSelectedBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonSelectedHighlight => Color.FromArgb(62, 62, 64);
            public override Color ButtonSelectedHighlightBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedHighlight => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedHighlightBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonCheckedHighlight => Color.FromArgb(62, 62, 64);
            public override Color ButtonCheckedHighlightBorder => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedGradientBegin => Color.FromArgb(0, 122, 204);
            public override Color ButtonPressedGradientEnd => Color.FromArgb(0, 122, 204);
            public override Color ButtonSelectedGradientBegin => Color.FromArgb(62, 62, 64);
            public override Color ButtonSelectedGradientEnd => Color.FromArgb(62, 62, 64);
            public override Color ButtonCheckedGradientBegin => Color.FromArgb(62, 62, 64);
            public override Color ButtonCheckedGradientEnd => Color.FromArgb(62, 62, 64);
            public override Color ToolStripBorder => Color.FromArgb(51, 51, 55);
            public override Color SeparatorDark => Color.FromArgb(90, 90, 90);
            public override Color SeparatorLight => Color.FromArgb(60, 60, 60);

            // Additional color overrides for text
            public override Color MenuStripGradientBegin => Color.FromArgb(37, 37, 38);
            public override Color MenuStripGradientEnd => Color.FromArgb(37, 37, 38);
            public override Color ToolStripGradientBegin => Color.FromArgb(37, 37, 38);
            public override Color ToolStripGradientMiddle => Color.FromArgb(37, 37, 38);
            public override Color ToolStripGradientEnd => Color.FromArgb(37, 37, 38);
            public override Color StatusStripGradientBegin => Color.FromArgb(37, 37, 38);
            public override Color StatusStripGradientEnd => Color.FromArgb(37, 37, 38);
        }
    }
}
