using System;
using System.Collections.Generic;
using System.Text;

namespace Sidebar
{
    internal struct AppBarData
    {
        internal int cbSize;
        internal int hWnd;
        internal int uCallBackMessage;
        internal int uEdge;
        internal RECT rc;
        internal int lParam;
    }
}
