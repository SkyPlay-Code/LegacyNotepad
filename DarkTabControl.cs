using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Notepad;

public class DarkTabControl : TabControl
{
    private const int TCM_FIRST = 0x1300;
    private const int TCM_ADJUSTRECT = TCM_FIRST + 40; // 0x1328

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    protected override void WndProc(ref Message m)
    {
        // Let the native control calculate the display rect first
        base.WndProc(ref m);

        if (m.Msg == TCM_ADJUSTRECT)
        {
            // Only adjust when calculating the Display Area from the Window rect (WParam == FALSE)
            if (m.WParam == IntPtr.Zero && m.LParam != IntPtr.Zero)
            {
                RECT rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT))!;

                // Expand the calculated display area slightly so the child tab pages
                // expand and cover the native default white borders of the TabControl.
                rc.Left -= 4;
                rc.Top -= 4;
                rc.Right += 4;
                rc.Bottom += 4;

                Marshal.StructureToPtr(rc, m.LParam, true);
            }
        }
    }
}
