using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using ExceptionReporting;
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

        #endregion

        #region Timers

        private DispatcherTimer _cpmTimer;
        private DispatcherTimer _pingTimer;
        private DispatcherTimer _readerTimer;
        private DispatcherTimer _updTimer;
        private int mins;
        private int ping;
        private Thread th;

        private void LoadTimers()
        {
            _cfgTimerInt = 1500;
            _readerTimer = new DispatcherTimer(DispatcherPriority.Render);
            _readerTimer.Interval = TimeSpan.FromMilliseconds(_cfgTimerInt);
            _readerTimer.IsEnabled = false;
            _readerTimer.Tick += _readerTimer_Tick;
            _pingTimer = new DispatcherTimer(DispatcherPriority.Input);
            _pingTimer.Interval = new TimeSpan(0, 0, 10);
            _pingTimer.IsEnabled = false;
            _pingTimer.Tick += _pingTimer_Tick;
            _cpmTimer = new DispatcherTimer(DispatcherPriority.Background);
            _cpmTimer.IsEnabled = false;
            _cpmTimer.Interval = new TimeSpan(0, 1, 0);
            _cpmTimer.Tick += _cpmTimer_Tick;
            _updTimer.Interval = new TimeSpan(0, 10, 0);
            _updTimer.Tick += _updTimer_Tick;
            _updTimer.IsEnabled = true;
        }

        private void _updTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (cfg.IniReadValue("Main", "CheckForUpdates").ToLower() == "true"){
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                    Version version = Version.Parse(fvi.FileVersion);
                    String response = Updater.CheckForUpdates(version);
                    if (response != null)
                    {
                        NotifyIcon nt = new NotifyIcon();
                        nt.BalloonTipText = "New CELO update available!\nVersion: " + response;
                        nt.BalloonTipTitle = "CELO Update";
                        nt.ShowBalloonTip(2500);
                        nt.Click += Nt_Click;
                        nt.BalloonTipClicked += Nt_BalloonTipClicked;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                _updTimer.IsEnabled = false;
            }
        }

        private void Nt_BalloonTipClicked(object sender, EventArgs e)
        {
            this.Show();
            this.Focus();
        }

        private void Nt_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Focus();
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

        private void _readerTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                switch (_gameSelected)
                {
                    case 0:
                        if (IsGameRunning(0))
                        {
                            if (CopyLog())
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
                                    setStatus("Information has been displayed");
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
                            if (CopyLog())
                            {
                                if (!_matchBeingPlayed)
                                {
                                    setStatus("Waiting for a match to start");
                                    if (CheckNotifications()){
                                        ExecuteNotifications();
                                    }

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
            catch (Exception ex)
            {
                WriteLog(ex);
                var reporter = new ExceptionReporter();
                reporter.Config.AppName = "CELO Enhanced";
                reporter.Config.CompanyName = "Neffware";
                reporter.Config.TitleText = "CELO Enhanced Error Report";
                reporter.Config.EmailReportAddress = "admin@neffware.com";
                reporter.Config.ShowSysInfoTab = false; 
                reporter.Config.ShowFlatButtons = true; 
                reporter.Config.TakeScreenshot = true; 
                reporter.Show(ex);
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
        private int _cfgTimerInt = 2100;
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
                WriteLog(ex);
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
                WriteLog(ex);
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            App.Current.Shutdown();
        }

        private void Load_Essential()
        {
            pLoadpic.Image = Properties.Resources.preLoader;
            if (!GetConfigs())
            {
                MessageBox.Show(this, "There was an error loading the configuration files",
                    "Error loading configuration",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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

            if (cfg.IniReadValue("Main", "CheckForUpdates").ToLower() == "true")
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                Version version = Version.Parse(fvi.FileVersion);
                String response = Updater.CheckForUpdates(version);
                if (response != null)
                {
                    if (MessageBox.Show(this,
                            "A new CELO Enchanced version is available.\nNew Version: " + response.ToString() +
                            "\nDo you want to update now?", "Update available", MessageBoxButton.YesNo,
                            MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        
                    }
                }
            }

            LoadTimers();
            
            if (_cfgStartGW)
            {
                ToggleGW();
            }
            _appLog.CreateNew();


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
                Process.Start("http://www.companyofheroes.com/en_us/player-stats/steam/" + n.SteamID);
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
        private static Utilities.Log _appLog = new Utilities.Log(AppDomain.CurrentDomain.BaseDirectory + @"\celolog.log");
        private readonly String _copyLogPath = AppDomain.CurrentDomain.BaseDirectory + @"\data\log.tempf";
        private readonly ObservableCollection<Player> _players = new ObservableCollection<Player>();
        private readonly MouseHookListener mhl = new MouseHookListener(new GlobalHooker());
        private Thread InfoThread;
        private List<String> _logContent = new List<String>();
        private long clicks;
        private int notificationStop = 0, notificationTooggle = 0;
        private Boolean isMakingList;
        private Boolean repeater = true;

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

        private Boolean CopyLog()
        {
            try
            {
                File.Copy(_cfgDocPath + @"\warnings.log", _copyLogPath, true);
                setLogContents(_gameSelected);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
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
            for (int i = notificationStop-10; i < _logContent.Count; i++)
            {
                if (_logContent[i].Contains("RNT_StatsUpdate:"))
                {
                    long steamIDNotification = 0;
                    string[] strArr1 = Regex.Split(_logContent[i], "],");
                    string sID = strArr1[0].Split(new char[] {'/'})[2];
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
                    var view = (CollectionView) CollectionViewSource.GetDefaultView(playerList.ItemsSource);
                    if (view.GroupDescriptions != null) 
                        view.GroupDescriptions.Clear();
                    if(view.SortDescriptions!= null) 
                        view.SortDescriptions.Clear();
                }
                catch(Exception ex)
                {
                    WriteLog(ex);
                    
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
                                WriteLog(ex);
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
                            String str = _logContent[i].Split(new[] {'.'})[0];
                            String[] str2 = str.Split(new[] {':'});
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
                            String str = _logContent[i].Split(new[] {'.'})[0];
                            String[] str2 = str.Split(new[] {':'});
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
                        String str = _logContent[i].Split(new[] {'.'})[0];
                        try
                        {
                            if (Regex.IsMatch(str.Split(new[] {':'})[0], @"^\d+$"))
                            {
                                if (CheckTime(Int32.Parse(str.Split(new[] {':'})[0]),
                                    Int32.Parse(str.Split(new[] {':'})[1])))
                                {
                                    if (
                                        _logContent[i].Contains(
                                            "AutomatchInternal::OnStartComplete - detected successful game start") ||
                                        _logContent[i].Contains("RLINK -- Match Started") ||
                                        _logContent[i].Contains("MOD - Setting player"))
                                    {
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
                        String str = _logContent[i].Split(new[] {'.'})[0];
                        try
                        {
                            if (Regex.IsMatch(str.Split(new[] {':'})[0], @"^\d+$"))
                            {
                                if (CheckTime(Int32.Parse(str.Split(new[] {':'})[0]),
                                    Int32.Parse(str.Split(new[] {':'})[1])))
                                {
                                    if (_logContent[i].Contains("GAME -- Human") || _logContent[i].Contains("GAME -- AI Player"))
                                    {
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
            ClearList();
            pgBarLoading.IsEnabled = true;
            pgBarLoading.IsIndeterminate = true;
            pgBarLoading.Value = 50;
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
                                break;
                            }
                            if (_logContent[i].Contains("MOD - Setting player"))
                            {
                                isModFirst = true;
                                break;
                            }
                        }
                        if (isModFirst)
                        {
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
                                        WriteLog(ex);
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
                                        WriteLog(ex);
                                        rank = Convert.ToInt32(_logContent[i].Substring(105, 5).Trim());
                                    }
                                    // Gets Player Rank
                                    _players.Insert(z,
                                        new Player {Race = -1000, Ranking = rank.ToString(), SteamID = sID});
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
                                    WriteLog(ex);
                                    rank = Convert.ToInt32(_logContent[i].Substring(96, 6).Trim());
                                }
                                
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
                        z = 0;
                        await TaskEx.Delay(150);
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
                                
                                string sl = Regex.Split(_logContent[i], "Player:")[1].Substring(0,2).Trim();
                                slot = Convert.ToInt32(sl);

                                if (_logContent[i].Contains("GAME -- Human"))
                                {
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
                        
                        
                    }


                    break;
            }
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
                    }


                    break;
                case 1:


                    for (int i = 0; i < _players.Count; i++)
                    {
                        currPlayer = i;

                        long y1 = -1;
                        if (_players[i].SteamID > 1)
                        {
                            try
                            {
                                y1 = (Utilities.Steam.getTimePlayed(_players[i].SteamID, 231430))/60;
                            }
                            catch (Exception ex)
                            {
                                y1 = 0;
                                WriteLog(ex);
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
                                break;
                            case 1:
                                _players[i].Icon = "Resources/coh2_1.png";
                                _players[i].RaceName = "Soviet Union";
                                break;
                            case 2:
                                _players[i].Icon = "Resources/coh2_2.png";
                                _players[i].RaceName = "Oberkommando West";
                                break;
                            case 3:
                                _players[i].Icon = "Resources/coh2_3.png";
                                _players[i].RaceName = "US Forces";
                                break;
                        }
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

            pgBarLoading.IsEnabled = false;
            pgBarLoading.IsIndeterminate = false;
            pgBarLoading.Value = 0;

        }

        private void SetUpMatchInfo(int game)
        {
            switch (game)
            {
                case 0:

                    Boolean ps = false;

                    match_type.Content = String.Format("Type: {0}vs{0}", (playerList.Items.Count/2));

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
            try
            {
                _pingTimer.IsEnabled = true;
                clicks = 0;
                mins = 0;
                mhl.Enabled = true;

                mhl.MouseClick += Mhl_MouseClick;
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
                        game_replaysCount.Content = "Replays Recorded: " +
                                                    Directory.GetFiles(_cfgDocPath + @"\playback", "*.rec").Length;
                        status_gameName.Content = "Company of Heroes";
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
                        game_replaysCount.Content = "Replays Recorded: " +
                                                    Directory.GetFiles(_cfgDocPath + @"\playback", "*.rec").Length;
                        status_gameName.Content = "Company of Heroes 2";
                        status_gameIcon.Source =
                            new BitmapImage(new Uri(@"pack://application:,,,/Resources/coh2_icon.png"));

                        break;
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                var reporter = new ExceptionReporter();
                reporter.Config.AppName = "CELO Enhanced";
                reporter.Config.CompanyName = "Neffware";
                reporter.Config.TitleText = "CELO Enhanced Error Report";
                reporter.Config.EmailReportAddress = "admin@neffware.com";
                reporter.Config.ShowSysInfoTab = false; // all tabs are shown by default
                reporter.Config.ShowFlatButtons = true; // this particular config is code-only
                reporter.Config.TakeScreenshot = true; // attached if sending email
                // reporter.Config.FilesToAttach = new[] { "c:/file.txt" }; // any other files to attach
                reporter.Show(ex);
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
                                    WriteLog(ex);
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

                                            String FinalLevel = String.Format("Prestige {0} ({1})", PrestigeNum, Level);
                                            _players[zed].Level = FinalLevel;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WriteLog(ex);
                                    
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

        private void SetUpInfo(int game, bool setting)
        {
            if (setting)
            {
                try
                {
                    SetUpList(game);
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    var reporter = new ExceptionReporter();
                    reporter.Config.AppName = "CELO Enhanced";
                    reporter.Config.CompanyName = "Neffware";
                    reporter.Config.TitleText = "CELO Enhanced Error Report";
                    reporter.Config.EmailReportAddress = "admin@neffware.com";
                    reporter.Config.ShowSysInfoTab = false; // all tabs are shown by default
                    reporter.Config.ShowFlatButtons = true; // this particular config is code-only
                    reporter.Config.TakeScreenshot = true; // attached if sending email
                    // reporter.Config.FilesToAttach = new[] { "c:/file.txt" }; // any other files to attach
                    reporter.Show(ex);
                }
                setStatus("Parsing information");
                LoadFormHost.Visibility = Visibility.Visible;
                SetUpGameInfo(game);
                try
                {
                    SetUpMatchInfo(game);
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    var reporter = new ExceptionReporter();
                    reporter.Config.AppName = "CELO Enhanced";
                    reporter.Config.CompanyName = "Neffware";
                    reporter.Config.TitleText = "CELO Enhanced Error Report";
                    reporter.Config.EmailReportAddress = "admin@neffware.com";
                    reporter.Config.ShowSysInfoTab = false; // all tabs are shown by default
                    reporter.Config.ShowFlatButtons = true; // this particular config is code-only
                    reporter.Config.TakeScreenshot = true; // attached if sending email
                    // reporter.Config.FilesToAttach = new[] { "c:/file.txt" }; // any other files to attach
                    reporter.Show(ex);
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
        }

        private void WriteLog(Exception exception)
        {
            _appLog.WriteLine(exception.ToString());
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
                                    WriteLog(ex);
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
                WriteLog(ex);
            }
        }

        private Task GenerateMatchHistory()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    Thread.Sleep(4500);

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
                    WriteLog(ex);
                    var reporter = new ExceptionReporter();
                    reporter.Config.AppName = "CELO Enhanced";
                    reporter.Config.CompanyName = "Neffware";
                    reporter.Config.TitleText = "CELO Enhanced Error Report";
                    reporter.Config.EmailReportAddress = "admin@neffware.com";
                    reporter.Config.ShowSysInfoTab = false; // all tabs are shown by default
                    reporter.Config.ShowFlatButtons = true; // this particular config is code-only
                    reporter.Config.TakeScreenshot = true; // attached if sending email
                    // reporter.Config.FilesToAttach = new[] { "c:/file.txt" }; // any other files to attach
                    reporter.Show(ex);
                }
            });
        }

        #endregion

        private void Celo_Main_Loaded(object sender, RoutedEventArgs e)
        {
            Load_Essential();
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

        public class NullImageConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null)
                    return DependencyProperty.UnsetValue;
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        internal class Player
        {
            public long SteamID { get; set; }
            public string Ranking { get; set; }
            public string RankingAfter { get; set; }
            public int Race { get; set; }
            public int Slot { get; set; }
            public string Nickname { get; set; }
            public string RaceName { get; set; }
            public long TimePlayed { get; set; }
            public Teams Team { get; set; }
            public string Level { get; set; }
            public string Icon { get; set; }
        }

       
    }
}