using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using HtmlAgilityPack;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace CELO_Enhanced
{
    public partial class MainWindow : Window
    {
        private readonly Utilities.INIFile cfg;
        

        public MainWindow()
        {
            InitializeComponent();
            App.Current.MainWindow = this;
            cfg = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
        }


        #region Critical Match Variables

        private bool _isListWritten;
        private int _lastLine;
        private bool _matchBeingPlayed;
        private int _stopPoint;
        private int currPlayer;


        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr windowHandle, out int processId);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(int hWnd, StringBuilder text, int count);

        private string GetActiveWindowTitle()
        {
            IntPtr activeWindowId = GetForegroundWindow();
            if (activeWindowId.Equals(0))
            {
                return null;
            }
            int processId;
            GetWindowThreadProcessId(activeWindowId, out processId);
            if (processId == 0)
            {
                return null;
            }
            Process ps = Process.GetProcessById(processId);
            String title = null;
            if (!string.IsNullOrEmpty(ps.MainWindowTitle))
            {
                title = ps.MainWindowTitle;
            }

            if (string.IsNullOrEmpty(title))
            {
                const int Count = 1024;
                var sb = new StringBuilder(Count);
                GetWindowText((int) activeWindowId, sb, Count);
                title = sb.ToString();
            }
            return title;
        }

        private void Celo_Main_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        #endregion

        #region Timers

        private DispatcherTimer _cpmTimer;
        private DispatcherTimer _pingTimer;
        private DispatcherTimer _readerTimer;
        private DispatcherTimer _logTimer;
        private DispatcherTimer _updTimer;
        private int mins;
        private int ping;
        private Thread th;

        private void LoadTimers()
        {
            
            _cfgTimerInt = 2200;
            logFile.WriteLine("MAIN WINDOW - Loading reader timer");
            _readerTimer = new DispatcherTimer(DispatcherPriority.Background);
            _readerTimer.Interval = TimeSpan.FromMilliseconds(_cfgTimerInt);
            _readerTimer.IsEnabled = false;
            _readerTimer.Tick += _readerTimer_Tick;
            logFile.WriteLine("MAIN WINDOW - Loaded reader timer");
            logFile.WriteLine("MAIN WINDOW - Loading ping timer");
            _pingTimer = new DispatcherTimer(DispatcherPriority.ContextIdle);
            _pingTimer.Interval = new TimeSpan(0, 0, 10);
            _pingTimer.IsEnabled = false;
            _pingTimer.Tick += _pingTimer_Tick;
            logFile.WriteLine("MAIN WINDOW - Loaded ping timer");
            logFile.WriteLine("MAIN WINDOW - Loading cpm timer");
            _cpmTimer = new DispatcherTimer(DispatcherPriority.Background);
            _cpmTimer.IsEnabled = false;
            _cpmTimer.Interval = new TimeSpan(0, 1, 0);
            _cpmTimer.Tick += _cpmTimer_Tick;
            logFile.WriteLine("MAIN WINDOW - Loaded cpm timer");
            logFile.WriteLine("MAIN WINDOW - Loading update timer");
            _updTimer = new DispatcherTimer(DispatcherPriority.ContextIdle);
            _updTimer.IsEnabled = _cfgCheckUpdates;
            _updTimer.Interval = new TimeSpan(0,10,0);
            _updTimer.Tick += _updTimer_Tick;
            logFile.WriteLine("MAIN WINDOW - Loaded update timer");
            logFile.WriteLine("MAIN WINDOW - Loading log timer");
            _logTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            _logTimer.IsEnabled = true;
            _logTimer.Interval = new TimeSpan(0,0,0,30);
            _logTimer.Tick += _logTimer_Tick;
            logFile.WriteLine("MAIN WINDOW - Loaded log timer");

        }
        
        private async void _logTimer_Tick(object sender, EventArgs e)
        {
            logFile.WriteLine("===== LOG TIMER START =====");
            var temp = cpuCounter.NextValue();
            await TaskEx.Delay(1000);
            logFile.WriteLine("Total Memory: " + (((new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory)/1024)/1024) + " MBytes");
            logFile.WriteLine("Used Memory: " + ((Process.GetCurrentProcess().WorkingSet64 / 1024)/1024) + " MBytes");
            logFile.WriteLine("Free Memory: " + ramCounter.NextValue() + " MBytes");
            logFile.WriteLine("CPU Usage: " + Math.Round(cpuCounter.NextValue()) + "% ");
            logFile.WriteLine("===== LOG TIMER ENDED =====");

        }

        private void _updTimer_Tick(object sender, EventArgs e)
        {
            if (_cfgCheckUpdates)
            {
                
                if (mnuNewUpdate.Visibility != Visibility.Visible)
                {
                    logFile.WriteLine("MAIN WINDOW - UPDATE TIMER - Checking for updates");
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                    Version version = Version.Parse(fvi.FileVersion);
                    String response = Updater.CheckForUpdates(version);
                    if (response != null)
                    {
                        logFile.WriteLine("MAIN WINDOW - UPDATE TIMER - Update found!");
                        mnuNewUpdate.Visibility = Visibility.Visible;
                        mnuNewUpdate.Opacity = 1;
                        mnuNewUpdate.IsEnabled = true;
                    }
                }
            }

        }

        private void _cpmTimer_Tick(object sender, EventArgs e)
        {
            ++mins;
            if (_cfgShowCPM)
            {
                game_cpm.Content = "CPM: " + (clicks/mins);
            }
            else
            {
                game_cpm.Content = "CPM: Disabled";
            }
        }

        private void _pingTimer_Tick(object sender, EventArgs e)
        {
            if (_cfgShowPing)
            {
                th = new Thread(RetrievePing);
                th.Start();
            }
            else
            {
                game_ping.Content = "Battle-Servers Ping: Disabled";
            }
        }

        private async void _readerTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                switch (_gameSelected)
                {
                    case 0:
                        if (IsGameRunning(0))
                        {
                            if (await CopyLog())
                            {
                                if (!_matchBeingPlayed)
                                {
                                    setStatus("Waiting for a match to start");
                                    FindMatch(0);
                                }

                                if (_matchBeingPlayed && _isListWritten == false)
                                {
                                    setStatus("A match has been found");
                                    playerList.ItemsSource = _players;
                                    repeater = true;
                                    SetUpInfo(0, true);
                                    }
                                if (_matchBeingPlayed && _isListWritten)
                                {
                                    if (CheckEnd(0))
                                    {
                                        setStatus("Current match ended");
                                        FactorCreator();
                                        SetUpInfo(0, false);
                                        setStatus("Waiting for the next match");
                                        repeater = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            setStatus("Company of Heroes is not running!");
                            setStatus("Waiting for the game to start");
                        }
                        break;
                    case 1:
                        if (IsGameRunning(1))
                        {
                            
                            if (await CopyLog())
                            {
                                if (!_matchBeingPlayed)
                                {
                                    setStatus("Waiting for a match to start");
                                    FindMatch(1);
                                }
                                if (_matchBeingPlayed && _isListWritten == false)
                                {
                                    _isListWritten = true;
                                    setStatus("A match has been found");
                                    playerList.ItemsSource = _players;
                                    SetUpInfo(1, true);
                                    setStatus("Information is being parsed...");
                                }
                                if (_matchBeingPlayed && _isListWritten)
                                {
                                    if (CheckEnd(1))
                                    {
                                        setStatus("Current match ended");
                                        FactorCreator();
                                        SetUpInfo(1, false);
                                        setStatus("Waiting for the next match");
                                        notificationTooggle = 0;
                                        repeater = true;
                                    }
                                    

                                }
                            }
                        }
                        else
                        {
                            setStatus("Company of Heroes 2 is not running!");
                            setStatus("Waiting for the game to start");
                        }
                        break;
                    default:
                        _gameSelected = 1;
                        break;
                }
            }
            catch (Exception)
            {
                
            }
        }

        #endregion

        #region Configurations

        private bool _cfgCheckUpdates;
        private bool _cfgCleanList;
        private string _cfgDocPath = "";
        private string _cfgGamePath = "";
        private bool _cfgHistoryEnabled;
        private bool _cfgLsdEnabled;
        private string _cfgLsdOutput = "";
        private bool _cfgPlaySound;
        private bool _cfgShowCPM = true;
        private bool _cfgShowPing = true;
        private bool _cfgShowTime = true;
        private bool _cfgStartGW;
        private int _cfgTimerInt = 1500;
        private bool _cfgWindowTop;
        private int _gameSelected = 1;

        private string ReadV(String section, String key)
        {
            try
            {
                return cfg.IniReadValue(section, key);
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION Reading Config: " + ex.ToString());
            }
            return null;
        }

        private void WriteV(String section, String key, String value)
        {
            try
            {
                cfg.IniWriteValue(section, key, value);
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION Writing Config: " + ex.ToString());
            }
        }

        private Boolean GetConfigs()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini"))
            {
                _gameSelected = Int32.Parse(ReadV("Essencial", "Game"));
                _cfgDocPath = ReadV("Essencial", "CoH" + (_gameSelected + 1) + "_DocPath");
                _cfgGamePath = ReadV("Essencial", "CoH" + (_gameSelected + 1) + "_GamePath");
                _cfgCleanList = ReadV("Game_Watcher", "CleanPlayers").ToLower() == "true";
                _cfgCheckUpdates = ReadV("Main", "CheckForUpdates").ToLower() == "true";
                _cfgStartGW = ReadV("Game_Watcher", "AutoStart").ToLower() == "true";
                Topmost = ReadV("Game_Watcher", "WindowTop").ToLower() == "true";
                _cfgPlaySound = ReadV("Game_Watcher", "PlaySound").ToLower() == "true";
                _cfgShowCPM = ReadV("Game_Watcher", "Show_CPM").ToLower() == "true";
                _cfgShowPing = ReadV("Game_Watcher", "Show_Ping").ToLower() == "true";
                _cfgShowTime = ReadV("Game_Watcher", "Show_Time").ToLower() == "true";
                _cfgHistoryEnabled = ReadV("Match_History_Viewer", "Enabled").ToLower() == "true";
                _cfgLsdEnabled = ReadV("Livestream_Displayer", "Enabled").ToLower() == "true";
                _cfgLsdOutput = ReadV("Livestream_Displayer", "OutputFolder");
                _cfgWindowTop = ReadV("Game_Watcher", "WindowTop").ToLower() == "true";
                if (_cfgWindowTop)
                {
                    mnuItemWindowTop.IsChecked = true;
                }
                else
                {
                    mnuItemWindowTop.IsChecked = false;
                }
                return true;
            }
            return false;
        }

        #endregion

        #region Load Main

        private Utilities.Log logFile = new Utilities.Log(AppDomain.CurrentDomain.BaseDirectory + @"\logs");
        PerformanceCounter cpuCounter = new PerformanceCounter();
        PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        private void Load_Essential()
        {
            logFile.WriteLine("MAIN WINDOW - START Loading essential");
            pLoadpic.Image = Properties.Resources.preLoader;
            logFile.WriteLine("MAIN WINDOW - START Loading configs");
            if (!GetConfigs())
            {
                MessageBox.Show(this, "There was an error loading the configuration files",
                    "Error loading configuration",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                logFile.WriteLine("MAIN WINDOW - Configs ERROR");
            }
            logFile.WriteLine("MAIN WINDOW - Game: " + _gameSelected);
            switch (_gameSelected)
            {
                case 0:
                    status_gameName.Content = "Company of Heroes";
                    status_gameIcon.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/coh1_icon.png"));
                    break;
                case 1:
                    status_gameName.Content = "Company of Heroes 2";
                    status_gameIcon.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/coh2_icon.png"));
                    break;
            }



            logFile.WriteLine("MAIN WINDOW - START Loading timers");
            LoadTimers();
            logFile.WriteLine("MAIN WINDOW - ENDED Loading timers");

            if (_cfgStartGW)
            {
                ToggleGW();
            }

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            PassAgeGateCookie();
            logFile.WriteLine("MAIN WINDOW - ENDED Loading essential");

            
        }

        private void Celo_Main_Loaded(object sender, RoutedEventArgs e)
        {
            logFile.WriteLine("MAIN WINDOW - STARTED");
            try
            {
                logFile.WriteLine("MAIN WINDOW - Interface FPS => 60");
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                        typeof(Timeline),
                        new FrameworkPropertyMetadata { DefaultValue = 30 }
                    );
                logFile.WriteLine("MAIN WINDOW - Interface FPS => 30");
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION: " + ex.ToString());
            }
            Load_Essential();

            
        }

        #endregion

        #region Menus

        private void mnuMHV_Click(object sender, RoutedEventArgs e)
        {
            var mhv = new MatchHistoryViewer(_gameSelected, "");
            mhv.ShowDialog();
        }

        private void mnu_ahk_Click(object sender, RoutedEventArgs e)
        {
            var hk = new HotKeyGen();
            hk.ShowDialog();
        }

        private void mnuHelp_Click(object sender, RoutedEventArgs e)
        {
            var bt = new About();
            bt.ShowDialog();
        }

        private void mnuReplayManager_Click(object sender, RoutedEventArgs e)
        {
            var rep = new ReplayManager(_cfgDocPath, _cfgGamePath, _gameSelected);
            rep.ShowDialog();
        }

        private void mnuRestart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void mnuItemWindowTop_Click(object sender, RoutedEventArgs e)
        {
            if (mnuItemWindowTop.IsChecked)
            {
                Topmost = true;
                WriteV("Game_Watcher", "WindowTop", "true");
            }
            else
            {
                Topmost = false;
                WriteV("Game_Watcher", "WindowTop", "false");
            }
        }

        private void playerList_MouseEnter(object sender, MouseEventArgs e)
        {
            if (playerList.Items.Count > 0 && playerList.SelectedIndex != -1)
            {
                mnu_p_Copy.IsEnabled = true;
                mnu_p_Open.IsEnabled = true;
            }
            else
            {
                mnu_p_Copy.IsEnabled = false;
                mnu_p_Open.IsEnabled = false;
            }
        }

        private void mnu_p_copyNick_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex != -1)
            {
                var n = (Player) playerList.SelectedItem;
                Clipboard.SetText(n.Nickname);
            }
        }

        private void mnu_p_copyRank_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex != -1)
            {
                var n = (Player) playerList.SelectedItem;
                Clipboard.SetText(n.Ranking);
            }
        }

        private void mnu_p_copyTimePlayed_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex != -1)
            {
                var n = (Player) playerList.SelectedItem;
                Clipboard.SetText(n.TimePlayed.ToString());
            }
        }

        private void mnu_p_copyLevel_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex != -1)
            {
                var n = (Player) playerList.SelectedItem;
                Clipboard.SetText(n.Level);
            }
        }

        private void mnu_p_copyID_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex != -1)
            {
                var n = (Player) playerList.SelectedItem;
                Clipboard.SetText(n.SteamID.ToString());
            }
        }

        private void mnu_p_open_coh2org_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex != -1)
            {
                var n = (Player) playerList.SelectedItem;
                Process.Start("http://www.coh2.org/ladders/playercard/steamid/" + n.SteamID);
            }
        }

        private void playerList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (playerList.Items.Count > 0 && playerList.SelectedIndex != -1)
            {
                mnu_p_Copy.IsEnabled = true;
                mnu_p_Open.IsEnabled = true;
            }
            else
            {
                mnu_p_Copy.IsEnabled = false;
                mnu_p_Open.IsEnabled = false;
            }
        }

        private void mnu_p_open_coh_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex != -1)
            {
                var n = (Player) playerList.SelectedItem;
                Process.Start("http://www.companyofheroes.com/leaderboards#profile/steam/" + n.SteamID);
            }
        }

        private void mnu_p_open_steampage_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex != -1)
            {
                var n = (Player) playerList.SelectedItem;
                Process.Start("http://steamcommunity.com/profiles/" + n.SteamID);
            }
        }

        private void playerList_MouseMove(object sender, MouseEventArgs e)
        {
            if (playerList.Items.Count > 0 && playerList.SelectedIndex != -1)
            {
                mnu_p_Copy.IsEnabled = true;
                mnu_p_Open.IsEnabled = true;
            }
            else
            {
                mnu_p_Copy.IsEnabled = false;
                mnu_p_Open.IsEnabled = false;
            }
        }

        private void mnuPref_Click(object sender, RoutedEventArgs e)
        {
            var pref = new Preferences();
            pref.ShowDialog();
            GetConfigs();
            switch (_gameSelected)
            {
                case 0:
                    status_gameName.Content = "Company of Heroes";
                    status_gameIcon.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/coh1_icon.png"));
                    break;
                case 1:
                    status_gameName.Content = "Company of Heroes 2";
                    status_gameIcon.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/coh2_icon.png"));
                    break;
            }
        }

        private void mnuCheckUpd_Click(object sender, RoutedEventArgs e)
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Version version = Version.Parse(fvi.FileVersion);
            String response = Updater.CheckForUpdates(version);
            if (response != null)
            {
                if (MessageBox.Show(this,
                    "A new version is available for download (" + response + ")\nDo you wish to update CELO?",
                    "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                    MessageBoxResult.Yes)
                {
                    var dp = new Updater();
                    dp.ShowDialog();
                }
            }
            else
            {
                Utilities.showMessage(this, "CELO Enhanced is up-to-date.", "Update not available");
            }
        }

        private void mnuNewUpdate_Click(object sender, RoutedEventArgs e)
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Version version = Version.Parse(fvi.FileVersion);
            String response = Updater.CheckForUpdates(version);
            if (response != null)
            {
                if (MessageBox.Show(this,
                    "A new version is available for download (" + response + ")\nDo you wish to update CELO?",
                    "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                    MessageBoxResult.Yes)
                {
                    var dp = new Updater();
                    dp.ShowDialog();
                }
            }
            else
            {
                Utilities.showMessage(this, "CELO Enhanced is up-to-date.", "Update not available");
            }
        }

        #endregion

        #region Buttons

        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        private void btn_GameWatcher_Click(object sender, RoutedEventArgs e)
        {
            ToggleGW();
        }

        private void ToggleGW()
        {
            if (IsLoaded)
            {
                if (btn_GameWatcher.Tag.ToString() == "en")
                {
                    if (_cfgPlaySound)
                    {
                        int NewVolume = (1100);
                        uint NewVolumeAllChannels = (((uint) NewVolume & 0x0000ffff) | ((uint) NewVolume << 16));
                        waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
                        var uri = new Uri(@"pack://application:,,,/Resources/beep_01.wav");
                        var player = new SoundPlayer(Application.GetResourceStream(uri).Stream);
                        player.Play();
                    }
                    _readerTimer.IsEnabled = true;
                    LoadFormHost.Visibility = Visibility.Visible;

                    btn_GameWatcher.Tag = "dis";
                    txt_GameWatcher.Text = "Stop Game Watcher";
                }
                else
                {
                    _readerTimer.IsEnabled = false;
                    btn_GameWatcher.Tag = "en";
                    LoadFormHost.Visibility = Visibility.Hidden;
                    txt_GameWatcher.Text = "Start Game Watcher";
                }
            }
        }

        private void btn_ReplayManager_Click(object sender, RoutedEventArgs e)
        {
            var rep = new ReplayManager(_cfgDocPath, _cfgGamePath, _gameSelected);
            rep.Owner = this;
            rep.ShowDialog();
        }

        private void mnu_lsd_Click(object sender, RoutedEventArgs e)
        {
            var lsd = new LivestreamDisplayer();
            lsd.ShowDialog();
        }

        #endregion

        #region Main Functions

        public enum Teams
        {
            Axis,
            Allies
        }

        public static readonly String _AssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly String _copyLogPath = AppDomain.CurrentDomain.BaseDirectory + @"\data\log.tempf";
        private readonly ObservableCollection<Player> _players = new ObservableCollection<Player>();
        private readonly MouseHookListener mhl = new MouseHookListener(new GlobalHooker());
        private WebBrowser webFlags;
        private Thread InfoThread;
        private List<String> _logContent = new List<String>();
        private long clicks;
        private int notificationStop = 0, notificationTooggle = 0;
        private Boolean isMakingList;
        private Boolean repeater = true;
        private int curFlag = 0;
        private Boolean IsGameRunning(int game)
        {
            switch (game)
            {
                case 0:

                    foreach (Process process in Process.GetProcesses())
                    {
                        if (process.ProcessName.Equals("RelicCOH"))
                        {
                            return true;
                        }
                    }
                    break;
                case 1:
                    foreach (Process process in Process.GetProcesses())
                    {
                        if (process.ProcessName.Equals("RelicCoH2"))
                        {
                            return true;
                        }
                    }
                    break;
                default:
                    foreach (Process process in Process.GetProcesses())
                    {
                        if (process.ProcessName.Equals("RelicCoH2"))
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }

        private async Task<Boolean> CopyLog()
        {
            Boolean res = await TaskEx.Run(() => CopyLogTask());
            return res;
        }

        private Boolean CopyLogTask()
        {
            try
            {
                File.Copy(_cfgDocPath + @"\warnings.log", _copyLogPath, true);
                setLogContents(_gameSelected);
                return true;
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION Copying log: " + ex.ToString());
                return false;
            }
            
        }

        private Boolean CheckNotifications()
        {
            if (notificationTooggle == 0)
            {
                for (int index = 0; index < _logContent.Count; index++)
                {
                    var log = _logContent[index];
                    String str = log.Split(new[] { '.' })[0];
                    try
                    {
                        if (Regex.IsMatch(str.Split(new[] { ':' })[0], @"^\d+$"))
                        {
                            if (CheckTime(Int32.Parse(str.Split(new[] { ':' })[0]),
                                Int32.Parse(str.Split(new[] { ':' })[1])))
                            {
                                if (log.Contains("WorldwideNotifier.cpp"))
                                {
                                    notificationStop = index;
                                    return true;
                                }
                            }
                        }
                    }
                    catch (FormatException)
                    {
                    }
                }
            }
            return false;
        }

        private void ExecuteNotifications()
        {
            for (int i = notificationStop - 10; i < _logContent.Count; i++)
            {
                if (_logContent[i].Contains("RNT_StatsUpdate:"))
                {
                    long steamIDNotification = 0;
                    string[] strArr1 = Regex.Split(_logContent[i], "],");
                    string sID = strArr1[0].Split(new char[] { '/' })[2];
                    steamIDNotification = Int64.Parse(sID);
                    string RankAfter = (Regex.Split(_logContent[i], "ranking=")[1]).Trim();
                    int z = 0;
                    foreach (var player in _players)
                    {
                        if (player.SteamID == steamIDNotification)
                        {
                            if (Int32.Parse(RankAfter) >= 1)
                            {
                                _players[z].RankingAfter = " ↦ " + RankAfter;
                            }
                            else
                            {
                                _players[z].RankingAfter = " ↦ " + "Unranked";
                            }

                        }
                        z++;
                    }
                }
            }
            notificationTooggle = 1;

        }

        private void ClearList()
        {
            if (_players.Count > 0)
            {
                try
                {
                    _players.Clear();
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(playerList.ItemsSource);
                    if (view.GroupDescriptions != null)
                        view.GroupDescriptions.Clear();
                    if (view.SortDescriptions != null)
                        view.SortDescriptions.Clear();
                }
                catch (Exception ex)
                {
                    logFile.WriteLine("EXCEPTION Clearing list: " + ex.ToString());

                }
            }
        }

        private Boolean CheckTime(int hour, int minute)
        {
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            var gameTime = new TimeSpan(hour, minute, 00);
            TimeSpan gameTimeLess = gameTime.Subtract(new TimeSpan(0, 2, 0));
            TimeSpan gameTimeMore = gameTime.Add(new TimeSpan(0, 2, 0));
            if (gameTimeLess <= currentTime && gameTimeMore >= currentTime)
            {
                return true;
            }
            return false;
        }

        private void CleanLSD()
        {
            if (_cfgLsdOutput != "")
            {
                if (_cfgLsdEnabled)
                {
                    if (!Directory.Exists(_cfgLsdOutput))
                    {
                        Directory.CreateDirectory(_cfgLsdOutput);
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        String path = _cfgLsdOutput + @"\player_" + (i + 1) + ".txt";
                        if (File.Exists(path))
                        {
                            try
                            {
                                File.WriteAllText(path, " ", Encoding.UTF8);
                            }
                            catch (Exception ex)
                            {
                                logFile.WriteLine("EXCEPTION Cleaning LSD: " + ex.ToString());
                            }
                        }
                    }
                }
            }
        }

        private Boolean CheckEnd(int game)
        {
            switch (game)
            {
                case 0:
                    for (int i = _lastLine; i < _logContent.Count; i++)
                    {
                        if (_logContent[i].Contains("APP -- Game Stop"))
                        {
                            String str = _logContent[i].Split(new[] { '.' })[0];
                            String[] str2 = str.Split(new[] { ':' });
                            if (CheckTime(Int32.Parse(str2[0]), Int32.Parse(str2[1])))
                            {
                                _matchBeingPlayed = false;
                                _pingTimer.IsEnabled = false;
                                mhl.Enabled = false;
                                _cpmTimer.IsEnabled = false;
                                CleanLSD();
                                return true;
                            }
                        }
                    }
                    break;
                case 1:
                    for (int i = _lastLine; i < _logContent.Count; i++)
                    {
                        if (_logContent[i].Contains("MOD -- Game Over at"))
                        {
                            String str = _logContent[i].Split(new[] { '.' })[0];
                            String[] str2 = str.Split(new[] { ':' });
                            if (CheckTime(Int32.Parse(str2[0]), Int32.Parse(str2[1])))
                            {
                                _matchBeingPlayed = false;
                                PassAgeGateCookie();
                                _pingTimer.IsEnabled = false;
                                mhl.Enabled = false;
                                _cpmTimer.IsEnabled = false;
                                CleanLSD();
                                return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }

        private void setStatus(string text)
        {
            status_cont_text.Text = text;
        }

        private void setLogContents(int game)
        {
            switch (game)
            {
                case 0:
                    _logContent = File.ReadAllLines(_copyLogPath, Encoding.UTF8).ToList();
                    break;
                case 1:
                    if (_logContent.Count < 2800)
                    {
                        _logContent = File.ReadAllLines(_copyLogPath, Encoding.UTF8).ToList();
                    }
                    else
                    {
                        _logContent = File.ReadAllLines(_copyLogPath, Encoding.UTF8).ToList();
                        _logContent.RemoveRange(400, 350);
                    }
                    break;
            }
        }

        private async void FindMatch(int game)
        {
            switch (game)
            {
                case 0:
                    for (int i = 10; i < _logContent.Count; i++)
                    {
                        String str = _logContent[i].Split(new[] { '.' })[0];
                        try
                        {
                            if (Regex.IsMatch(str.Split(new[] { ':' })[0], @"^\d+$"))
                            {
                                if (CheckTime(Int32.Parse(str.Split(new[] { ':' })[0]),
                                    Int32.Parse(str.Split(new[] { ':' })[1])))
                                {
                                    if (
                                        _logContent[i].Contains(
                                            "AutomatchInternal::OnStartComplete - detected successful game start") ||
                                        _logContent[i].Contains("RLINK -- Match Started") ||
                                        _logContent[i].Contains("MOD - Setting player"))
                                    {
                                        logFile.WriteLine("MAIN WINDOW - READER - Found a game match");
                                        _stopPoint = i - 30;
                                        ProcessLog(game);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (FormatException)
                        {
                        }
                    }
                    break;
                case 1:

                    for (int i = 10; i < _logContent.Count; i++)
                    {
                        String str = _logContent[i].Split(new[] { '.' })[0];
                        try
                        {
                            if (Regex.IsMatch(str.Split(new[] { ':' })[0], @"^\d+$"))
                            {

                                if (CheckTime(Int32.Parse(str.Split(new[] { ':' })[0]),
                                    Int32.Parse(str.Split(new[] { ':' })[1])))
                                {
                                    if (_logContent[i].Contains("GAME -- Human") || _logContent[i].Contains("GAME -- AI Player"))
                                    {
                                        logFile.WriteLine("MAIN WINDOW - READER - Found a game match");
                                        await TaskEx.Delay(350);
                                        _stopPoint = i - 135;
                                        ProcessLog(game);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (FormatException)
                        {
                        }
                    }


                    break;
            }
        }

        private async void ProcessLog(int game)
        {
            logFile.WriteLine("MAIN WINDOW - Watcher - Processing Log data - START");
            ClearList();
            pgBarLoading.IsEnabled = true;
            pgBarLoading.IsIndeterminate = true;
            pgBarLoading.Value = 50;
            logFile.WriteLine("MAIN WINDOW - Watcher - Progressbar Enabled");
            switch (game)
            {
                case 0:
                    if (!_matchBeingPlayed)
                    {
                        int matches = 0, mods = 0, order = 0, z = 0;
                        Boolean isModFirst = false;
                        _lastLine = 0;
                        for (int i = _stopPoint; i < _logContent.Count; i++)
                        {
                            if (_logContent[i].Contains("RLINK -- Match Started"))
                            {
                                isModFirst = false;
                                logFile.WriteLine("MAIN WINDOW - Watcher - (COH1) Mod is NOT first");
                                break;
                            }
                            if (_logContent[i].Contains("MOD - Setting player"))
                            {
                                logFile.WriteLine("MAIN WINDOW - Watcher - (COH1) Mod is first");
                                isModFirst = true;
                                break;
                            }
                        }
                        if (isModFirst)
                        {
                            logFile.WriteLine("MAIN WINDOW - Watcher - Finding players info");
                            for (int i = _stopPoint; i < _logContent.Count; i++)
                            {
                                if (_logContent[i].Contains("MOD - Setting player") &&
                                    Regex.IsMatch(_logContent[i], @"\d+(\.\d+)?$"))
                                {
                                    _matchBeingPlayed = true;
                                    _isListWritten = false;

                                    string str1 = _logContent[i].Substring(48, 1).Trim();
                                    _players.Insert(z, new Player
                                    {
                                        Race = Int32.Parse(str1),
                                        Ranking = "-1000",
                                        SteamID = -1000
                                    });
                                    mods++;
                                    z++;
                                    _lastLine = i;
                                }
                            }
                            z = 0;
                            for (int i = _stopPoint; i < _logContent.Count; i++)
                            {
                                if (_logContent[i].Contains("RLINK -- Match Started")) // Setting race
                                {
                                    _matchBeingPlayed = true;
                                    _isListWritten = false;
                                    long sID = Convert.ToInt64(_logContent[i].Substring(65, 17)); // Gets Steam ID
                                    int rank = 0;
                                    try
                                    {
                                        rank = Convert.ToInt32(_logContent[i].Substring(105, 6).Trim());
                                    }
                                    catch (Exception ex)
                                    {
                                        logFile.WriteLine("EXCEPTION Processing log: " + ex.ToString());
                                        rank = Convert.ToInt32(_logContent[i].Substring(105, 5).Trim());
                                    }
                                    // Gets Player Rank
                                    _players[z].Ranking = rank.ToString();
                                    _players[z].SteamID = sID;
                                    matches++;
                                    order++;
                                    z++;
                                    _lastLine = i;
                                }
                            }
                            _matchBeingPlayed = true;
                            _isListWritten = false;
                        }
                        else
                        {
                            z = 0;
                            for (int i = _stopPoint; i < _logContent.Count; i++)
                            {
                                if (_logContent[i].Contains("RLINK -- Match Started")) // Setting race
                                {
                                    long sID = Convert.ToInt64(_logContent[i].Substring(65, 17)); // Gets Steam ID
                                    int rank = 0;
                                    try
                                    {
                                        rank = Convert.ToInt32(_logContent[i].Substring(105, 6).Trim());
                                    }
                                    catch (Exception ex)
                                    {
                                        logFile.WriteLine("EXCEPTION Processing log: " + ex.ToString());
                                        rank = Convert.ToInt32(_logContent[i].Substring(105, 5).Trim());
                                    }
                                    // Gets Player Rank
                                    _players.Insert(z,
                                        new Player { Race = -1000, Ranking = rank.ToString(), SteamID = sID });
                                    matches++;
                                    order++;
                                    z++;
                                    _lastLine = i;
                                }
                            }
                            z = 0;
                            for (int i = _stopPoint; i < _logContent.Count; i++)
                            {
                                if (_logContent[i].Contains("MOD - Setting player") &&
                                    Regex.IsMatch(_logContent[i], @"\d+(\.\d+)?$")) // Setting race
                                {
                                    _matchBeingPlayed = true;
                                    _isListWritten = false;

                                    int race = Int32.Parse(_logContent[i].Substring(48, 1).Trim());
                                    _players[z].Race = race;
                                    mods++;
                                    z++;
                                    _lastLine = i;
                                }
                            }
                        }
                        if (mods > matches)
                        {
                            if (isModFirst)
                            {
                                for (int i = matches; i < mods; i++)
                                {
                                    _players[order].Ranking = "0";
                                    _players[order].SteamID = 0;
                                    order++;
                                }
                            }
                        }
                    }

                    
                    break;
                case 1:

                    if (!_matchBeingPlayed)
                    {
                        _lastLine = 0;
                        int z = 0;
                        int slot = 0;
                        logFile.WriteLine("MAIN WINDOW - Watcher - First Step - START");
                        for (int i = _stopPoint; i < _logContent.Count; i++)
                        {
                            #region FirstStep

                            if (_logContent[i].Contains("Match Started")) // Setting race
                            {
                                _matchBeingPlayed = true;
                                _isListWritten = false;
                                long sID = Convert.ToInt64(_logContent[i].Substring(56, 17)); // Gets Steam ID
                                int rank = 0;
                                try
                                {
                                    string[] strArr =
                                        Regex.Split(_logContent[i].Substring(87, _logContent[i].Length - 87), "=");
                                    rank = Convert.ToInt32(strArr[1].Trim());
                                }
                                catch (Exception ex)
                                {
                                    logFile.WriteLine("EXCEPTION Processing log: " + ex.ToString());
                                    rank = Convert.ToInt32(_logContent[i].Substring(96, 6).Trim());
                                }
                                rank = rank - 1;
                                int altSlot = Int32.Parse(Regex.Split(_logContent[i], "slot =")[1].Substring(0, 3).Trim());
                                _players.Insert(z, new Player()
                                {
                                    Slot = altSlot,
                                    Ranking = rank.ToString(),
                                    SteamID = sID
                                });


                                z++;
                                _lastLine = i;
                            }

                            #endregion
                        }
                        logFile.WriteLine("MAIN WINDOW - Watcher - First Step - ENDED");
                        z = 0;
                        await TaskEx.Delay(150);
                        logFile.WriteLine("MAIN WINDOW - Watcher - Second Step - START");
                        for (int i = _stopPoint; i < _logContent.Count; i++)
                        {
                            #region SecondStep

                            if (_logContent[i].Contains("GAME -- Human") || _logContent[i].Contains("GAME -- AI Player"))
                            {
                                _matchBeingPlayed = true;
                                _isListWritten = false;
                                string g_nick = "";
                                int g_race = 0;
                                int g_team = 0;

                                string sl = Regex.Split(_logContent[i], "Player:")[1].Substring(0, 2).Trim();
                                slot = Convert.ToInt32(sl);

                                if (_logContent[i].Contains("GAME -- Human"))
                                {
                                    logFile.WriteLine("MAIN WINDOW - Watcher - Found Human");
                                    #region Human

                                    string str = _logContent[i].Substring(38, _logContent[i].Length - 38);
                                    string[] rc = Regex.Split(str, " ");
                                    string rc2 = rc[rc.Length - 1];

                                    if (rc2.Equals("aef"))
                                    {
                                        g_race = 3;
                                    }
                                    else if (rc2.Equals("soviet"))
                                    {
                                        g_race = 1;
                                    }
                                    else if (rc2.Equals("west_german"))
                                    {
                                        g_race = 2;
                                    }
                                    else if (rc2.Equals("german"))
                                    {
                                        g_race = 0;
                                    }
                                    else if (rc2.Equals("british"))
                                    {
                                        g_race = 4;
                                    }

                                    try
                                    {
                                        g_team = Int32.Parse(rc[rc.Length - 2]);
                                    }
                                    catch
                                    {
                                        g_team = 0;
                                    }
                                    string unknown = rc[rc.Length - 3];
                                    string[] nickArr = Regex.Split(str, unknown);
                                    g_nick = nickArr[0].Trim();
                                    Teams team;
                                    if (g_race == 0 || g_race == 2)
                                    {
                                        team = Teams.Axis;
                                    }
                                    else
                                    {
                                        team = Teams.Allies;
                                    }

                                    for (int index = 0; index < _players.Count; index++)
                                    {
                                        var player = _players[index];
                                        if (player.Slot == slot)
                                        {
                                            _players[index].Race = g_race;
                                            _players[index].Nickname = g_nick;
                                            _players[index].Team = team;

                                        }
                                    }

                                    #endregion
                                }
                                else if (_logContent[i].Contains("GAME -- AI Player"))
                                {
                                    logFile.WriteLine("MAIN WINDOW - Watcher - Found A.I.");
                                    #region Bots

                                    string str = _logContent[i].Substring(35, _logContent[i].Length - 35);
                                    string[] rc = Regex.Split(str, " ");
                                    string rc2 = rc[rc.Length - 1];

                                    if (rc2.Equals("aef"))
                                    {
                                        g_race = 3;
                                    }
                                    else if (rc2.Equals("soviet"))
                                    {
                                        g_race = 1;
                                    }
                                    else if (rc2.Equals("west_german"))
                                    {
                                        g_race = 2;
                                    }
                                    else if (rc2.Equals("german"))
                                    {
                                        g_race = 0;
                                    }
                                    else if (rc2.Equals("british"))
                                    {
                                        g_race = 4;
                                    }

                                    try
                                    {
                                        g_team = Int32.Parse(rc[rc.Length - 2]);
                                    }
                                    catch
                                    {
                                        g_team = 0;
                                    }

                                    string unknown = rc[rc.Length - 3];

                                    string[] nickArr = Regex.Split(str, unknown);
                                    g_nick = "AI Player - " + (nickArr[0].Trim());
                                    Teams team;
                                    if (g_race == 0 || g_race == 2)
                                    {
                                        team = Teams.Axis;
                                    }
                                    else
                                    {
                                        team = Teams.Allies;
                                    }
                                    _players.Add(new Player
                                    {
                                        Race = g_race,
                                        Slot = slot,
                                        Nickname = g_nick,
                                        Team = team,
                                        Ranking = "N/A",
                                        Level = "N/A"
                                    });


                                    #endregion
                                }


                                z++;
                                _lastLine = i;
                            }

                            #endregion
                        }
                        logFile.WriteLine("MAIN WINDOW - Watcher - Second Step - ENDED");


                    }


                    break;
            }
            logFile.WriteLine("MAIN WINDOW - Watcher - Processing Log data - ENDED");
            _matchBeingPlayed = true;
        }

        private async void SetUpList(int game)
        {
            isMakingList = true;
            
            currPlayer = 0;
            playerList.ItemsSource = null;

            switch (game)
            {
                case 0:
                    for (int i = 0; i < _players.Count; i++)
                    {
                        string final = null;
                        currPlayer = i;


                        
                        while (String.IsNullOrEmpty(final))
                        {
                            if (_players[i].SteamID != 0)
                            {
                                final = Utilities.Steam.getNick(_players[i].SteamID);
                            }
                            else
                            {
                                final = "Computer (CPU)";
                            }
                        }
                        _players[i].Nickname = final;

                        
                        if (_players[i].Ranking == "-1")
                        {
                            _players[i].Ranking = "Unranked (Placements)";
                        }
                        else if (_players[i].Ranking == "0")
                        {
                            _players[i].Ranking = "Unranked (Custom Game)";
                        }
                        else
                        {
                            _players[i].Ranking = _players[i].Ranking;
                        }

                        
                        switch (_players[i].Race)
                        {
                            case 0:
                                _players[i].Icon = "Resources/coh1_0.png";
                                _players[i].RaceName = "Commonwealth";
                                _players[i].Team = Teams.Allies;
                                break;
                            case 1:
                                _players[i].Icon = "Resources/coh1_1.png";
                                _players[i].RaceName = "USA";
                                _players[i].Team = Teams.Allies;
                                break;
                            case 2:
                                _players[i].Icon = "Resources/coh1_2.png";
                                _players[i].RaceName = "Wehrmacht";
                                _players[i].Team = Teams.Axis;
                                break;
                            case 3:
                                _players[i].Icon = "Resources/coh1_3.png";
                                _players[i].RaceName = "Panzer Elite";
                                _players[i].Team = Teams.Axis;
                                break;
                        }

                        logFile.WriteLine("MAIN WINDOW - WATCHER - SET UP PLAYERS - " +
                            String.Format("Player {0}; Nickname: {1}; Race: {2}; Rank: {3}; SteamID: {4}", i, _players[i].Nickname, _players[i].RaceName, _players[i].Ranking, _players[i].SteamID));

                    }


                    break;
                case 1:


                    for (int i = 0; i < _players.Count; i++)
                    {
                        currPlayer = i;

                        _players[i].Country = SourceToImage("Resources/flags/fail.png");
                        _players[i].CountryName = "Unnavailable";

                        
                        long y1 = -1;
                        if (_players[i].SteamID > 1)
                        {
                            try
                            {
                                y1 = (Utilities.Steam.getTimePlayed(_players[i].SteamID, 231430)) / 60;
                            }
                            catch (Exception ex)
                            {
                                y1 = 0;
                                logFile.WriteLine("EXCEPTION Setting up list: " + ex.ToString());
                            }
                            finally
                            {
                                _players[i].TimePlayed = y1;
                            }
                        }

                        _players[i].RankingAfter = "";

                        if (_players[i].Ranking == "-1" || _players[i].Ranking == "-2" || _players[i].Ranking == "0")
                        {
                            _players[i].Ranking = "Unranked";
                        }
                        else
                        {
                            _players[i].Ranking = _players[i].Ranking;
                        }


                        switch (_players[i].Race)
                        {
                            case 0:
                                _players[i].Icon = "Resources/coh2_0.png";
                                _players[i].RaceName = "Wehrmacht";
                                _players[i].Team = Teams.Axis;
                                break;
                            case 1:
                                _players[i].Icon = "Resources/coh2_1.png";
                                _players[i].RaceName = "Soviet Union";
                                _players[i].Team = Teams.Allies;
                                break;
                            case 2:
                                _players[i].Icon = "Resources/coh2_2.png";
                                _players[i].RaceName = "Oberkommando West";
                                _players[i].Team = Teams.Axis;
                                break;
                            case 3:
                                _players[i].Icon = "Resources/coh2_3.png";
                                _players[i].RaceName = "US Forces";
                                _players[i].Team = Teams.Allies;
                                break;
                            case 4:
                                _players[i].Icon = "Resources/coh2_4.png";
                                _players[i].RaceName = "UK Forces";
                                _players[i].Team = Teams.Allies;
                                break;
                        }
                        logFile.WriteLine("MAIN WINDOW - WATCHER - SET UP PLAYERS - " +
                            String.Format("Player {0}; Nickname: {1}; Race: {2}; Rank: {3}; SteamID: {4}", i, _players[i].Nickname, _players[i].RaceName, _players[i].Ranking, _players[i].SteamID));

                    }

                    await RetriveSecondaryInfo();
                    isMakingList = false;

                    break;
            }

            _isListWritten = true;


            playerList.ItemsSource = _players;
            var view = (CollectionView)CollectionViewSource.GetDefaultView(playerList.ItemsSource);
            if (view != null)
            {
                view.GroupDescriptions.Clear();
                var groupDescription = new PropertyGroupDescription("Team");
                view.GroupDescriptions.Add(groupDescription);


                view.SortDescriptions.Clear();
                var sort = new SortDescription("Ranking", ListSortDirection.Ascending);
                view.SortDescriptions.Add(sort);
            }


            playerList.Items.Refresh();


            if (_cfgLsdEnabled)
            {
                RenderLSD();
            }

            if (game == 1)
            {
                RetrieveFlags();
            }

        }

        private void SetUpMatchInfo(int game)
        {
            switch (game)
            {
                case 0:

                    Boolean ps = false;

                    match_type.Content = String.Format("Type: {0}vs{0}", (playerList.Items.Count/2));
                    logFile.WriteLine("MAIN WINDOW - Watcher - Match type: " + match_type.Content);
                    break;

                case 1:
                    if (IsLoaded)
                    {
                        for (int i = 0; i < _logContent.Count; i++)
                        {
                            if (_logContent[i].Contains("GAME -- Scenario:"))
                            {
                                String scenarioName = (Regex.Split(_logContent[i], "scenarios")[1]);
                                String[] MapData = File.ReadAllLines(_AssemblyDir + @"\data\maps\coh2\maps.data");
                                foreach (string line in MapData)
                                {
                                    if (!line.StartsWith("#") || !line.Contains("#"))
                                    {
                                        string mapf = (Regex.Split(line, "==")[1]);
                                        if (mapf.Equals(scenarioName))
                                        {
                                            match_mapName.Text = Regex.Split(line, "==")[0];
                                            string mapFilename = mapf.Split(new[] {'\\'})[3];
                                            var bp =
                                                new BitmapImage(
                                                    new Uri(_AssemblyDir + @"\data\maps\coh2\" + mapFilename + ".jpg"));
                                            match_mapImg.Source = bp;
                                            match_imgTooltip.Source = bp;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        Boolean ps2 = false;
                        match_type.Content = String.Format("Type: {0}vs{0}", (_players.Count/2));
                        logFile.WriteLine("MAIN WINDOW - Watcher - Match type: " + match_type.Content);
                    }

                    break;
            }
            if (_gameSelected == 1)
            {
                match_map.Content = "Map: " + match_mapName.Text;
            }
            else
            {
                match_mapName.Text = "Map: N/A";
            }
        }

        private void SetUpGameInfo(int game)
        {
            logFile.WriteLine("MAIN WINDOW - Watcher - Setting game info - START");
            try
            {
                logFile.WriteLine("MAIN WINDOW - Watcher - Ping timer enabled");
                _pingTimer.IsEnabled = true;
                clicks = 0;
                mins = 0;
                logFile.WriteLine("MAIN WINDOW - Watcher - Mouse Hook enabled (CPM)");
                mhl.Enabled = true;

                mhl.MouseClick += Mhl_MouseClick;
                logFile.WriteLine("MAIN WINDOW - Watcher - CPM Timer is enabled");
                _cpmTimer.IsEnabled = true;
                game_cpm.Content = "CPM: 0";

                switch (game)
                {
                    case 0:
                        
                        var file1 = new FileInfo(_cfgGamePath + @"\RelicCoH.exe");
                        if (file1.Exists)
                        {
                            game_build.Content = "Game Build: 2.700.2.42";
                        }
                        logFile.WriteLine("MAIN WINDOW - Watcher - Game Build: " + game_build.Content);
                        game_replaysCount.Content = "Replays Recorded: " +
                                                    Directory.GetFiles(_cfgDocPath + @"\playback", "*.rec").Length;
                        logFile.WriteLine("MAIN WINDOW - Watcher - Replays Recorded: " + game_replaysCount.Content);
                        status_gameName.Content = "Company of Heroes";
                        logFile.WriteLine("MAIN WINDOW - Watcher - Game name: " + status_gameName.Content);
                        status_gameIcon.Source =
                            new BitmapImage(new Uri(@"pack://application:,,,/Resources/coh1_icon.png"));
                        break;
                    case 1:
                        var file2 = new FileInfo(_cfgGamePath + @"\RelicCoH2.exe");
                        if (file2.Exists)
                        {
                            FileVersionInfo version = FileVersionInfo.GetVersionInfo(_cfgGamePath + @"\RelicCoH2.exe");
                            game_build.Content = "Game Build: " + version.FileVersion;
                        }
                        logFile.WriteLine("MAIN WINDOW - Watcher - Game Build: " + game_build.Content);
                        game_replaysCount.Content = "Replays Recorded: " +
                                                    Directory.GetFiles(_cfgDocPath + @"\playback", "*.rec").Length;
                        logFile.WriteLine("MAIN WINDOW - Watcher - Replays Recorded: " + game_replaysCount.Content);
                        status_gameName.Content = "Company of Heroes 2";
                        logFile.WriteLine("MAIN WINDOW - Watcher - Game name: " + status_gameName.Content);
                        status_gameIcon.Source =
                            new BitmapImage(new Uri(@"pack://application:,,,/Resources/coh2_icon.png"));

                        break;
                }
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION Setting up game info: " + ex.ToString());
            }
        }

        private Task RetriveSecondaryInfo()
        {
            return Task.Factory.StartNew(() =>
            {
                bool x1 = false;
                long y1 = -1;
                switch (_gameSelected)
                {
                    case 0:

                        if (_players[currPlayer].SteamID > 1)
                        {
                            while (!x1)
                            {
                                try
                                {
                                    y1 = (Utilities.Steam.getTimePlayed(_players[currPlayer].SteamID, 231430))/60;
                                    x1 = true;
                                }
                                catch (Exception ex)
                                {
                                    logFile.WriteLine("EXCEPTION Getting time played (coh1) " + ex.ToString());
                                    x1 = false;
                                }
                            }
                            _players[currPlayer].TimePlayed = y1;
                            _players[currPlayer].Level = "Unnavailable";
                        }
                        else
                        {
                            _players[currPlayer].TimePlayed = 0;
                            _players[currPlayer].Level = "Unnavailable";
                        }

                        break;
                    case 1:

                        int zed = 0;
                        int max = _players.Count;

                        for(int i = 0; i < max; i++)
                        {
                            
                            if (_players[i].SteamID > 1)
                            {
                                try
                                {
                                    var web = new HtmlWeb();
                                    
                                    var htmlDoc = new HtmlDocument();
                                    htmlDoc = web.Load("http://www.coh2.org/ladders/playercard/steamid/" +
                                                       _players[i].SteamID);
                                    htmlDoc.OptionFixNestedTags = true;

                                    if (htmlDoc.DocumentNode != null)
                                    {
                                        HtmlNode bodyNode =
                                            htmlDoc.DocumentNode.SelectSingleNode("//div[@class='playerxp']");

                                        if (bodyNode != null)
                                        {
                                            string z = bodyNode.InnerText;
                                            string[] zArr = Regex.Split(z, "Prestige:");
                                            string PrestigeNum = zArr[1].Substring(0, 3).Trim();
                                            string Level = Regex.Split(zArr[1], ";")[1].Substring(0, 2).Trim();
                                            Level = Level.Replace("&", "");
                                            String FinalLevel = String.Format("Prestige {0} ({1})", PrestigeNum, Level);
                                            if (FinalLevel == "Prestige 3 (33)")
                                            {
                                                FinalLevel = "Prestige 3 (MAX)";
                                            }
                                            _players[zed].Level = FinalLevel;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logFile.WriteLine("EXCEPTION Getting player level: " + ex.ToString());
                                    
                                }

                            }
                            zed++;
                        }


                        break;
                }
            });
        }
        
        private void RetrievePing()
        {
            if (Utilities.CheckInternet())
            {
                var info = new ProcessStartInfo(_AssemblyDir + @"\data\assemblies\paping.exe");
                String RelicServerIp = "54.209.64.161"; // RELIC Battle Server IP
                info.Arguments = "--nocolor -c 3 -p 27020 " + RelicServerIp; 

                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.RedirectStandardOutput = true;

                var pr = new Process();
                pr.StartInfo = info;
                var outputList = new List<String>();
                if (pr.Start())
                {
                    int sum = 0;
                    while (!pr.StandardOutput.EndOfStream)
                    {
                        outputList.Add(pr.StandardOutput.ReadLine());
                    }
                    foreach (string line in outputList)
                    {
                        if (line.StartsWith("Connected to"))
                        {
                            string[] t1 = Regex.Split(line, "=");
                            string[] t2 = Regex.Split(t1[1], "ms");
                            int millis = Int32.Parse(t2[0].Split(new[] {'.'})[0]);
                            sum += millis;
                        }
                    }
                    ping = sum/3;
                    Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        (MethodInvoker) delegate { game_ping.Content = "Battle-Servers Ping: " + ping + " ms"; });
                }
            }
        }

        private async void RetrieveFlags()
        {
            setStatus("Getting players countries");
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].SteamID > 0 && _players[i].CountryName == "Unnavailable")
                {
                    
                        webFlags.Navigate("http://www.companyofheroes.com/leaderboards#profile/steam/" +
                                          _players[i].SteamID.ToString());

                        
                        while (webFlags.ReadyState != WebBrowserReadyState.Complete)
                        {
                            await TaskEx.Delay(350);
                        }
                        await TaskEx.Delay(3500);
                        
                         
                            var doc = webFlags.Document;
                            if (doc != null)
                            {
                                var elements = doc.GetElementsByTagName("span");
                                string z = "";
                                int k = 0;
                                while (string.IsNullOrEmpty(z) && k < 10)
                                {
                                    
                                    foreach (HtmlElement element in elements)
                                    {
                                        string className = element.GetAttribute("className");
                                        if (Regex.IsMatch(className, "flag .."))
                                        {
                                            z = className.Split(new char[] {' '})[1];
                                            break;
                                        }

                                    }

                                    k++;
                                }

                                if (string.IsNullOrEmpty(z) || z.Length != 2 || k >= 20)
                                {
                                    _players[i].Country = SourceToImage("Resources/flags/fail.png");
                                    _players[i].CountryName = "Unnavailable";
                                }
                                else
                                {
                                    _players[i].Country = SourceToImage("Resources/flags/" + z + ".png");
                                    _players[i].CountryName = z;
                                }
                                webFlags.Navigate("about:blank");
                                
                            }

                       
                    
                    
                }
            }

            curFlag++;
            if (_players.Any(x => x.CountryName == "Unnavailable" && x.SteamID > 0))
            {
                if (curFlag > 3)
                {
                    pgBarLoading.IsEnabled = false;
                    pgBarLoading.IsIndeterminate = false;
                    pgBarLoading.Value = 0;
                    playerList.ItemsSource = null;
                    playerList.ItemsSource = _players;

                    await TaskEx.Delay(1000);

                    setStatus("Information listed");
                    return;
                }

                RetrieveFlags();
            }

            pgBarLoading.IsEnabled = false;
            pgBarLoading.IsIndeterminate = false;
            pgBarLoading.Value = 0;
            playerList.ItemsSource = null;
            playerList.ItemsSource = _players;

            await TaskEx.Delay(1000);

            setStatus("Information listed");
            
        }

        private void SetUpInfo(int game, bool setting)
        {
            logFile.WriteLine("MAIN WINDOW - Watcher - Setting up info - START");
            if (setting)
            {
                try
                {
                    logFile.WriteLine("MAIN WINDOW - Watcher - Setting up list - START");
                    SetUpList(game);
                    logFile.WriteLine("MAIN WINDOW - Watcher - Setting up list - ENDED");
                }
                catch (Exception ex)
                {
                    logFile.WriteLine("EXCEPTION Setting up list: " + ex.ToString());
                }
                setStatus("Parsing information");
                LoadFormHost.Visibility = Visibility.Visible;
                SetUpGameInfo(game);
                try
                {
                    logFile.WriteLine("MAIN WINDOW - Watcher - Setting up Match Info - START");
                    SetUpMatchInfo(game);
                    logFile.WriteLine("MAIN WINDOW - Watcher - Setting up Match Info - ENDED");
                }
                catch (Exception ex)
                {
                    logFile.WriteLine("EXCEPTION Setting up Match info: " + ex.ToString());
                }
            }
            else
            {
                _matchBeingPlayed = false;
                _isListWritten = true;
                _lastLine = 0;
                _stopPoint = 0;
                _pingTimer.IsEnabled = false;
                btn_GameWatcher_Click(btn_GameWatcher, null);
                btn_GameWatcher_Click(btn_GameWatcher, null);
            }

            logFile.WriteLine("MAIN WINDOW - Watcher - Setting up info - ENDED");
        }

        private int maxZ = 0;

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        private async void PassAgeGateCookie()
        {
            webFlags = null;
            webFlags = new WebBrowser();
            webFlags.AllowWebBrowserDrop = false;
            webFlags.ScrollBarsEnabled = false;
            webFlags.WebBrowserShortcutsEnabled = true;
            webFlags.ScriptErrorsSuppressed = true;
            logFile.WriteLine("MAIN WINDOW - AGE GATE START");
            logFile.WriteLine("MAIN WINDOW - Setting Cookie [AGE GATE]");
            Cookie temp1 = new Cookie("agegate[passed]","yes", Path.GetTempPath(), "companyofheroes.com");
            InternetSetCookie("http://www.companyofheroes.com", null, temp1.ToString() + "; expires = Sun, 01-Jan-2020 00:00:00 GMT");
            logFile.WriteLine("MAIN WINDOW - COOKIE SET [AGE GATE]");
            Console.WriteLine("COOKIE: " + temp1.ToString());
            await TaskEx.Delay(500);
            webFlags.Navigate("http://www.companyofheroes.com/");
            logFile.WriteLine("MAIN WINDOW - LOADING COH WEBSITE [AGE GATE]");
            logFile.WriteLine("MAIN WINDOW - AGE GATE END");

        }
        

      #endregion

        #region CoH Functions

        private void Mhl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                String app = GetActiveWindowTitle();
                if (app.Contains("Company Of Heroes 2") || app.Contains("Company Of Heroes"))
                {
                    clicks++;
                }
            }
            catch
            {
            }
        }

        private void RenderLSD()
        {
            if (_cfgLsdOutput != "")
            {
                if (_cfgLsdEnabled)
                {
                    if (!Directory.Exists(_cfgLsdOutput))
                    {
                        Directory.CreateDirectory(_cfgLsdOutput);
                    }

                    if (File.Exists(_AssemblyDir + @"\lsd.ini") && new FileInfo(_AssemblyDir + @"\lsd.ini").Length > 5)
                    {
                        var lsdcfg = new Utilities.INIFile(_AssemblyDir + @"\lsd.ini");
                        String outputFolder = _cfgLsdOutput;
                        for (int i = 0; i < _players.Count; i++)
                        {
                            String pCont = lsdcfg.IniReadValue("Players", "P" + (i + 1));
                            if (!String.IsNullOrEmpty(pCont))
                            {
                                String Pass1 = pCont.Replace("%NICK%", _players[i].Nickname)
                                    .Replace("%RANK%", _players[i].Ranking);
                                String Pass2 =
                                    Pass1.Replace("%LEVEL%", _players[i].Level)
                                        .Replace("%STEAMID%", _players[i].SteamID.ToString())
                                        .Replace("%HOURS%", _players[i].TimePlayed.ToString());
                                String output = Pass2;
                                try
                                {
                                    File.WriteAllText(outputFolder + @"\player_" + (i + 1) + ".txt", output,
                                        Encoding.UTF8);
                                }
                                catch (Exception ex)
                                {
                                    logFile.WriteLine("EXCEPTION Rendering LSD: " + ex.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void FactorCreator()
        {
            try
            {
                if (_cfgHistoryEnabled)
                {
                    await GenerateMatchHistory();
                }
                if (_cfgLsdEnabled)
                {
                    CleanLSD();
                }
                if (_cfgCleanList)
                {
                    ClearList();
                }
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION Creating factors: " + ex.ToString());
            }
        }

        private Task GenerateMatchHistory()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    Thread.Sleep(5000);

                    string dbFile = _AssemblyDir + @"\data\history\coh" + (_gameSelected + 1) + @"\mhv.xml";
                    string repFolder = _AssemblyDir + @"\data\history\coh" + (_gameSelected + 1) + @"\replays";
                    if (!File.Exists(dbFile) || File.ReadAllText(dbFile) == "")
                    {
                        XmlWriter xWriter = new XmlTextWriter(new StreamWriter(dbFile));
                        xWriter.WriteStartElement("Matches");
                    }
                    Directory.CreateDirectory(repFolder);
                    String ReplayFile = _cfgDocPath + @"\playback\temp.rec";
                    String ReplayCopy = Guid.NewGuid() + ".rec";
                    File.Copy(ReplayFile, repFolder + @"\" + ReplayCopy, true);
                    String gameDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                    String MapFileName = ReplayManager.RetrieveMap(repFolder + @"\" + ReplayCopy, _gameSelected);

                    var document = new XmlDocument();
                    document.Load(dbFile);
                    XmlNode MatchNode = document.CreateElement("Match");
                    XmlNode ReplayNode = document.CreateElement("Replay");
                    ReplayNode.InnerText = ReplayCopy;
                    XmlNode DateNode = document.CreateElement("Date");
                    DateNode.InnerText = gameDate;
                    XmlNode MapNode = document.CreateElement("Map");
                    MapNode.InnerText = MapFileName;
                    XmlNode TypeNode = document.CreateElement("Type");
                    TypeNode.InnerText = (_players.Count/2).ToString();
                    XmlNode PlayersNode = document.CreateElement("Players");
                    MatchNode.AppendChild(ReplayNode);
                    MatchNode.AppendChild(DateNode);
                    MatchNode.AppendChild(MapNode);
                    MatchNode.AppendChild(TypeNode);
                    MatchNode.AppendChild(PlayersNode);

                    for (int i = 0; i < _players.Count; i++)
                    {
                        XmlNode PLNode = document.CreateElement("Player");

                        XmlNode NickNode = document.CreateElement("Nickname");
                        NickNode.InnerText = _players[i].Nickname;
                        XmlNode RaceNode = document.CreateElement("Race");
                        RaceNode.InnerText = _players[i].Race.ToString();
                        XmlNode RankNode = document.CreateElement("Ranking");
                        RankNode.InnerText = _players[i].Ranking;
                        XmlNode LevelNode = document.CreateElement("Level");
                        LevelNode.InnerText = _players[i].Level;
                        XmlNode TimeNode = document.CreateElement("Timeplayed");
                        TimeNode.InnerText = _players[i].TimePlayed.ToString();
                        XmlNode SteamNode = document.CreateElement("SteamID");
                        SteamNode.InnerText = _players[i].SteamID.ToString();
                        PlayersNode.AppendChild(PLNode);
                        PLNode.AppendChild(NickNode);
                        PLNode.AppendChild(RaceNode);
                        PLNode.AppendChild(RankNode);
                        PLNode.AppendChild(LevelNode);
                        PLNode.AppendChild(TimeNode);
                        PLNode.AppendChild(SteamNode);
                    }
                    document.DocumentElement.AppendChild(MatchNode);
                    document.Save(dbFile);
                }
                catch (Exception ex)
                {
                    logFile.WriteLine("EXCEPTION writing to XML (MHV): " + ex.ToString());
                }
            });
        }

        #endregion

        public ImageSource SourceToImage(string source)
        {
            BitmapImage bm = new BitmapImage(new Uri("pack://application:,,,/" + source,UriKind.Absolute));
            return bm;
        }

        internal class Player
        {
            public long SteamID { get; set; }
            public string Ranking { get; set; }
            public string RankingAfter { get; set; }
            public ImageSource Country { get; set; }
            public string CountryName { get; set; }
            public int Race { get; set; }
            public int Slot { get; set; }
            public string Nickname { get; set; }
            public string RaceName { get; set; }
            public long TimePlayed { get; set; }
            public Teams Team { get; set; }
            public string Level { get; set; }
            public string Icon { get; set; }
        }

        private void mnuLogs_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\logs");
        }

        

        

       
    }
}