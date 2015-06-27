using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        private void imgDonate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://www.neffware.com/index.php/support");
        }

        private void imgNeffware_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://www.neffware.com/projects/celo");
        }

        private void aboutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            txt_version.Content = "Version: " + version;
        }

        private void imgCELO_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void imgCELO_KeyDown(object sender, KeyEventArgs e)
        {
        }


        private void aboutWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                var bx = new InputBox("Debug", "Insert debug code:", "");
                bx.ShowDialog();

                if (bx.val == "feedfish2014")
                {
                    AllocConsole();
                }
            }
        }
    }
}