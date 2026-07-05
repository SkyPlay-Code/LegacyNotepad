using System;
using System.Windows.Forms;
using System.Drawing;

namespace Notepad;

public class GoToForm : Form
{
    private Label label;
    private TextBox lineTextBox;
    private Button okButton;
    private Button cancelButton;

    public int LineNumber { get; private set; } = -1;

    public GoToForm(int maxLines)
    {
        this.Text = "Go To Line";
        this.Size = new Size(300, 150);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        label = new Label { Text = $"Line number (1 - {maxLines}):", Location = new Point(12, 15), Size = new Size(260, 20) };
        lineTextBox = new TextBox { Location = new Point(12, 40), Size = new Size(260, 20) };

        okButton = new Button { Text = "Go To", Location = new Point(115, 80), Size = new Size(75, 25), DialogResult = DialogResult.OK };
        cancelButton = new Button { Text = "Cancel", Location = new Point(195, 80), Size = new Size(75, 25), DialogResult = DialogResult.Cancel };

        this.Controls.Add(label);
        this.Controls.Add(lineTextBox);
        this.Controls.Add(okButton);
        this.Controls.Add(cancelButton);

        this.AcceptButton = okButton;
        this.CancelButton = cancelButton;

        okButton.Click += (s, e) =>
        {
            if (int.TryParse(lineTextBox.Text, out int line) && line >= 1 && line <= maxLines)
            {
                LineNumber = line;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid line number.", "Go To Line", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
    }
}
