using System;
using System.Drawing;
using System.Windows.Forms;

namespace Notepad;

public class GoToForm : Form
{
    private TextBox txtLineNumber;
    private Button btnGoTo;
    private Button btnCancel;
    private int maxLines;

    public int LineNumber { get; private set; } = 1;

    public GoToForm(int maxLines)
    {
        this.maxLines = maxLines;
        this.Text = "Go To Line";
        this.Size = new Size(300, 150);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        var lblPrompt = new Label
        {
            Text = $"Line number (1 - {maxLines}):",
            Location = new Point(12, 15),
            Size = new Size(260, 20)
        };

        txtLineNumber = new TextBox
        {
            Location = new Point(12, 40),
            Size = new Size(260, 20),
            Text = "1"
        };

        btnGoTo = new Button
        {
            Text = "Go To",
            Location = new Point(115, 75),
            Size = new Size(75, 25),
            DialogResult = DialogResult.OK
        };
        btnGoTo.Click += BtnGoTo_Click;

        btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(197, 75),
            Size = new Size(75, 25),
            DialogResult = DialogResult.Cancel
        };

        this.Controls.Add(lblPrompt);
        this.Controls.Add(txtLineNumber);
        this.Controls.Add(btnGoTo);
        this.Controls.Add(btnCancel);

        this.AcceptButton = btnGoTo;
        this.CancelButton = btnCancel;
    }

    private void BtnGoTo_Click(object? sender, EventArgs e)
    {
        if (int.TryParse(txtLineNumber.Text, out int line) && line >= 1 && line <= maxLines)
        {
            LineNumber = line;
        }
        else
        {
            MessageBox.Show("Please enter a valid line number within the range.", "Go To Line", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.DialogResult = DialogResult.None; // Prevent closing if invalid input
        }
    }
}
