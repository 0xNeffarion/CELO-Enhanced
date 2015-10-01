using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace CELO_Enhanced
{
    public partial class Updater : Window
    {
        private static readonly string xmlUrl = "http://www.neffware.com/downloads/celo/app.xml";
        private readonly string baseDownloadURL = "http://www.neffware.com/downloads/celo/CELO_Enhanced_Setup.exe";
        private readonly WebClient webDownloader = new WebClient();
        private string FileName = "";

        public Updater()
        {
            InitializeComponent();
            webDownloader.DownloadFileCompleted += webDownloader_DownloadFileCompleted;
            webDownloader.DownloadProgressChanged += webDownloader_DownloadProgressChanged;
            var mainINI = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
        }

        public static String CheckForUpdates(Version appVersion)
        {
            try
            {
                if (Utilities.CheckInternet())
                {
                    if (Utilities.PingDomain("www.neffware.com"))
                    {
                        var wb = new WebClient();
                        String content;
                        using (wb)
                        {
                            content = wb.DownloadString(xmlUrl);
                        }
                        var rng = new Random();
                        var path = MainWindow._AssemblyDir + @"\541da5ax.xml";
                        File.Delete(path);
                        File.WriteAllText(path, content, Encoding.UTF8);
                        var doc = new XmlDocument();
                        doc.Load(path);
                        var XnList = doc.SelectNodes("Application");
                        var newVersion = new Version();
                        if (XnList != null)
                        {
                            foreach (XmlNode xnode in XnList)
                            {
                                newVersion = Version.Parse(xnode["Version"].InnerText);
                            }
                        }

                        File.Delete(path);
                        if (appVersion < newVersion)
                        {
                            return String.Format("{0}.{1}.{2}.{3}", newVersion.Major, newVersion.Minor, newVersion.Build,
                                newVersion.Revision);
                        }

                        return null;
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        private void webDownloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
        }

        private async void webDownloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                txt_status.Text = "Download complete, follow the installation to continue...";
                Utilities.showMessage(this,
                    "Download complete.\nPlease follow the installation to continue through the update process.",
                    "Update complete");
                Process.Start(FileName);
                await TaskEx.Delay(2500);
                Environment.Exit(0);
            }
            catch (Exception)
            {
            }
        }

        private void updaterWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Utilities.CheckInternet())
            {
                Utilities.showWarning(this, "No internet available, please connect your computer to the web.");
                Close();
            }
            DownloadSetup();
        }

        private void DownloadSetup()
        {
            try
            {
                var rn = new Random();
                FileName = Path.GetTempPath() + @"\tmp_celo_setup.exe";
                using (webDownloader)
                {
                    webDownloader.DownloadFileAsync(new Uri(baseDownloadURL), FileName);
                }
            }
            catch (Exception ex)
            {
                Utilities.showError(this, "An error occured while trying to download the new update.\n" + ex.StackTrace);
            }
        }
    }
}