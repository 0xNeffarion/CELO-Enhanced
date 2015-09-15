using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CELO_Enhanced
{
    /// <summary>
    /// Interaction logic for WhatsNew.xaml
    /// </summary>
    public partial class WhatsNew : Window
    {

        private readonly static string textUrl = "http://www.neffware.com/downloads/celo/news.txt";
        private string changes = "";
        public WhatsNew()
        {
            WebClient wc = new WebClient();
            using (wc)
            {
                changes = wc.DownloadString(textUrl);
            }
            InitializeComponent();
        }

        private void WhatsNewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            txtWN.Text = "What's new on version " + version;
            txtChanges.Text = changes;

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();

        }

        private void WhatsNewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            File.Delete(MainWindow._AssemblyDir + @"\data\news.txt");
        }
    }
}
