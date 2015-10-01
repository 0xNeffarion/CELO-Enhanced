using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CELO_Enhanced
{
    public class FloatingOSDWindow : FloatingWindow
    {
        #region Public Methods

        /// <summary>
        ///     Show given text on OSD-window
        /// </summary>
        /// <param name="pt">Top-left corner of text in screen coordinates</param>
        /// <param name="alpha">Transparency of text</param>
        /// <param name="textColor">Color of text</param>
        /// <param name="textFont">Font of text</param>
        /// <param name="showTimeMSec">How long text will be remain on screen, in millisecond</param>
        /// <param name="mode">Effect to be applied. Work only if <c>time</c> greater than 0</param>
        /// <param name="time">
        ///     Time, in milliseconds, for effect playing. If this equal to 0 <c>mode</c> ignored and text showed at
        ///     once
        /// </param>
        /// <param name="text">Text to display</param>
        public void Show(Point pt, byte alpha, Color textColor, Font textFont, int showTimeMSec, AnimateMode mode,
            uint time, string text)
        {
            if (_viewClock != null)
            {
                _viewClock.Stop();
                _viewClock.Dispose();
            }
            _brush = new SolidBrush(textColor);
            _textFont = textFont;
            _text = text;
            _mode = mode;
            _time = time;
            SizeF textArea;
            _rScreen = Screen.PrimaryScreen.Bounds;
            if (_stringFormat == null)
            {
                _stringFormat = new StringFormat();
                _stringFormat.Alignment = StringAlignment.Near;
                _stringFormat.LineAlignment = StringAlignment.Near;
                _stringFormat.Trimming = StringTrimming.EllipsisWord;
            }
            using (var bm = new Bitmap(Width, Height))
            using (var fx = Graphics.FromImage(bm))
                textArea = fx.MeasureString(text, textFont, _rScreen.Width, _stringFormat);
            Location = pt;
            Alpha = alpha;
            Size = new Size((int) Math.Ceiling(textArea.Width), (int) Math.Ceiling(textArea.Height));
            if (time > 0)
                ShowAnimate(mode, time);
            else
                base.Show();
            _viewClock = new Timer();
            _viewClock.Tick += viewTimer;
            _viewClock.Interval = showTimeMSec;
            _viewClock.Start();
        }

        #endregion

        #region Overrided Drawing & Path Creation

        protected override void PerformPaint(PaintEventArgs e)
        {
            if (Handle == IntPtr.Zero)
                return;
            var g = e.Graphics;
            if (_gp != null)
                _gp.Dispose();
            _gp = new GraphicsPath();
            _gp.AddString(_text, _textFont.FontFamily, (int) _textFont.Style, g.DpiY*_textFont.SizeInPoints/72, Bound,
                _stringFormat);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.FillPath(_brush, _gp);
        }

        #endregion

        #region Timers

        protected void viewTimer(object sender, EventArgs e)
        {
            _viewClock.Stop();
            _viewClock.Dispose();
            if (_time > 0)
                HideAnimate(_mode, _time);
            Close();
        }

        #endregion

        #region Variables

        private SolidBrush _brush;
        private StringFormat _stringFormat;
        private Rectangle _rScreen;
        private Timer _viewClock;
        private Font _textFont;
        private string _text;
        private AnimateMode _mode;
        private uint _time;
        private GraphicsPath _gp;

        #endregion
    }
}