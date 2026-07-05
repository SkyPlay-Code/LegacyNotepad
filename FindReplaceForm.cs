using System;
using System.Drawing;
using System.Windows.Forms;

namespace Notepad;

public class FindReplaceForm : Form
{
    private TextBox targetTextBox;
    private TextBox txtFind;
    private TextBox txtReplace;
    private CheckBox chkMatchCase;
    private RadioButton rdoUp;
    private RadioButton rdoDown;
    private Button btnFindNext;
    private Button? btnReplace;     // Declared as nullable to fix CS8618 warnings
    private Button? btnReplaceAll;  // Declared as nullable to fix CS8618 warnings
    private Button btnCancel;

    public FindReplaceForm(TextBox textBox, bool isReplaceMode)
    {
        this.targetTextBox = textBox;
        this.Text = isReplaceMode ? "Replace" : "Find";
        this.Size = new Size(400, isReplaceMode ? 220 : 170);
        this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        this.StartPosition = FormStartPosition.CenterParent;
        this.ShowInTaskbar = false;

        var lblFind = new Label { Text = "Find what:", Location = new Point(10, 15), Size = new Size(80, 20) };
        txtFind = new TextBox { Location = new Point(90, 12), Size = new Size(180, 20), Text = SearchState.LastSearchText };

        this.Controls.Add(lblFind);
        this.Controls.Add(txtFind);

        if (isReplaceMode)
        {
            var lblReplace = new Label { Text = "Replace with:", Location = new Point(10, 45), Size = new Size(80, 20) };
            txtReplace = new TextBox { Location = new Point(90, 42), Size = new Size(180, 20), Text = SearchState.LastReplaceText };
            this.Controls.Add(lblReplace);
            this.Controls.Add(txtReplace);
        }
        else
        {
            txtReplace = new TextBox();
        }

        chkMatchCase = new CheckBox
        {
            Text = "Match case",
            Location = new Point(10, isReplaceMode ? 85 : 55),
            Size = new Size(100, 20),
            Checked = SearchState.LastMatchCase
        };
        this.Controls.Add(chkMatchCase);

        if (!isReplaceMode)
        {
            var grpDirection = new GroupBox
            {
                Text = "Direction",
                Location = new Point(120, 45),
                Size = new Size(150, 50)
            };
            rdoUp = new RadioButton { Text = "Up", Location = new Point(10, 20), Size = new Size(50, 20) };
            rdoDown = new RadioButton { Text = "Down", Location = new Point(70, 20), Size = new Size(70, 20), Checked = true };
            grpDirection.Controls.Add(rdoUp);
            grpDirection.Controls.Add(rdoDown);
            this.Controls.Add(grpDirection);
        }
        else
        {
            rdoUp = new RadioButton();
            rdoDown = new RadioButton();
        }

        int btnX = 285;
        btnFindNext = new Button { Text = "Find Next", Location = new Point(btnX, 10), Size = new Size(85, 25) };
        btnFindNext.Click += BtnFindNext_Click;
        this.Controls.Add(btnFindNext);

        if (isReplaceMode)
        {
            btnReplace = new Button { Text = "Replace", Location = new Point(btnX, 40), Size = new Size(85, 25) };
            btnReplace.Click += BtnReplace_Click;

            btnReplaceAll = new Button { Text = "Replace All", Location = new Point(btnX, 70), Size = new Size(85, 25) };
            btnReplaceAll.Click += BtnReplaceAll_Click;

            this.Controls.Add(btnReplace);
            this.Controls.Add(btnReplaceAll);
        }

        btnCancel = new Button { Text = "Cancel", Location = new Point(btnX, isReplaceMode ? 100 : 40), Size = new Size(85, 25) };
        btnCancel.Click += (s, e) => this.Close();
        this.Controls.Add(btnCancel);
    }

    private void SyncSearchState()
    {
        SearchState.LastSearchText = txtFind.Text;
        SearchState.LastReplaceText = txtReplace.Text;
        SearchState.LastMatchCase = chkMatchCase.Checked;
    }

    private void BtnFindNext_Click(object? sender, EventArgs e)
    {
        SyncSearchState();
        bool searchUp = rdoUp.Checked;
        PerformSearch(searchUp);
    }

    private void PerformSearch(bool searchUp)
    {
        string findText = SearchState.LastSearchText;
        if (string.IsNullOrEmpty(findText)) return;

        string mainText = targetTextBox.Text;
        StringComparison comp = SearchState.LastMatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
        int startPos = targetTextBox.SelectionStart;

        if (searchUp)
        {
            int searchStart = startPos - 1;
            if (searchStart < 0) searchStart = 0;

            int index = mainText.LastIndexOf(findText, searchStart, comp);
            if (index != -1)
            {
                targetTextBox.Select(index, findText.Length);
                targetTextBox.ScrollToCaret();
                targetTextBox.Focus();
            }
            else
            {
                MessageBox.Show($"Cannot find \"{findText}\"", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else
        {
            int searchStart = startPos + targetTextBox.SelectionLength;
            if (searchStart > mainText.Length) searchStart = mainText.Length;

            int index = mainText.IndexOf(findText, searchStart, comp);
            if (index != -1)
            {
                targetTextBox.Select(index, findText.Length);
                targetTextBox.ScrollToCaret();
                targetTextBox.Focus();
            }
            else
            {
                MessageBox.Show($"Cannot find \"{findText}\"", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void BtnReplace_Click(object? sender, EventArgs e)
    {
        SyncSearchState();
        string findText = SearchState.LastSearchText;
        if (string.IsNullOrEmpty(findText)) return;

        if (targetTextBox.SelectedText.Equals(findText, SearchState.LastMatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
        {
            targetTextBox.SelectedText = SearchState.LastReplaceText;
        }
        PerformSearch(false);
    }

    private void BtnReplaceAll_Click(object? sender, EventArgs e)
    {
        SyncSearchState();
        string findText = SearchState.LastSearchText;
        if (string.IsNullOrEmpty(findText)) return;

        string replaceText = SearchState.LastReplaceText;
        string mainText = targetTextBox.Text;
        StringComparison comp = SearchState.LastMatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        int count = 0;
        int index = mainText.IndexOf(findText, comp);
        while (index != -1)
        {
            targetTextBox.Select(index, findText.Length);
            targetTextBox.SelectedText = replaceText;
            mainText = targetTextBox.Text;
            index = mainText.IndexOf(findText, index + replaceText.Length, comp);
            count++;
        }

        MessageBox.Show($"Replaced {count} occurrence(s).", "Replace All", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
