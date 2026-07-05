using System;
using System.Windows.Forms;
using System.Drawing;

namespace Notepad;

// Global container to store historical search configurations
public static class SearchState
{
    public static string LastSearchText = "";
    public static bool LastMatchCase = false;
}

public class FindReplaceForm : Form
{
    private TextBox mainTextBox;
    private Label lblFind;
    private TextBox txtFind;
    private Label lblReplace;
    private TextBox txtReplace;
    private CheckBox chkMatchCase;
    private RadioButton rdoUp;
    private RadioButton rdoDown;
    private GroupBox grpDirection;
    private Button btnFindNext;
    private Button btnReplace;
    private Button btnReplaceAll;
    private Button btnCancel;

    public FindReplaceForm(TextBox targetTextBox, bool isReplaceMode = false)
    {
        this.mainTextBox = targetTextBox;
        this.Text = isReplaceMode ? "Replace" : "Find";
        this.Size = new Size(420, isReplaceMode ? 220 : 160);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        lblFind = new Label { Text = "Find what:", Location = new Point(10, 15), Size = new Size(80, 20) };
        txtFind = new TextBox { Text = SearchState.LastSearchText, Location = new Point(90, 12), Size = new Size(200, 20) };

        lblReplace = new Label { Text = "Replace with:", Location = new Point(10, 45), Size = new Size(80, 20), Visible = isReplaceMode };
        txtReplace = new TextBox { Location = new Point(90, 42), Size = new Size(200, 20), Visible = isReplaceMode };

        chkMatchCase = new CheckBox { Text = "Match case", Checked = SearchState.LastMatchCase, Location = new Point(10, isReplaceMode ? 80 : 50), Size = new Size(100, 20) };

        grpDirection = new GroupBox { Text = "Direction", Location = new Point(120, 50), Size = new Size(110, 45), Visible = !isReplaceMode };
        rdoUp = new RadioButton { Text = "Up", Location = new Point(10, 18), Size = new Size(45, 20) };
        rdoDown = new RadioButton { Text = "Down", Checked = true, Location = new Point(55, 18), Size = new Size(50, 20) };
        grpDirection.Controls.Add(rdoUp);
        grpDirection.Controls.Add(rdoDown);

        btnFindNext = new Button { Text = "Find Next", Location = new Point(310, 10), Size = new Size(85, 25) };
        btnReplace = new Button { Text = "Replace", Location = new Point(310, 40), Size = new Size(85, 25), Visible = isReplaceMode };
        btnReplaceAll = new Button { Text = "Replace All", Location = new Point(310, 70), Size = new Size(85, 25), Visible = isReplaceMode };
        btnCancel = new Button { Text = "Cancel", Location = new Point(310, isReplaceMode ? 100 : 40), Size = new Size(85, 25) };

        this.Controls.Add(lblFind);
        this.Controls.Add(txtFind);
        this.Controls.Add(lblReplace);
        this.Controls.Add(txtReplace);
        this.Controls.Add(chkMatchCase);
        this.Controls.Add(grpDirection);
        this.Controls.Add(btnFindNext);
        this.Controls.Add(btnReplace);
        this.Controls.Add(btnReplaceAll);
        this.Controls.Add(btnCancel);

        btnFindNext.Click += FindNext;
        btnReplace.Click += ReplaceSelected;
        btnReplaceAll.Click += ReplaceAll;
        btnCancel.Click += (s, e) => this.Close();
    }

    public void FindNext(object? sender, EventArgs e)
    {
        FindText(rdoUp.Visible && rdoUp.Checked);
    }

    private void FindText(bool searchUp)
    {
        string findText = txtFind.Text;
        if (string.IsNullOrEmpty(findText)) return;

        // Sync local settings to global cache
        SearchState.LastSearchText = findText;
        SearchState.LastMatchCase = chkMatchCase.Checked;

        string mainText = mainTextBox.Text;
        StringComparison comparison = chkMatchCase.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
        int startPos = mainTextBox.SelectionStart;

        if (searchUp)
        {
            int searchStart = startPos - 1;
            if (searchStart < 0) searchStart = 0;

            int index = mainText.LastIndexOf(findText, searchStart, comparison);
            if (index != -1)
            {
                mainTextBox.Select(index, findText.Length);
                mainTextBox.ScrollToCaret();
                mainTextBox.Focus();
            }
            else
            {
                MessageBox.Show($"Cannot find \"{findText}\"", "Legacy Notepad", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else
        {
            int searchStart = startPos + mainTextBox.SelectionLength;
            if (searchStart > mainText.Length) searchStart = mainText.Length;

            int index = mainText.IndexOf(findText, searchStart, comparison);
            if (index != -1)
            {
                mainTextBox.Select(index, findText.Length);
                mainTextBox.ScrollToCaret();
                mainTextBox.Focus();
            }
            else
            {
                MessageBox.Show($"Cannot find \"{findText}\"", "Legacy Notepad", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void ReplaceSelected(object? sender, EventArgs e)
    {
        if (mainTextBox.SelectedText.Equals(txtFind.Text, chkMatchCase.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
        {
            mainTextBox.SelectedText = txtReplace.Text;
        }
        FindNext(sender, e);
    }

    private void ReplaceAll(object? sender, EventArgs e)
    {
        string findText = txtFind.Text;
        string replaceText = txtReplace.Text;
        if (string.IsNullOrEmpty(findText)) return;

        string mainText = mainTextBox.Text;
        StringComparison comparison = chkMatchCase.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        int count = 0;
        int index = mainText.IndexOf(findText, comparison);

        while (index != -1)
        {
            mainTextBox.Select(index, findText.Length);
            mainTextBox.SelectedText = replaceText;

            mainText = mainTextBox.Text; // Refresh content copy
            index = mainText.IndexOf(findText, index + replaceText.Length, comparison);
            count++;
        }

        MessageBox.Show($"{count} occurrence(s) replaced.", "Replace All", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
