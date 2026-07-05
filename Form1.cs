using System;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Notepad;

public enum ThemeMode { System, Light, Dark }

public partial class Form1 : Form
{
    private DarkTabControl tabControl; // Uses the custom DarkTabControl subclass
    private MenuStrip menuStrip;
    private StatusStrip statusStrip;

    // Class-scoped menu fields
    private ToolStripMenuItem fileMenu = null!;
    private ToolStripMenuItem editMenu = null!;
    private ToolStripMenuItem viewMenu = null!;
    private ToolStripMenuItem zoomMenu = null!;
    private ToolStripMenuItem recentMenu = null!;

    // Class-scoped status labels
    private ToolStripStatusLabel lblPosition = null!;
    private ToolStripStatusLabel lblCharCount = null!;
    private ToolStripStatusLabel lblZoom = null!;
    private ToolStripStatusLabel lblLineEndings = null!;
    private ToolStripStatusLabel lblEncoding = null!;

    // Dialog reference tracker
    private FindReplaceForm? activeFindReplaceForm = null;

    // Theme Configs
    private ThemeMode currentTheme = ThemeMode.System;

    // Font & Zoom Configs
    private Font globalFont = new Font("Consolas", 11, FontStyle.Regular);
    private float currentZoomPercentage = 100f;
    private const float DefaultFontSize = 11f;

    // Printing Setup
    private PrintDocument printDocument = new PrintDocument();
    private string textToPrint = "";
    private int characterPosition = 0;

    private NotepadTab? ActiveTab => tabControl.SelectedTab as NotepadTab;

    public Form1()
    {
        this.Text = "Legacy Notepad";
        this.Size = new Size(850, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        menuStrip = new MenuStrip();

        // Initialize menu structures
        InitializeFileMenu();
        InitializeEditMenu();
        InitializeViewMenu();

        // FIX 1: Register the top-level menus to the MenuStrip (This was missing)
        menuStrip.Items.Add(fileMenu);
        menuStrip.Items.Add(editMenu);
        menuStrip.Items.Add(viewMenu);

        // Footer initialization
        statusStrip = new StatusStrip();
        lblPosition = new ToolStripStatusLabel("Ln 1, Col 1");
        lblCharCount = new ToolStripStatusLabel("0 characters");
        var spacer = new ToolStripStatusLabel { Spring = true }; // Right-aligns all elements following it
        lblZoom = new ToolStripStatusLabel("100%");
        lblLineEndings = new ToolStripStatusLabel("Windows (CRLF)");
        lblEncoding = new ToolStripStatusLabel("UTF-8");

        statusStrip.Items.Add(lblPosition);
        statusStrip.Items.Add(new ToolStripStatusLabel("  |  ") { Enabled = false });
        statusStrip.Items.Add(lblCharCount);
        statusStrip.Items.Add(spacer);
        statusStrip.Items.Add(lblZoom);
        statusStrip.Items.Add(new ToolStripStatusLabel("  |  ") { Enabled = false });
        statusStrip.Items.Add(lblLineEndings);
        statusStrip.Items.Add(new ToolStripStatusLabel("  |  ") { Enabled = false });
        statusStrip.Items.Add(lblEncoding);

        // Fixed tab constraints with adjusted heights for proper text scaling
        tabControl = new DarkTabControl
        {
            Dock = DockStyle.Fill,
            DrawMode = TabDrawMode.OwnerDrawFixed,
            ItemSize = new Size(130, 30), // Height/width configured for DPI and alignment
            SizeMode = TabSizeMode.Fixed
        };
        tabControl.DrawItem += TabControl_DrawItem;
        tabControl.MouseDown += TabControl_MouseDown;

        tabControl.SelectedIndexChanged += (s, e) => {
            if (activeFindReplaceForm != null && !activeFindReplaceForm.IsDisposed)
            {
                activeFindReplaceForm.Close();
            }
            UpdateFormTitle();
            UpdateStatusBar();
        };

        // Z-order docking layout (prevents menus overlapping tabs)
        this.Controls.Add(tabControl);
        this.Controls.Add(statusStrip);
        this.Controls.Add(menuStrip);
        this.MainMenuStrip = menuStrip;

        // FIX 2: Bring the fill-docked control to the front of the Z-order so it docks last,
        // correctly filling only the remaining space after top/bottom-docked controls are positioned.
        tabControl.BringToFront();

        RefreshRecentFilesMenu();
        AddNewTab();
        ApplyTheme();
    }

    // --- Tab Owner Drawing ---

    private void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= tabControl.TabPages.Count) return;

        TabPage tabPage = tabControl.TabPages[e.Index];
        Rectangle tabRect = tabControl.GetTabRect(e.Index);

        bool isDark = currentTheme == ThemeMode.Dark || (currentTheme == ThemeMode.System && IsSystemDarkTheme());

        Color backColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
            ? (isDark ? Color.FromArgb(30, 30, 30) : Color.White)
            : (isDark ? Color.FromArgb(40, 40, 40) : Color.FromArgb(230, 230, 230));
        Color textColor = isDark ? Color.White : Color.Black;
        Color xColor = isDark ? Color.LightGray : Color.Gray;

        using (Brush backBrush = new SolidBrush(backColor))
        {
            e.Graphics.FillRectangle(backBrush, tabRect);
        }

        string tabText = tabPage.Text;

        Font font;
        bool mustDisposeFont = false;

        // Bitwise check for the Selected state (safer and more robust on all displays)
        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
        {
            font = new Font(this.Font, FontStyle.Bold);
            mustDisposeFont = true;
        }
        else
        {
            font = this.Font; // References the shared resource without duplicating/disposing it
        }

        try
        {
            // Set 8px left margin and reserve 28px on the right for the close button
            Rectangle textRect = new Rectangle(tabRect.X + 8, tabRect.Y + 3, tabRect.Width - 28, tabRect.Height - 6);
            TextRenderer.DrawText(e.Graphics, tabText, font, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
        finally
        {
            // ONLY dispose of the font if we explicitly created a new Bold instance
            if (mustDisposeFont)
            {
                font.Dispose();
            }
        }

        Rectangle closeRect = GetCloseButtonRect(tabRect);
        using (Font xFont = new Font("Segoe UI", 8, FontStyle.Regular))
        {
            TextRenderer.DrawText(e.Graphics, "x", xFont, closeRect, xColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private Rectangle GetCloseButtonRect(Rectangle tabRect)
    {
        // Centers a 14x14 close button box vertically, staying 6px from the right edge
        int size = 14;
        int x = tabRect.Right - size - 6;
        int y = tabRect.Y + (tabRect.Height - size) / 2;
        return new Rectangle(x, y, size, size);
    }

    private void TabControl_MouseDown(object? sender, MouseEventArgs e)
    {
        for (int i = 0; i < tabControl.TabPages.Count; i++)
        {
            Rectangle tabRect = tabControl.GetTabRect(i);
            Rectangle closeRect = GetCloseButtonRect(tabRect);
            if (closeRect.Contains(e.Location))
            {
                CloseTab(i);
                break;
            }
        }
    }

    private bool CloseTab(int index)
    {
        if (index < 0 || index >= tabControl.TabPages.Count) return false;

        var tab = (NotepadTab)tabControl.TabPages[index];
        tabControl.SelectedTab = tab;

        if (ConfirmDiscardChanges(tab))
        {
            tabControl.TabPages.RemoveAt(index);

            if (tabControl.TabPages.Count == 0)
            {
                AddNewTab();
            }

            UpdateFormTitle();
            return true;
        }
        return false;
    }

    // --- State & Menu Refresh Controllers ---

    private void UpdateFormTitle()
    {
        if (ActiveTab != null)
        {
            this.Text = $"{ActiveTab.Text} - Legacy Notepad";
        }
        else
        {
            this.Text = "Legacy Notepad";
        }
    }

    private void RefreshRecentFilesMenu()
    {
        recentMenu.DropDownItems.Clear();
        var files = RecentFilesManager.LoadRecentFiles();

        if (files.Count == 0)
        {
            recentMenu.DropDownItems.Add(new ToolStripMenuItem("(No recent files)") { Enabled = false });
            return;
        }

        foreach (var file in files)
        {
            string menuText = Path.GetFileName(file);
            var item = new ToolStripMenuItem(menuText, null, OnRecentFileClick) { Tag = file, ToolTipText = file };
            recentMenu.DropDownItems.Add(item);
        }
    }

    private void OnRecentFileClick(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem clickedItem && clickedItem.Tag is string path)
        {
            OpenFileFromPath(path);
        }
    }

    // --- Modern Theme Engine (Registry-linked) ---

    private bool IsSystemDarkTheme()
    {
        try
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (key != null)
                {
                    var value = key.GetValue("AppsUseLightTheme");
                    if (value is int val) return val == 0;
                }
            }
        }
        catch { }
        return false;
    }

    private void ApplyTheme()
    {
        bool isDark = currentTheme == ThemeMode.Dark || (currentTheme == ThemeMode.System && IsSystemDarkTheme());

        Color formBg = isDark ? Color.FromArgb(28, 28, 28) : Color.FromArgb(243, 243, 243);
        Color menuBg = isDark ? Color.FromArgb(32, 32, 32) : Color.FromArgb(243, 243, 243);
        Color textBg = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
        Color textFg = isDark ? Color.White : Color.Black;
        Color menuFg = isDark ? Color.White : Color.Black;

        this.BackColor = formBg;

        menuStrip.BackColor = menuBg;
        menuStrip.ForeColor = menuFg;
        statusStrip.BackColor = menuBg;
        statusStrip.ForeColor = menuFg;

        foreach (ToolStripItem item in statusStrip.Items)
        {
            item.ForeColor = menuFg;
        }

        // Apply custom dark renderer or fallback to the native system renderer
        menuStrip.Renderer = isDark ? new DarkToolStripRenderer() : null;

        tabControl.BackColor = formBg;
        foreach (NotepadTab tab in tabControl.TabPages)
        {
            tab.BackColor = textBg;
            tab.TextBox.BackColor = textBg; // Styled internal text element
            tab.TextBox.ForeColor = textFg;
        }

        tabControl.Invalidate();
        UpdateThemeMenuChecks();
    }

    // --- Footer Status Bar Engine ---

    private void UpdateStatusBar()
    {
        if (ActiveTab == null) return;

        // Fetch selection indexes
        int index = ActiveTab.TextBox.SelectionStart;
        int line = ActiveTab.TextBox.GetLineFromCharIndex(index);
        int firstCharOfLine = ActiveTab.TextBox.GetFirstCharIndexFromLine(line);
        int col = index - firstCharOfLine;

        lblPosition.Text = $"Ln {line + 1}, Col {col + 1}";
        lblCharCount.Text = $"{ActiveTab.TextBox.Text.Length} characters";
        lblZoom.Text = $"{(int)currentZoomPercentage}%";

        string content = ActiveTab.TextBox.Text;
        if (content.Contains("\r\n"))
            lblLineEndings.Text = "Windows (CRLF)";
        else if (content.Contains("\n"))
            lblLineEndings.Text = "Unix (LF)";
        else if (content.Contains("\r"))
            lblLineEndings.Text = "Macintosh (CR)";
        else
            lblLineEndings.Text = "Windows (CRLF)"; // Default fallback for newly initialized empty files
    }
}
