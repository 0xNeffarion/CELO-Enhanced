using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for Replay_Uploader.xaml
    /// </summary>
    public partial class Replay_Uploader : Window
    {
        private const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;
        private const int SET_FEATURE_ON_THREAD = 0x00000001;
        private const int SET_FEATURE_ON_PROCESS = 0x00000002;
        private const int SET_FEATURE_IN_REGISTRY = 0x00000004;
        private const int SET_FEATURE_ON_THREAD_LOCALMACHINE = 0x00000008;
        private const int SET_FEATURE_ON_THREAD_INTRANET = 0x00000010;
        private const int SET_FEATURE_ON_THREAD_TRUSTED = 0x00000020;
        private const int SET_FEATURE_ON_THREAD_INTERNET = 0x00000040;
        private const int SET_FEATURE_ON_THREAD_RESTRICTED = 0x00000080;
        private readonly BackgroundWorker bgWorker = new BackgroundWorker();
        private readonly BackgroundWorker bgWorkerUploader = new BackgroundWorker();
        private readonly Utilities.INIFile mainINI;
        private readonly string replayPath;
        private readonly WebBrowser wbLogin = new WebBrowser();
        private Boolean _pLoggedIn;
        private int autolog;

        public Replay_Uploader(string docPath, string filename, int game = 0)
        {
            mainINI = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
            bgWorker.DoWork += BgWorker_DoWork;
            int feature = FEATURE_DISABLE_NAVIGATION_SOUNDS;
            CoInternetSetFeatureEnabled(feature, SET_FEATURE_ON_PROCESS, true);
            replayPath = docPath + @"\playback\" + filename;
            InitializeComponent();
            if (game == 0)
            {
                Utilities.showError(this, "This feature is only available on Company of Heroes 2.");
                Close();
            }
            Console.WriteLine(replayPath);
        }

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        private static extern int CoInternetSetFeatureEnabled(
            int FeatureEntry,
            [MarshalAs(UnmanagedType.U4)] int dwFlags,
            bool fEnable);

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(log));
        }

        private void log()
        {
            if (IsLoaded)
            {
                try
                {
                    HtmlElementCollection elc = wbLogin.Document.GetElementsByTagName("input");
                    pgBar.Value = 50;
                    foreach (HtmlElement el in elc)
                    {
                        if (el.Id == "User_name")
                        {
                            el.SetAttribute("value", tBox_username.Text);
                        }
                        if (el.Id == "User_password")
                        {
                            el.SetAttribute("value", tBox_password.Password);
                        }
                        if (el.Id == "User_rememberMe")
                        {
                            el.SetAttribute("checked", "true");
                        }
                    }
                    HtmlElement ele2 = wbLogin.Document.GetElementsByTagName("input")["yt0"];
                    ele2.InvokeMember("click");
                    _pLoggedIn = true;
                }
                catch (Exception)
                {
                    log();
                }
            }
        }

        public static Task Delay(double milliseconds)
        {
            var tcs = new TaskCompletionSource<bool>();
            var timer = new Timer();
            timer.Elapsed += (obj, args) => { tcs.TrySetResult(true); };
            timer.Interval = milliseconds;
            timer.AutoReset = false;
            timer.Start();
            return tcs.Task;
        }

        private string ReadV(Utilities.INIFile inf, String section, String key)
        {
            try
            {
                return inf.IniReadValue(section, key);
            }
            catch (Exception)
            {
            }
            return null;
        }

        private void UploadWindow_Loaded(object sender, RoutedEventArgs e)
        {
            winHost.Child = wbLogin;
            wbLogin.DocumentCompleted += WbLogin_DocumentCompleted;
            wbLogin.ScriptErrorsSuppressed = true;

            wbLogin.Navigate("http://www.coh2.org/user/login");
            if (!Utilities.CheckInternet())
            {
                Utilities.showError(this,
                    "There is no network in order to upload replays\nPlease connect to the Internet");
                Close();
            }
            if (ReadV(mainINI, "ReplayManager", "RememberUser").ToLower() == "true")
            {
                if (!String.IsNullOrEmpty(ReadV(mainINI, "ReplayManager", "Username")))
                {
                    tBox_username.Text =
                        Utilities.SimpleTripleDES.Decrypt3DES(ReadV(mainINI, "ReplayManager", "Username"),
                            "xCb54nZs235mi8", true);
                }
                if (ReadV(mainINI, "ReplayManager", "RememberPass").ToLower() == "true")
                {
                    if (!String.IsNullOrEmpty(ReadV(mainINI, "ReplayManager", "Password")))
                    {
                        tBox_password.Password =
                            Utilities.SimpleTripleDES.Decrypt3DES(ReadV(mainINI, "ReplayManager", "Password"),
                                "xCb54nZs235mi8", true);
                    }
                    if (ReadV(mainINI, "ReplayManager", "AutoLogin").ToLower() == "true")
                    {
                        autolog = 1;
                        btnLogin_Click(btnLogin, null);
                    }
                }
            }
        }

        private int failsafe = 0;
        private async void WbLogin_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_pLoggedIn)
            {
                await TaskEx.Delay(500);
                if (wbLogin.Url.ToString() == "http://www.coh2.org/user/login")
                {
                    if (autolog == 0)
                    {
                        failsafe = 1;
                        pgBar.Value = 0;
                        _pLoggedIn = false;
                    }
                    else
                    {
                        autolog--;
                    }
                }
            }

            await TaskEx.Delay(500);
            if (wbLogin.Url.ToString() == "http://www.coh2.org/")
            {
                pgBar.Value = 35;
                groupBox2.IsEnabled = true;
                groupBox3.IsEnabled = true;
                pgBar.Value = 75;
                groupBox1.IsEnabled = false;
                pgBar.Value = 100;
                wbLogin.Navigate("http://www.coh2.org/replay/upload");
                pgBar.Value = 0;
                failsafe = 0;
                tBox_title.Focus();
            }
            else
            {
                if (failsafe == 1)
                {
                    Utilities.showError(this, "Username or Password are incorrect");
                }
            }
        }

        private async void UploadFile()
        {
            pgBar.Value = 0;
            HtmlElementCollection elc = wbLogin.Document.GetElementsByTagName("input");
            foreach (HtmlElement el in elc)
            {
                if (el.Id == "ytThreadReplay_replayFile")
                {
                    el.SetAttribute("value", replayPath);
                    pgBar.Value = 10;
                }
                if (el.Id == "Thread_title")
                {
                    el.SetAttribute("value", tBox_title.Text);
                    pgBar.Value = 25;
                }
            }
            HtmlElementCollection elc2 = wbLogin.Document.GetElementsByTagName("textarea");
            foreach (HtmlElement el in elc2)
            {
                if (el.Id == "Post_content")
                {
                    el.InnerText = tBox_comment.Text;
                    pgBar.Value = 45;
                }
            }
            HtmlElementCollection elements2 = wbLogin.Document.GetElementsByTagName("input");
            foreach (HtmlElement file in elements2)
            {
                if (file.Id == "ThreadReplay_replayFile")
                {
                    file.Focus();
                    Thread ts = new Thread(PopulateFile);
                    ts.SetApartmentState(ApartmentState.STA);
                    ts.Start();
                    file.InvokeMember("Click");
                    
                }
            }
            
            HtmlElementCollection elements = wbLogin.Document.GetElementsByTagName("form");
            pgBar.Value = 85;
            await TaskEx.Delay(1000);
            foreach (HtmlElement currentElement in elements)
            {
                if (currentElement.Id == "createReplay")
                {
                    currentElement.InvokeMember("submit");
                }
            }
            pgBar.Value = 100;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (Utilities.CheckInternet())
            {
                pgBar.Value = 0;
                _pLoggedIn = true;
                var sf = new List<string>
                {
                    "Connecting to COH2.ORG...",
                    "Settting up credentials...",
                    "Logging in..."
                };
                var LS = new LoadingScreen(this, sf, 4000, 5000);
                LS.ShowDialog();
                log();
            }
            else
            {
                Utilities.showError(this,
                    "There is no network in order to upload replays!\nPlease connect to the Internet!");
            }
        }

        private async void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            
            if (tBox_title.Text.Length <= 0 || tBox_comment.Text.Length <= 0)
            {
                Utilities.showError(this, "Title and comment is required for uploading");
                return;
            } 
            
            if (MessageBox.Show(this, "Are you sure you want to upload the replay file?", "Confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                await TaskEx.Delay(500);
                UploadFile();
                var LS = new LoadingScreen(this, new List<string> {"Uploading...", "Finalizing..."}, 5000, 7000);
                LS.ShowDialog();
                Thread.Sleep(500);
                if (MessageBox.Show(this, "Upload complete.\nDo you want to go to the replay page?", "Upload complete",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Process.Start(wbLogin.Url.ToString());
                    Close();
                }
                else
                {
                    Close();
                }
            }
        }

        [STAThread]
        private void PopulateFile()
        {
            Clipboard.Clear();
            Clipboard.SetText(replayPath);
            Thread.Sleep(500);
            SendKeys.SendWait("^v");
            Thread.Sleep(150);
            SendKeys.SendWait("{ENTER}");
            Clipboard.Clear();
        }

        private void groupBox2_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (groupBox2.IsEnabled)
            {
                tBox_title.Focus();
            }
        }

        private void tBox_username_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tBox_username.Text != "" && tBox_password.Password != "")
            {
                if (tBox_username.Text.Length >= 3 && tBox_password.Password.Length >= 3)
                {
                    btnLogin.IsEnabled = tBox_username.Text.Length < 40;
                }
                else
                {
                    btnLogin.IsEnabled = false;
                }
            }
            else
            {
                btnLogin.IsEnabled = false;
            }
        }

        private void tBox_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (tBox_username.Text != "" && tBox_password.Password != "")
            {
                if (tBox_username.Text.Length >= 3 && tBox_password.Password.Length >= 3)
                {
                    btnLogin.IsEnabled = tBox_username.Text.Length < 40;
                }
                else
                {
                    btnLogin.IsEnabled = false;
                }
            }
            else
            {
                btnLogin.IsEnabled = false;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}