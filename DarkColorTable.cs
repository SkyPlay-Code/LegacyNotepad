using System.Drawing;
using System.Windows.Forms;

namespace Notepad;

public class DarkColorTable : ProfessionalColorTable
{
    public override Color ToolStripDropDownBackground => Color.FromArgb(32, 32, 32);
    public override Color MenuStripGradientBegin => Color.FromArgb(32, 32, 32);
    public override Color MenuStripGradientEnd => Color.FromArgb(32, 32, 32);
    public override Color ImageMarginGradientBegin => Color.FromArgb(32, 32, 32);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(32, 32, 32);
    public override Color ImageMarginGradientEnd => Color.FromArgb(32, 32, 32);
    public override Color MenuItemSelected => Color.FromArgb(50, 50, 50);
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(50, 50, 50);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(50, 50, 50);
    public override Color MenuItemBorder => Color.FromArgb(60, 60, 60);
    public override Color MenuItemPressedGradientBegin => Color.FromArgb(40, 40, 40);
    public override Color MenuItemPressedGradientEnd => Color.FromArgb(40, 40, 40);
    public override Color SeparatorDark => Color.FromArgb(50, 50, 50);
    public override Color SeparatorLight => Color.FromArgb(32, 32, 32);
}
