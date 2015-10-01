using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for WhatsNew.xaml
    /// </summary>
    public partial class WhatsNew : Window
    {
        private static readonly string textUrl = "http://www.neffware.com/downloads/celo/news.txt";
        private readonly string changes = "";

        public WhatsNew()
        {
            var wc = new WebClient();
            using (wc)
            {
                changes = wc.DownloadString(textUrl);
            }
            InitializeComponent();
        }

        private void WhatsNewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            txtWN.Text = "What's new on version " + version;
            txtChanges.Text = changes;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WhatsNewWindow_Closing(object sender, CancelEventArgs e)
        {
            File.Delete(MainWindow._AssemblyDir + @"\data\news.txt");
        }
    }
}