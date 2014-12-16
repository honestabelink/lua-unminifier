using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace luaformatter
{
    public class listboxex : System.Windows.Forms.ListBox
    {

        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;

        private const int SB_LINELEFT = 0;
        private const int SB_LINERIGHT = 1;
        private const int SB_PAGELEFT = 2;
        private const int SB_PAGERIGHT = 3;
        private const int SB_THUMBPOSITION = 4;
        private const int SB_THUMBTRACK = 5;
        private const int SB_LEFT = 6;
        private const int SB_RIGHT = 7;
        private const int SB_ENDSCROLL = 8;

        private const int SIF_TRACKPOS = 0x10;
        private const int SIF_RANGE = 0x1;
        private const int SIF_POS = 0x4;
        private const int SIF_PAGE = 0x2;
        private const int SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS;

        private System.ComponentModel.Container components = null;

        private struct ScrollInfoStruct
        {
            public int cbSize;
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;
        }

        [Category("Action")]
        public event ScrollEventHandler Scrolled = null;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetScrollInfo(
            IntPtr hWnd, int n, ref ScrollInfoStruct lpScrollInfo);

        public listboxex()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        protected override void WndProc(ref System.Windows.Forms.Message msg)
        {
            if (msg.Msg == WM_VSCROLL)
            {
                if (Scrolled != null)
                {
                    ScrollInfoStruct si = new ScrollInfoStruct();
                    si.fMask = SIF_ALL;
                    si.cbSize = Marshal.SizeOf(si);
                    GetScrollInfo(msg.HWnd, 0, ref si);

                    if (msg.WParam.ToInt32() == SB_ENDSCROLL)
                    {
                        ScrollEventArgs sargs = new ScrollEventArgs(
                            ScrollEventType.EndScroll,
                            si.nPos);
                        Scrolled(this, sargs);
                    }
                }
            }
            base.WndProc(ref msg);
        }
    }
}
