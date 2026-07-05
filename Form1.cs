using System;
using System.Drawing;
using System.Windows.Forms;

namespace Notepad;

public class Form1 : Form
{
    // Declare our UI elements
    private TextBox textBox;
    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem exitMenuItem;

    public Form1()
    {
        // 1. Configure the Main Window (Form)
        this.Text = "Untitled - Legacy Notepad";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        // 2. Initialize the Menu Strip (Top Menu Bar)
        menuStrip = new MenuStrip();
        
        // Create the "File" menu category
        fileMenu = new ToolStripMenuItem("&File");
        
        // Create the "Exit" action, linking it to our close event
        exitMenuItem = new ToolStripMenuItem("&Exit", null, OnExitClick);
        
        // Assemble the menus
        fileMenu.DropDownItems.Add(exitMenuItem);
        menuStrip.Items.Add(fileMenu);
        
        // Assign the menu strip to the window and add it as a control
        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);

        // 3. Initialize the Main Text Box
        textBox = new TextBox();
        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.Both; // Show vertical & horizontal scrollbars
        textBox.WordWrap = true;              // Wrap text by default
        textBox.Font = new Font("Consolas", 11, FontStyle.Regular); // Standard coding font
        textBox.BorderStyle = BorderStyle.None; // Flat, borderless look (classic clean style)
        textBox.Dock = DockStyle.Fill;        // Instructs the textbox to fill the remaining screen space

        // Add the text box to the window
        this.Controls.Add(textBox);
        
        // Make sure the text box goes behind the menu bar, not over it
        textBox.BringToFront(); 
    }

    // This runs when the user clicks "File -> Exit"
    private void OnExitClick(object? sender, EventArgs e)
    {
        this.Close();
    }
}