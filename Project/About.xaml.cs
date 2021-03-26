using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace CELO_Enhanced {
    /// <summary>
    ///     Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window {
        public About() {
            InitializeComponent();
        }

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        private void imgDonate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.neffware.com");
        }

        private void imgNeffware_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Process.Start("https://github.com/0xNeffarion/CELO-Enhanced");
        }

        private void aboutWindow_Loaded(object sender, RoutedEventArgs e) {
        }

        private void imgCELO_MouseEnter(object sender, MouseEventArgs e) {
        }

        private void imgCELO_KeyDown(object sender, KeyEventArgs e) {
        }

        private void aboutWindow_KeyDown(object sender, KeyEventArgs e) {
        }
    }
}