using System;
using System.Windows.Forms;

namespace Notepad;

public partial class Form1
{
    private ToolStripMenuItem lightThemeItem = null!;
    private ToolStripMenuItem darkThemeItem = null!;
    private ToolStripMenuItem systemThemeItem = null!;

    private void InitializeViewMenu()
    {
        viewMenu = new ToolStripMenuItem("&View");
        zoomMenu = new ToolStripMenuItem("&Zoom");

        var zoomInItem = new ToolStripMenuItem("Zoom &In", null, (s, e) => ZoomIn()) { ShortcutKeys = Keys.Control | Keys.Oemplus };
        var zoomOutItem = new ToolStripMenuItem("Zoom &Out", null, (s, e) => ZoomOut()) { ShortcutKeys = Keys.Control | Keys.OemMinus };
        var resetZoomItem = new ToolStripMenuItem("Restore Default &Zoom", null, (s, e) => ResetZoom()) { ShortcutKeys = Keys.Control | Keys.D0 };

        zoomMenu.DropDownItems.Add(zoomInItem);
        zoomMenu.DropDownItems.Add(zoomOutItem);
        zoomMenu.DropDownItems.Add(resetZoomItem);

        var themeMenu = new ToolStripMenuItem("&Theme");
        lightThemeItem = new ToolStripMenuItem("&Light", null, (s, e) => SetTheme(ThemeMode.Light));
        darkThemeItem = new ToolStripMenuItem("&Dark", null, (s, e) => SetTheme(ThemeMode.Dark));
        systemThemeItem = new ToolStripMenuItem("Use &System Setting", null, (s, e) => SetTheme(ThemeMode.System));

        themeMenu.DropDownItems.Add(lightThemeItem);
        themeMenu.DropDownItems.Add(darkThemeItem);
        themeMenu.DropDownItems.Add(systemThemeItem);

        viewMenu.DropDownItems.Add(zoomMenu);
        viewMenu.DropDownItems.Add(themeMenu);
    }

    private void SetTheme(ThemeMode mode)
    {
        currentTheme = mode;
        ApplyTheme();
    }

    private void UpdateThemeMenuChecks()
    {
        lightThemeItem.Checked = currentTheme == ThemeMode.Light;
        darkThemeItem.Checked = currentTheme == ThemeMode.Dark;
        systemThemeItem.Checked = currentTheme == ThemeMode.System;
    }

    private void ZoomIn()
    {
        if (currentZoomPercentage >= 500f) return;
        currentZoomPercentage += 10f;
        ApplyZoom();
    }

    private void ZoomOut()
    {
        if (currentZoomPercentage <= 20f) return;
        currentZoomPercentage -= 10f;
        ApplyZoom();
    }

    private void ResetZoom()
    {
        currentZoomPercentage = 100f;
        ApplyZoom();
    }

    private void ApplyZoom()
    {
        float targetSize = DefaultFontSize * (currentZoomPercentage / 100f);
        if (targetSize < 1f) targetSize = 1f;

        globalFont = new Font(globalFont.FontFamily, targetSize, globalFont.Style);

        foreach (NotepadTab tab in tabControl.TabPages)
        {
            tab.TextBox.Font = globalFont;
        }
        UpdateStatusBar();
    }
}
