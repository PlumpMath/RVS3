using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RVS3
{
    static class ExtensionsForWPF
    {
        public static System.Drawing.Rectangle GetScreen(System.Windows.Window window)
        {
            return System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(window).Handle).Bounds;
        }
    }
}
