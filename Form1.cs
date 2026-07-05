using System;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Notepad;

public class Form1 : Form
{
    private TabControl tabControl;
    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem editMenu;
    private ToolStripMenuItem recentMenu;

    // Modeless search instance tracker
    private FindReplaceForm? activeFindReplaceForm = null;

    // Font Configurations (Global)
    private Font globalFont = new Font("Consolas", 11, FontStyle.Regular);

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

        // --- Assemble File Menu ---
        fileMenu = new ToolStripMenuItem("&File");

        var newTabItem = new ToolStripMenuItem("New &Tab", null, (s, e) => AddNewTab()) { ShortcutKeys = Keys.Control | Keys.N };
        var newWindowItem = new ToolStripMenuItem("New &Window", null, OnNewWindowClick) { ShortcutKeys = Keys.Control | Keys.Shift | Keys.N };
        var openItem = new ToolStripMenuItem("&Open...", null, OnOpenClick) { ShortcutKeys = Keys.Control | Keys.O };

        recentMenu = new ToolStripMenuItem("&Recent");

        var saveItem = new ToolStripMenuItem("&Save", null, (s, e) => SaveTab(ActiveTab)) { ShortcutKeys = Keys.Control | Keys.S };
        var saveAsItem = new ToolStripMenuItem("Save &As...", null, (s, e) => SaveTabAs(ActiveTab)) { ShortcutKeys = Keys.Control | Keys.Shift | Keys.S };
        var saveAllItem = new ToolStripMenuItem("Save A&ll", null, OnSaveAllClick) { ShortcutKeys = Keys.Control | Keys.Alt | Keys.S };

        var pageSetupItem = new ToolStripMenuItem("Page Set&up...", null, (s, e) => ShowPageSetup());
        var printItem = new ToolStripMenuItem("&Print...", null, (s, e) => PrintActiveTab()) { ShortcutKeys = Keys.Control | Keys.P };

        var closeTabItem = new ToolStripMenuItem("Close &Tab", null, (s, e) => CloseTab(tabControl.SelectedIndex)) { ShortcutKeys = Keys.Control | Keys.W };
        var closeWindowItem = new ToolStripMenuItem("Close &Window", null, (s, e) => this.Close()) { ShortcutKeys = Keys.Control | Keys.Shift | Keys.W };
        var exitItem = new ToolStripMenuItem("&Exit", null, (s, e) => Application.Exit());

        fileMenu.DropDownItems.Add(newTabItem);
        fileMenu.DropDownItems.Add(newWindowItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(openItem);
        fileMenu.DropDownItems.Add(recentMenu);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(saveItem);
        fileMenu.DropDownItems.Add(saveAsItem);
        fileMenu.DropDownItems.Add(saveAllItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(pageSetupItem);
        fileMenu.DropDownItems.Add(printItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(closeTabItem);
        fileMenu.DropDownItems.Add(closeWindowItem);
        fileMenu.DropDownItems.Add(exitItem);

        // --- Assemble Edit Menu ---
        editMenu = new ToolStripMenuItem("&Edit");

        var undoItem = new ToolStripMenuItem("&Undo", null, (s, e) => ActiveTab?.TextBox.Undo()) { ShortcutKeys = Keys.Control | Keys.Z };

        var cutItem = new ToolStripMenuItem("Cu&t", null, (s, e) => ActiveTab?.TextBox.Cut()) { ShortcutKeys = Keys.Control | Keys.X };
        var copyItem = new ToolStripMenuItem("&Copy", null, (s, e) => ActiveTab?.TextBox.Copy()) { ShortcutKeys = Keys.Control | Keys.C };
        var pasteItem = new ToolStripMenuItem("&Paste", null, (s, e) => ActiveTab?.TextBox.Paste()) { ShortcutKeys = Keys.Control | Keys.V };
        var deleteItem = new ToolStripMenuItem("De&lete", null, (s, e) => { if (ActiveTab != null) ActiveTab.TextBox.SelectedText = ""; }) { ShortcutKeys = Keys.Delete };

        var findItem = new ToolStripMenuItem("&Find...", null, (s, e) => ShowFindReplace(false)) { ShortcutKeys = Keys.Control | Keys.F };
        var findNextItem = new ToolStripMenuItem("Find &Next", null, (s, e) => FindNextDirect(false)) { ShortcutKeys = Keys.F3 };
        var findPrevItem = new ToolStripMenuItem("Find Pre&vious", null, (s, e) => FindNextDirect(true)) { ShortcutKeys = Keys.Shift | Keys.F3 };
        var replaceItem = new ToolStripMenuItem("&Replace...", null, (s, e) => ShowFindReplace(true)) { ShortcutKeys = Keys.Control | Keys.H };
        var goToItem = new ToolStripMenuItem("&Go To...", null, (s, e) => GoToLine()) { ShortcutKeys = Keys.Control | Keys.G };

        var selectAllItem = new ToolStripMenuItem("Select &All", null, (s, e) => ActiveTab?.TextBox.SelectAll()) { ShortcutKeys = Keys.Control | Keys.A };
        var timeDateItem = new ToolStripMenuItem("Time/&Date", null, (s, e) => InsertTimeDate()) { ShortcutKeys = Keys.F5 };

        var fontItem = new ToolStripMenuItem("&Font...", null, (s, e) => ChangeGlobalFont());

        editMenu.DropDownItems.Add(undoItem);
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add(cutItem);
        editMenu.DropDownItems.Add(copyItem);
        editMenu.DropDownItems.Add(pasteItem);
        editMenu.DropDownItems.Add(deleteItem);
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add(findItem);
        editMenu.DropDownItems.Add(findNextItem);
        editMenu.DropDownItems.Add(findPrevItem);
        editMenu.DropDownItems.Add(replaceItem);
        editMenu.DropDownItems.Add(goToItem);
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add(selectAllItem);
        editMenu.DropDownItems.Add(timeDateItem);
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add(fontItem);

        // Bind main categories
        menuStrip.Items.Add(fileMenu);
        menuStrip.Items.Add(editMenu);
        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);

        tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            DrawMode = TabDrawMode.OwnerDrawFixed
        };
        tabControl.DrawItem += TabControl_DrawItem;
        tabControl.MouseDown += TabControl_MouseDown;

        // When user switches tabs, close the modeless find pop-up to avoid typing-target sync bugs
        tabControl.SelectedIndexChanged += (s, e) => {
            if (activeFindReplaceForm != null && !activeFindReplaceForm.IsDisposed)
            {
                activeFindReplaceForm.Close();
            }
            UpdateFormTitle();
        };

        this.Controls.Add(tabControl);
        tabControl.BringToFront();

        RefreshRecentFilesMenu();
        AddNewTab();
    }

    // --- Tab Management & Owner Drawing ---

    private NotepadTab AddNewTab(string? filePath = null, string? initialText = null)
    {
        var tab = new NotepadTab(filePath);
        tab.TextBox.Font = globalFont; // Apply active global font setting

        if (initialText != null)
        {
            tab.TextBox.Text = initialText;
            tab.IsModified = false;
            tab.Text = tab.GetHeaderName();
        }

        tab.ModificationStateChanged += (s, e) => {
            UpdateFormTitle();
            tabControl.Invalidate();
        };

        tabControl.TabPages.Add(tab);
        tabControl.SelectedTab = tab;
        this.ActiveControl = tab.TextBox;
        UpdateFormTitle();
        return tab;
    }

    private void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= tabControl.TabPages.Count) return;

        TabPage tabPage = tabControl.TabPages[e.Index];
        Rectangle tabRect = tabControl.GetTabRect(e.Index);

        Color backColor = e.State == DrawItemState.Selected ? Color.White : Color.FromArgb(240, 240, 240);
        using (Brush backBrush = new SolidBrush(backColor))
        {
            e.Graphics.FillRectangle(backBrush, tabRect);
        }

        string tabText = tabPage.Text;
        using (Font font = e.State == DrawItemState.Selected ? new Font(this.Font, FontStyle.Bold) : this.Font)
        {
            Rectangle textRect = new Rectangle(tabRect.X + 6, tabRect.Y + 4, tabRect.Width - 24, tabRect.Height - 8);
            TextRenderer.DrawText(e.Graphics, tabText, font, textRect, Color.Black, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        Rectangle closeRect = GetCloseButtonRect(tabRect);
        using (Font xFont = new Font("Segoe UI", 8, FontStyle.Regular))
        {
            TextRenderer.DrawText(e.Graphics, "x", xFont, closeRect, Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private Rectangle GetCloseButtonRect(Rectangle tabRect)
    {
        return new Rectangle(tabRect.Right - 16, tabRect.Y + 4, 12, 12);
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

    // --- Edit Actions & Dialog Managers ---

    private void InsertTimeDate()
    {
        if (ActiveTab != null)
        {
            string formattedDateTime = DateTime.Now.ToString("h:mm tt M/d/yyyy");
            ActiveTab.TextBox.SelectedText = formattedDateTime;
        }
    }

    private void ChangeGlobalFont()
    {
        using (FontDialog fd = new FontDialog())
        {
            fd.Font = globalFont;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                globalFont = fd.Font;
                // Force update across all open tabs dynamically
                foreach (NotepadTab tab in tabControl.TabPages)
                {
                    tab.TextBox.Font = globalFont;
                }
            }
        }
    }

    private void GoToLine()
    {
        if (ActiveTab == null) return;
        string[] lines = ActiveTab.TextBox.Lines;

        using (GoToForm gtf = new GoToForm(lines.Length == 0 ? 1 : lines.Length))
        {
            if (gtf.ShowDialog(this) == DialogResult.OK)
            {
                int targetLine = gtf.LineNumber;
                if (targetLine < 1) return;

                // Trace caret index position of target line bounds
                int charIndex = 0;
                for (int i = 0; i < targetLine - 1; i++)
                {
                    charIndex += lines[i].Length + Environment.NewLine.Length;
                }

                ActiveTab.TextBox.SelectionStart = charIndex;
                ActiveTab.TextBox.SelectionLength = 0;
                ActiveTab.TextBox.ScrollToCaret();
                ActiveTab.TextBox.Focus();
            }
        }
    }

    private void ShowFindReplace(bool isReplaceMode)
    {
        if (ActiveTab == null) return;

        // Dismiss active search overlays if existing
        if (activeFindReplaceForm != null && !activeFindReplaceForm.IsDisposed)
        {
            activeFindReplaceForm.Close();
        }

        activeFindReplaceForm = new FindReplaceForm(ActiveTab.TextBox, isReplaceMode);
        activeFindReplaceForm.FormClosed += (s, e) => activeFindReplaceForm = null;
        activeFindReplaceForm.Show(this); // Shown Modelessly!
    }

    private void FindNextDirect(bool searchUp)
    {
        if (ActiveTab == null) return;
        string findText = SearchState.LastSearchText;

        // If no prior search query exists, slide open search setup modal instead
        if (string.IsNullOrEmpty(findText))
        {
            ShowFindReplace(false);
            return;
        }

        string mainText = ActiveTab.TextBox.Text;
        StringComparison comparison = SearchState.LastMatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
        int startPos = ActiveTab.TextBox.SelectionStart;

        if (searchUp)
        {
            int searchStart = startPos - 1;
            if (searchStart < 0) searchStart = 0;

            int index = mainText.LastIndexOf(findText, searchStart, comparison);
            if (index != -1)
            {
                ActiveTab.TextBox.Select(index, findText.Length);
                ActiveTab.TextBox.ScrollToCaret();
                ActiveTab.TextBox.Focus();
            }
            else
            {
                MessageBox.Show($"Cannot find \"{findText}\"", "Legacy Notepad", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else
        {
            int searchStart = startPos + ActiveTab.TextBox.SelectionLength;
            if (searchStart > mainText.Length) searchStart = mainText.Length;

            int index = mainText.IndexOf(findText, searchStart, comparison);
            if (index != -1)
            {
                ActiveTab.TextBox.Select(index, findText.Length);
                ActiveTab.TextBox.ScrollToCaret();
                ActiveTab.TextBox.Focus();
            }
            else
            {
                MessageBox.Show($"Cannot find \"{findText}\"", "Legacy Notepad", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    // --- Core File Handling Engine ---

    private void OnNewWindowClick(object? sender, EventArgs e)
    {
        string? path = Environment.ProcessPath;
        if (path != null)
        {
            System.Diagnostics.Process.Start(path);
        }
    }

    private void OnOpenClick(object? sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenFileFromPath(openFileDialog.FileName);
            }
        }
    }

    private void OpenFileFromPath(string path)
    {
        try
        {
            string text = File.ReadAllText(path, System.Text.Encoding.UTF8);

            if (ActiveTab != null && ActiveTab.FilePath == null && !ActiveTab.IsModified && ActiveTab.TextBox.Text.Length == 0)
            {
                ActiveTab.TextBox.Text = text;
                ActiveTab.MarkSaved(path);
            }
            else
            {
                AddNewTab(path, text);
            }

            RecentFilesManager.AddRecentFile(path);
            RefreshRecentFilesMenu();
            UpdateFormTitle();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool SaveTab(NotepadTab? tab)
    {
        if (tab == null) return false;

        if (tab.FilePath == null)
        {
            return SaveTabAs(tab);
        }
        else
        {
            try
            {
                File.WriteAllText(tab.FilePath, tab.TextBox.Text, System.Text.Encoding.UTF8);
                tab.MarkSaved(tab.FilePath);
                RecentFilesManager.AddRecentFile(tab.FilePath);
                RefreshRecentFilesMenu();
                UpdateFormTitle();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }

    private bool SaveTabAs(NotepadTab? tab)
    {
        if (tab == null) return false;

        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.DefaultExt = "txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                tab.FilePath = saveFileDialog.FileName;
                return SaveTab(tab);
            }
        }
        return false;
    }

    private void OnSaveAllClick(object? sender, EventArgs e)
    {
        foreach (NotepadTab tab in tabControl.TabPages)
        {
            if (tab.IsModified)
            {
                SaveTab(tab);
            }
        }
    }

    private bool ConfirmDiscardChanges(NotepadTab tab)
    {
        if (!tab.IsModified) return true;

        DialogResult result = MessageBox.Show(
            $"Do you want to save changes to {tab.DisplayName}?",
            "Legacy Notepad",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning
        );

        if (result == DialogResult.Yes)
        {
            return SaveTab(tab);
        }
        else if (result == DialogResult.No)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // --- Page Setup & Printing Systems ---

    private void ShowPageSetup()
    {
        using (PageSetupDialog psd = new PageSetupDialog())
        {
            psd.Document = printDocument;
            psd.ShowDialog();
        }
    }

    private void PrintActiveTab()
    {
        if (ActiveTab == null) return;

        using (PrintDialog pd = new PrintDialog())
        {
            pd.Document = printDocument;
            pd.UseEXDialog = true;

            if (pd.ShowDialog() == DialogResult.OK)
            {
                textToPrint = ActiveTab.TextBox.Text;
                characterPosition = 0;

                printDocument.PrintPage += OnPrintPage;
                try
                {
                    printDocument.Print();
                }
                finally
                {
                    printDocument.PrintPage -= OnPrintPage;
                }
            }
        }
    }

    private void OnPrintPage(object sender, PrintPageEventArgs ev)
    {
        if (ev.Graphics == null || ActiveTab == null) return;

        Font printFont = ActiveTab.TextBox.Font;

        int charsFitted;
        int linesFitted;

        ev.Graphics.MeasureString(
            textToPrint.Substring(characterPosition),
            printFont,
            ev.MarginBounds.Size,
            StringFormat.GenericTypographic,
            out charsFitted,
            out linesFitted
        );

        ev.Graphics.DrawString(
            textToPrint.Substring(characterPosition, charsFitted),
            printFont,
            Brushes.Black,
            ev.MarginBounds,
            StringFormat.GenericTypographic
        );

        characterPosition += charsFitted;
        ev.HasMorePages = characterPosition < textToPrint.Length;
    }

    // --- Window Closures Handling ---

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        for (int i = tabControl.TabPages.Count - 1; i >= 0; i--)
        {
            var tab = (NotepadTab)tabControl.TabPages[i];
            tabControl.SelectedTab = tab;

            if (!ConfirmDiscardChanges(tab))
            {
                e.Cancel = true;
                return;
            }
            else
            {
                tabControl.TabPages.RemoveAt(i);
            }
        }
        base.OnFormClosing(e);
    }
}
