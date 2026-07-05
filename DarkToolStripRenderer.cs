using System.Drawing;
using System.Windows.Forms;

namespace Notepad;

public class DarkToolStripRenderer : ToolStripProfessionalRenderer
{
    public DarkToolStripRenderer() : base(new DarkColorTable())
    {
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        // Force the text to draw as white for all dropdown and parent menu items
        e.TextColor = Color.White;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        // Force submenu expansion arrows to draw as white
        e.ArrowColor = Color.White;
        base.OnRenderArrow(e);
    }
}
