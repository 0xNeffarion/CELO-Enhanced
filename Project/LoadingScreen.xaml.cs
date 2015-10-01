using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for LoadingScreen.xaml
    /// </summary>
    public partial class LoadingScreen : Window
    {
        private readonly DispatcherTimer _chek = new DispatcherTimer(DispatcherPriority.Background);
        private readonly List<String> _cnt = new List<string>();
        private readonly DispatcherTimer _end = new DispatcherTimer(DispatcherPriority.Background);
        private readonly DispatcherTimer _med = new DispatcherTimer(DispatcherPriority.Background);
        private readonly DispatcherTimer _tim = new DispatcherTimer(DispatcherPriority.Background);
        private readonly Func<Boolean> CheckEnd;
        private readonly Boolean valToCheck;
        private int _current;

        public LoadingScreen(Window win, List<String> LoadList, int min, int max)
        {
            InitializeComponent();
            _cnt = LoadList;
            var rng = new Random();
            var val = rng.Next(min, max + 1);
            var eachVal = val/LoadList.Count;
            _tim.Interval = TimeSpan.FromMilliseconds(val);
            _tim.IsEnabled = false;

            Owner = win;
            _med.Interval = TimeSpan.FromMilliseconds(eachVal);
            _med.Tick += _med_Tick;
            _tim.Tick += _tim_Tick;
        }

        public LoadingScreen(Window win, string message)
        {
            InitializeComponent();
            _cnt.Add(message);
            _current = 0;
            Owner = win;
        }

        public LoadingScreen(double x, double y, List<String> LoadList, int min, int max)
        {
            InitializeComponent();
            Left = x;
            Top = y;
            _cnt = LoadList;

            var rng = new Random();
            var val = rng.Next(min, max + 1);
            var eachVal = val/LoadList.Count;
            _tim.Interval = TimeSpan.FromMilliseconds(val);
            _tim.IsEnabled = false;

            //Owner = win;
            _med.Interval = TimeSpan.FromMilliseconds(eachVal);
            _med.Tick += _med_Tick;
            _tim.Tick += _tim_Tick;
        }

        public LoadingScreen(String str, ref Boolean val)
        {
            InitializeComponent();
            valToCheck = val;
            _chek.Tick += _chek_Tick;
            _chek.Interval = new TimeSpan(0, 0, 0, 1);
            _chek.IsEnabled = true;
            _cnt = new List<string> {str};
        }

        public LoadingScreen(Window win, String LoadList, Func<Boolean> meth)
        {
            InitializeComponent();
            _cnt = new List<string> {LoadList};
            CheckEnd = meth;
            Owner = win;
            _end.IsEnabled = true;
            _end.Tick += _end_Tick;
            _end.Interval = TimeSpan.FromMilliseconds(1500);
        }

        private void _chek_Tick(object sender, EventArgs e)
        {
            if (valToCheck)
            {
            }
            else
            {
                Close();
            }
        }

        private void _end_Tick(object sender, EventArgs e)
        {
            if (CheckEnd())
            {
                Close();
            }
        }

        private void _med_Tick(object sender, EventArgs e)
        {
            _current++;

            try
            {
                if (_cnt[_current - 1] != null)
                {
                    if (_current <= _cnt.Count)
                    {
                        text_load.Content = _cnt[_current - 1];
                    }
                }
            }
            catch (Exception)
            {
                _med.IsEnabled = false;
            }
        }

        private void LoadingScreenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _tim.IsEnabled = true;
            _med.IsEnabled = true;
            text_load.Content = _cnt[_current];
        }

        private void _tim_Tick(object sender, EventArgs e)
        {
            _tim.IsEnabled = false;
            Close();
        }
    }
}