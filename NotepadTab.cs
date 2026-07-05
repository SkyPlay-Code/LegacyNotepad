using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Notepad;

public class NotepadTab : TabPage
{
    public TextBox TextBox { get; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? FilePath { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsModified { get; set; }

    public event EventHandler? ModificationStateChanged;
    public event EventHandler? CaretPositionChanged; // Cursor changes tracker

    public string DisplayName => FilePath != null ? Path.GetFileName(FilePath) : "Untitled";

    public NotepadTab(string? filePath = null)
    {
        this.FilePath = filePath;
        this.IsModified = false;
        this.Text = GetHeaderName();

        TextBox = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = true,
            Font = new Font("Consolas", 11, FontStyle.Regular),
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill
        };

        TextBox.TextChanged += TextBox_TextChanged;

        // Track keyboard selection movements and mouse clicks safely
        TextBox.KeyUp += (s, e) => CaretPositionChanged?.Invoke(this, EventArgs.Empty);
        TextBox.MouseUp += (s, e) => CaretPositionChanged?.Invoke(this, EventArgs.Empty);

        this.Controls.Add(TextBox);
    }

    private void TextBox_TextChanged(object? sender, EventArgs e)
    {
        if (!IsModified)
        {
            IsModified = true;
            this.Text = GetHeaderName();
            ModificationStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void MarkSaved(string newPath)
    {
        FilePath = newPath;
        IsModified = false;
        this.Text = GetHeaderName();
        ModificationStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetHeaderName()
    {
        string prefix = IsModified ? "*" : "";
        return $"{prefix}{DisplayName}";
    }
}
