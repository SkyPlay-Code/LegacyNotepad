using System;
using System.Drawing;
using System.Windows.Forms;

namespace Notepad;

public partial class Form1
{
    private void InitializeEditMenu()
    {
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
    }

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
                globalFont = new Font(fd.Font.FontFamily, globalFont.Size, fd.Font.Style);
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

        if (activeFindReplaceForm != null && !activeFindReplaceForm.IsDisposed)
        {
            activeFindReplaceForm.Close();
        }

        activeFindReplaceForm = new FindReplaceForm(ActiveTab.TextBox, isReplaceMode);
        activeFindReplaceForm.FormClosed += (s, e) => activeFindReplaceForm = null;
        activeFindReplaceForm.Show(this);
    }

    private void FindNextDirect(bool searchUp)
    {
        if (ActiveTab == null) return;
        string findText = SearchState.LastSearchText;

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
                MessageBox.Show($"Cannot find \"{findText}\"", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"Cannot find \"{findText}\"", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
