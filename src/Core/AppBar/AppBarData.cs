using System;
using System.Collections.Generic;
using System.Text;

namespace Sidebar.Core
{
    internal struct AppBarData
    {
        internal int cbSize;
        internal int hWnd;
        internal int uCallBackMessage;
        internal int uEdge;
        internal Rect rc;
        internal int lParam;
    }
}
