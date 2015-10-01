using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CELO_Enhanced
{
    public class FloatingWindow : NativeWindow, IDisposable
    {
        #region #  Enums  #

        public enum AnimateMode
        {
            Blend,
            SlideRightToLeft,
            SlideLeftToRight,
            SlideTopToBottom,
            SlideBottmToTop,
            RollRightToLeft,
            RollLeftToRight,
            RollTopToBottom,
            RollBottmToTop,
            ExpandCollapse
        }

        #endregion

        #region #  Fields  #

        private bool _disposed;
        private byte _alpha = 250;
        private Size _size = new Size(250, 50);
        private Point _location = new Point(50, 50);

        #endregion

        #region #  Methods  #

        #region == Painting ==

        /// <summary>
        ///     Performs the painting of the window. Overide this method to provide custom painting
        /// </summary>
        /// <param name="e">A <see cref="PaintEventArgs" /> containing the event data.</param>
        protected virtual void PerformPaint(PaintEventArgs e)
        {
            using (var b = new LinearGradientBrush(Bound, Color.LightBlue, Color.DarkGoldenrod, 45f))
                e.Graphics.FillRectangle(b, Bound);
            e.Graphics.DrawString("Overide this PerformPaint method...",
                new Font(FontFamily.GenericSansSerif, 12f, FontStyle.Regular),
                new SolidBrush(Color.FromArgb(170, Color.Red)), new PointF(0f, 10f));
        }

        #endregion

        #region == Updating ==

        protected internal void Invalidate()
        {
            UpdateLayeredWindow();
        }

        private void UpdateLayeredWindow()
        {
            var bitmap1 = new Bitmap(Size.Width, Size.Height, PixelFormat.Format32bppArgb);
            using (var graphics1 = Graphics.FromImage(bitmap1))
            {
                Rectangle rectangle1;
                SIZE size1;
                POINT point1;
                POINT point2;
                BLENDFUNCTION blendfunction1;
                rectangle1 = new Rectangle(0, 0, Size.Width, Size.Height);
                PerformPaint(new PaintEventArgs(graphics1, rectangle1));
                var ptr1 = User32.GetDC(IntPtr.Zero);
                var ptr2 = Gdi32.CreateCompatibleDC(ptr1);
                var ptr3 = bitmap1.GetHbitmap(Color.FromArgb(0));
                var ptr4 = Gdi32.SelectObject(ptr2, ptr3);
                size1.cx = Size.Width;
                size1.cy = Size.Height;
                point1.x = Location.X;
                point1.y = Location.Y;
                point2.x = 0;
                point2.y = 0;
                blendfunction1 = new BLENDFUNCTION();
                blendfunction1.BlendOp = 0;
                blendfunction1.BlendFlags = 0;
                blendfunction1.SourceConstantAlpha = _alpha;
                blendfunction1.AlphaFormat = 1;
                User32.UpdateLayeredWindow(Handle, ptr1, ref point1, ref size1, ptr2, ref point2, 0, ref blendfunction1,
                    2); //2=ULW_ALPHA
                Gdi32.SelectObject(ptr2, ptr4);
                User32.ReleaseDC(IntPtr.Zero, ptr1);
                Gdi32.DeleteObject(ptr3);
                Gdi32.DeleteDC(ptr2);
            }
        }

        #endregion

        #region == Show / Hide ==

        /// <summary>
        ///     Shows the window. Position determined by Location property in screen coordinates
        /// </summary>
        public virtual void Show()
        {
            if (Handle == IntPtr.Zero) //if handle don't equal to zero - window was created and just hided
                CreateWindowOnly();
            User32.ShowWindow(Handle, User32.SW_SHOWNOACTIVATE);
        }

        /// <summary>
        ///     Shows the window.
        /// </summary>
        /// <param name="x">x-coordinate of window in screen coordinates</param>
        /// <param name="y">x-coordinate of window in screen coordinates</param>
        public virtual void Show(int x, int y)
        {
            _location.X = x;
            _location.Y = y;
            Show();
        }

        /// <summary>
        ///     Shows the window with animation effect. Position determined by Location property in screen coordinates
        /// </summary>
        /// <param name="mode">Effect to be applied</param>
        /// <param name="time">Time, in milliseconds, for effect playing</param>
        public virtual void ShowAnimate(AnimateMode mode, uint time)
        {
            uint dwFlag = 0;
            switch (mode)
            {
                case AnimateMode.Blend:
                    dwFlag = User32.AW_BLEND;
                    break;
                case AnimateMode.ExpandCollapse:
                    dwFlag = User32.AW_CENTER;
                    break;
                case AnimateMode.SlideLeftToRight:
                    dwFlag = User32.AW_HOR_POSITIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideRightToLeft:
                    dwFlag = User32.AW_HOR_NEGATIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideTopToBottom:
                    dwFlag = User32.AW_VER_POSITIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideBottmToTop:
                    dwFlag = User32.AW_VER_NEGATIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.RollLeftToRight:
                    dwFlag = User32.AW_HOR_POSITIVE;
                    break;
                case AnimateMode.RollRightToLeft:
                    dwFlag = User32.AW_HOR_NEGATIVE;
                    break;
                case AnimateMode.RollBottmToTop:
                    dwFlag = User32.AW_VER_NEGATIVE;
                    break;
                case AnimateMode.RollTopToBottom:
                    dwFlag = User32.AW_VER_POSITIVE;
                    break;
            }
            if (Handle == IntPtr.Zero)
                CreateWindowOnly();
            if ((dwFlag & User32.AW_BLEND) != 0)
                AnimateWithBlend(true, time);
            else
                User32.AnimateWindow(Handle, time, dwFlag);
        }

        /// <summary>
        ///     Shows the window with animation effect.
        /// </summary>
        /// <param name="x">x-coordinate of window in screen coordinates</param>
        /// <param name="y">x-coordinate of window in screen coordinates</param>
        /// <param name="mode">Effect to be applied</param>
        /// <param name="time">Time, in milliseconds, for effect playing</param>
        public virtual void ShowAnimate(int x, int y, AnimateMode mode, uint time)
        {
            _location.X = x;
            _location.Y = y;
            ShowAnimate(mode, time);
        }

        /// <summary>
        ///     Hides the window and release it's handle.
        /// </summary>
        public virtual Boolean Hide()
        {
            if (Handle == IntPtr.Zero)
                return false;
            User32.ShowWindow(Handle, User32.SW_HIDE);
            DestroyHandle();
            return true;
        }

        /// <summary>
        ///     Hides the window with animation effect and release it's handle.
        /// </summary>
        /// <param name="mode">Effect to be applied</param>
        /// <param name="time">Time, in milliseconds, for effect playing</param>
        public virtual void HideAnimate(AnimateMode mode, uint time)
        {
            if (Handle == IntPtr.Zero)
                return;
            uint dwFlag = 0;
            switch (mode)
            {
                case AnimateMode.Blend:
                    dwFlag = User32.AW_BLEND;
                    break;
                case AnimateMode.ExpandCollapse:
                    dwFlag = User32.AW_CENTER;
                    break;
                case AnimateMode.SlideLeftToRight:
                    dwFlag = User32.AW_HOR_POSITIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideRightToLeft:
                    dwFlag = User32.AW_HOR_NEGATIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideTopToBottom:
                    dwFlag = User32.AW_VER_POSITIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideBottmToTop:
                    dwFlag = User32.AW_VER_NEGATIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.RollLeftToRight:
                    dwFlag = User32.AW_HOR_POSITIVE;
                    break;
                case AnimateMode.RollRightToLeft:
                    dwFlag = User32.AW_HOR_NEGATIVE;
                    break;
                case AnimateMode.RollBottmToTop:
                    dwFlag = User32.AW_VER_NEGATIVE;
                    break;
                case AnimateMode.RollTopToBottom:
                    dwFlag = User32.AW_VER_POSITIVE;
                    break;
            }
            dwFlag |= User32.AW_HIDE;
            if ((dwFlag & User32.AW_BLEND) != 0)
                AnimateWithBlend(false, time);
            else
                User32.AnimateWindow(Handle, time, dwFlag);
            Hide();
        }

        /// <summary>
        ///     Close the window and destroy it's handle.
        /// </summary>
        public virtual void Close()
        {
            Hide();
            Dispose();
        }

        private void AnimateWithBlend(bool show, uint time)
        {
            var originalAplha = _alpha;
            var p = (byte) (originalAplha/(time/10));
            if (p == 0) p++;
            if (show)
            {
                _alpha = 0;
                UpdateLayeredWindow();
                User32.ShowWindow(Handle, User32.SW_SHOWNOACTIVATE);
            }
            for (var i = show ? (byte) 0 : originalAplha;
                (show ? i <= originalAplha : i >= (byte) 0);
                i += (byte) (p*(show ? 1 : -1)))
            {
                _alpha = i;
                UpdateLayeredWindow();
                if ((show && i > originalAplha - p) || (!show && i < p))
                    break;
            }
            _alpha = originalAplha;
            if (show)
                UpdateLayeredWindow();
        }

        private void CreateWindowOnly()
        {
            var params1 = new CreateParams();
            params1.Caption = "FloatingNativeWindow";
            var nX = _location.X;
            var nY = _location.Y;
            var screen1 = Screen.FromHandle(Handle);
            if ((nX + _size.Width) > screen1.Bounds.Width)
            {
                nX = screen1.Bounds.Width - _size.Width;
            }
            if ((nY + _size.Height) > screen1.Bounds.Height)
            {
                nY = screen1.Bounds.Height - _size.Height;
            }
            _location = new Point(nX, nY);
            var size1 = _size;
            var point1 = _location;
            params1.X = nX;
            params1.Y = nY;
            params1.Height = size1.Height;
            params1.Width = size1.Width;
            params1.Parent = IntPtr.Zero;
            var ui = User32.WS_POPUP;
            params1.Style = (int) ui;
            params1.ExStyle = User32.WS_EX_TOPMOST | User32.WS_EX_TOOLWINDOW | User32.WS_EX_LAYERED |
                              User32.WS_EX_NOACTIVATE | User32.WS_EX_TRANSPARENT;
            CreateHandle(params1);
            UpdateLayeredWindow();
        }

        #endregion

        #region == Other messages ==

        private void PerformWmPaint_WmPrintClient(ref Message m, bool isPaintMessage)
        {
            PAINTSTRUCT paintstruct1;
            RECT rect1;
            Rectangle rectangle1;
            paintstruct1 = new PAINTSTRUCT();
            var ptr1 = (isPaintMessage ? User32.BeginPaint(m.HWnd, ref paintstruct1) : m.WParam);
            rect1 = new RECT();
            User32.GetWindowRect(Handle, ref rect1);
            rectangle1 = new Rectangle(0, 0, rect1.right - rect1.left, rect1.bottom - rect1.top);
            using (var graphics1 = Graphics.FromHdc(ptr1))
            {
                var bitmap1 = new Bitmap(rectangle1.Width, rectangle1.Height);
                using (var graphics2 = Graphics.FromImage(bitmap1))
                    PerformPaint(new PaintEventArgs(graphics2, rectangle1));
                graphics1.DrawImageUnscaled(bitmap1, 0, 0);
            }
            if (isPaintMessage)
                User32.EndPaint(m.HWnd, ref paintstruct1);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 15) // WM_PAINT
            {
                PerformWmPaint_WmPrintClient(ref m, true);
                return;
            }
            if (m.Msg == 0x318) // WM_PRINTCLIENT
            {
                PerformWmPaint_WmPrintClient(ref m, false);
                return;
            }
            base.WndProc(ref m);
        }

        #endregion

        #region == Size and Location ==

        protected virtual void SetBoundsCore(int x, int y, int width, int height)
        {
            if (((X != x) || (Y != y)) || ((Width != width) || (Height != height)))
            {
                if (Handle != IntPtr.Zero)
                {
                    var num1 = 20;
                    if ((X == x) && (Y == y))
                    {
                        num1 |= 2;
                    }
                    if ((Width == width) && (Height == height))
                    {
                        num1 |= 1;
                    }
                    User32.SetWindowPos(Handle, IntPtr.Zero, x, y, width, height, (uint) num1);
                }
                else
                {
                    Location = new Point(x, y);
                    Size = new Size(width, height);
                }
            }
        }

        #endregion

        #endregion

        #region #  Properties  #

        /// <summary>
        ///     Get or set position of top-left corner of floating native window in screen coordinates
        /// </summary>
        public virtual Point Location
        {
            get { return _location; }
            set
            {
                if (Handle != IntPtr.Zero)
                {
                    SetBoundsCore(value.X, value.Y, _size.Width, _size.Height);
                    var rect = new RECT();
                    User32.GetWindowRect(Handle, ref rect);
                    _location = new Point(rect.left, rect.top);
                    UpdateLayeredWindow();
                }
                else
                {
                    _location = value;
                }
            }
        }

        /// <summary>
        ///     Get or set size of client area of floating native window
        /// </summary>
        public virtual Size Size
        {
            get { return _size; }
            set
            {
                if (Handle != IntPtr.Zero)
                {
                    SetBoundsCore(_location.X, _location.Y, value.Width, value.Height);
                    var rect = new RECT();
                    User32.GetWindowRect(Handle, ref rect);
                    _size = new Size(rect.right - rect.left, rect.bottom - rect.top);
                    UpdateLayeredWindow();
                }
                else
                {
                    _size = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the height of the floating native window
        /// </summary>
        public int Height
        {
            get { return _size.Height; }
            set { _size = new Size(_size.Width, value); }
        }

        /// <summary>
        ///     Gets or sets the width of the floating native window
        /// </summary>
        public int Width
        {
            get { return _size.Width; }
            set { _size = new Size(value, _size.Height); }
        }

        /// <summary>
        ///     Get or set x-coordinate of top-left corner of floating native window in screen coordinates
        /// </summary>
        public int X
        {
            get { return _location.X; }
            set { Location = new Point(value, Location.Y); }
        }

        /// <summary>
        ///     Get or set y-coordinate of top-left corner of floating native window in screen coordinates
        /// </summary>
        public int Y
        {
            get { return _location.Y; }
            set { Location = new Point(Location.X, value); }
        }

        /// <summary>
        ///     Get rectangle represented client area of floating native window in client coordinates(top-left corner always has
        ///     coord. 0,0)
        /// </summary>
        public Rectangle Bound
        {
            get { return new Rectangle(new Point(0, 0), _size); }
        }

        /// <summary>
        ///     Get or set full opacity(255) or full transparency(0) or any intermediate state for floating native window
        ///     transparency
        /// </summary>
        public byte Alpha
        {
            get { return _alpha; }
            set
            {
                if (_alpha == value) return;
                _alpha = value;
                UpdateLayeredWindow();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                DestroyHandle();
                _disposed = true;
            }
        }

        #endregion
    }

    #region #  Win32  #

    internal struct PAINTSTRUCT
    {
        public int fErase;
        public int fIncUpdate;
        public int fRestore;
        public IntPtr hdc;
        public Rectangle rcPaint;
        public int Reserved1;
        public int Reserved2;
        public int Reserved3;
        public int Reserved4;
        public int Reserved5;
        public int Reserved6;
        public int Reserved7;
        public int Reserved8;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SIZE
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TRACKMOUSEEVENTS
    {
        public uint cbSize;
        public uint dwFlags;
        public IntPtr hWnd;
        public uint dwHoverTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MSG
    {
        public IntPtr hwnd;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public int time;
        public int pt_x;
        public int pt_y;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    internal class User32
    {
        public const uint WS_POPUP = 0x80000000;
        public const int WS_EX_TOPMOST = 0x8;
        public const int WS_EX_TOOLWINDOW = 0x80;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_HIDE = 0;
        public const uint AW_HOR_POSITIVE = 0x1;
        public const uint AW_HOR_NEGATIVE = 0x2;
        public const uint AW_VER_POSITIVE = 0x4;
        public const uint AW_VER_NEGATIVE = 0x8;
        public const uint AW_CENTER = 0x10;
        public const uint AW_HIDE = 0x10000;
        public const uint AW_ACTIVATE = 0x20000;
        public const uint AW_SLIDE = 0x40000;
        public const uint AW_BLEND = 0x80000;
        // Methods
        private User32()
        {
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool AnimateWindow(IntPtr hWnd, uint dwTime, uint dwFlags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT pt);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DispatchMessage(ref MSG msg);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DrawFocusRect(IntPtr hWnd, ref RECT rect);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetFocus();

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern ushort GetKeyState(int virtKey);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMessage(ref MSG msg, int hWnd, uint wFilterMin, uint wFilterMax);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GetClientRect(IntPtr hWnd, [In, Out] ref RECT rect);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetWindow(IntPtr hWnd, int cmd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool HideCaret(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool InvalidateRect(IntPtr hWnd, ref RECT rect, bool erase);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr LoadCursor(IntPtr hInstance, uint cursor);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref RECT rect, int cPoints);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PeekMessage(ref MSG msg, int hWnd, uint wFilterMin, uint wFilterMax, uint wFlag);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ReleaseCapture();

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern uint SendMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int newLong);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int X, int Y, int Width, int Height,
            uint flags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetCapture(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int ShowWindow(IntPtr hWnd, short cmdShow);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref int bRetValue, uint fWinINI);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool TrackMouseEvent(ref TRACKMOUSEEVENTS tme);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool TranslateMessage(ref MSG msg);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
            IntPtr hdcSrc, ref POINT pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool UpdateWindow(IntPtr hwnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool WaitMessage();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool AdjustWindowRectEx(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);
    }

    internal class Gdi32
    {
        // Methods
        private Gdi32()
        {
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern int CombineRgn(IntPtr dest, IntPtr src1, IntPtr src2, int flags);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateBrushIndirect(ref LOGBRUSH brush);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateRectRgnIndirect(ref RECT rect);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetClipBox(IntPtr hDC, ref RECT rectBox);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PatBlt(IntPtr hDC, int x, int y, int width, int height, uint flags);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern int SelectClipRgn(IntPtr hDC, IntPtr hRgn);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LOGBRUSH
    {
        public uint lbStyle;
        public uint lbColor;
        public uint lbHatch;
    }

    #endregion
}