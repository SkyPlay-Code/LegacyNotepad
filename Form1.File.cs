using System;
using System.IO;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Notepad;

public partial class Form1
{
    private void InitializeFileMenu()
    {
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
    }

    private NotepadTab AddNewTab(string? filePath = null, string? initialText = null)
    {
        var tab = new NotepadTab(filePath);
        tab.TextBox.Font = globalFont;

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

        // Real-time cursor hook
        tab.CaretPositionChanged += (s, e) => UpdateStatusBar();

        tabControl.TabPages.Add(tab);
        tabControl.SelectedTab = tab;
        this.ActiveControl = tab.TextBox;

        UpdateFormTitle();
        UpdateStatusBar();
        ApplyTheme(); // Automatically forces theme adjustments on newly added tabs
        return tab;
    }

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
            UpdateStatusBar();
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
}
