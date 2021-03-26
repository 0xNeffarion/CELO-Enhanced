using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
using Microsoft.VisualBasic.Devices;
using Gma.System.MouseKeyHook;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Color = System.Drawing.Color;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;

namespace CELO_Enhanced {
    public partial class MainWindow : Window {
        private readonly Utilities.INIFile cfg;
        private long mysteamId = 0;
        private bool isSteamIdSearch = false;
        private bool isLocked = false;

        public MainWindow() {
            InitializeComponent();
            Application.Current.MainWindow = this;
            cfg = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += GlobalHook_KeyDown;
            globalHook.MouseClick += GlobalHook_MouseClick;

        }

        private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (isKeyboardHookEnabled) {
                if (e.KeyCode == Keys.F4) {
                    _warspoilTimeSpan = new TimeSpan(0, 0, 0, 0);

                }
            }
        }

        private void GlobalHook_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (isMouseHookEnabled) {
                try {
                    var app = GetActiveWindowTitle();
                    if (app.Contains("Company Of Heroes 2") || app.Contains("Company Of Heroes")) {
                        clicks++;
                        game_cpmTotal.Content = "Clicks: " + clicks;
                    }
                } catch {
                }
            }

        }

        public ImageSource SourceToImage(string source) {
            var bm = new BitmapImage(new Uri("pack://application:,,,/" + source, UriKind.Absolute));
            return bm;
        }

        internal class Player : INotifyPropertyChanged {
            private ImageSource country;
            private string level;
            private string nickname;
            private string ranking;
            private long steamid;
            private long timeplayed;

            public long SteamID {
                get { return steamid; }
                set {
                    if (steamid != value) {
                        steamid = value;
                        NotifyPropertyChanged("SteamID");
                    }
                }
            }

            public string Ranking {
                get { return ranking; }
                set {
                    if (ranking != value) {
                        ranking = value;
                        NotifyPropertyChanged("Ranking");
                    }
                }
            }

            public string RankingAfter { get; set; }

            public ImageSource Country {
                get { return country; }
                set {
                    if (country != value) {
                        country = value;
                        NotifyPropertyChanged("Country");
                    }
                }
            }

            public string CountryName { get; set; }
            public int Race { get; set; }
            public int Slot { get; set; }

            public string Nickname {
                get { return nickname; }
                set {
                    if (nickname != value) {
                        nickname = value;
                        NotifyPropertyChanged("Nickname");
                    }
                }
            }

            public string RaceName { get; set; }

            public long TimePlayed {
                get { return timeplayed; }
                set {
                    if (timeplayed != value) {
                        timeplayed = value;
                        NotifyPropertyChanged("TimePlayed");
                    }
                }
            }

            public Teams Team { get; set; }

            public string Level {
                get { return level; }
                set {
                    if (level != value) {
                        level = value;
                        NotifyPropertyChanged("Level");
                    }
                }
            }

            public string Icon { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;

            public void NotifyPropertyChanged(string propName) {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
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

        private string GetActiveWindowTitle() {
            var activeWindowId = GetForegroundWindow();
            if (activeWindowId.Equals(0)) {
                return null;
            }
            int processId;
            GetWindowThreadProcessId(activeWindowId, out processId);
            if (processId == 0) {
                return null;
            }
            var ps = Process.GetProcessById(processId);
            String title = null;
            if (!string.IsNullOrEmpty(ps.MainWindowTitle)) {
                title = ps.MainWindowTitle;
            }

            if (string.IsNullOrEmpty(title)) {
                const int Count = 1024;
                var sb = new StringBuilder(Count);
                GetWindowText((int)activeWindowId, sb, Count);
                title = sb.ToString();
            }
            return title;
        }

        private void Celo_Main_Closing(object sender, CancelEventArgs e) {
            Application.Current.Shutdown(0);
        }

        #endregion

        #region Timers

        private DispatcherTimer _cpmTimer;
        private DispatcherTimer _pingTimer;
        private DispatcherTimer _readerTimer;
        private DispatcherTimer _logTimer;
        private DispatcherTimer _updTimer;
        private DispatcherTimer _failsafeTimer;
        private DispatcherTimer _warSpoilTimer;
        private TimeSpan _warspoilTimeSpan;
        private readonly TimeSpan tm = new TimeSpan(0, 3, 0, 0);
        private int mins;
        private int ping;
        private Thread th;

        private void LoadTimers() {

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
            _updTimer.Interval = new TimeSpan(0, 15, 0);
            _updTimer.Tick += _updTimer_Tick;
            logFile.WriteLine("MAIN WINDOW - Loaded update timer");
            logFile.WriteLine("MAIN WINDOW - Loading log timer");
            _logTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            _logTimer.IsEnabled = true;
            _logTimer.Interval = new TimeSpan(0, 0, 1, 0);
            _logTimer.Tick += _logTimer_Tick;
            logFile.WriteLine("MAIN WINDOW - Loaded log timer");
            logFile.WriteLine("MAIN WINDOW - Loading failsafe timer");
            _failsafeTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            _failsafeTimer.IsEnabled = false;
            _failsafeTimer.Interval = new TimeSpan(0, 0, 0, 25);
            _failsafeTimer.Tick += _failsafeTimer_Tick;
            logFile.WriteLine("MAIN WINDOW - Loaded failsafe timer");
            logFile.WriteLine("MAIN WINDOW - Loading warspoil timer");
            _warSpoilTimer = new DispatcherTimer(DispatcherPriority.Background);
            _warSpoilTimer.IsEnabled = false;
            _warSpoilTimer.Interval = new TimeSpan(0, 0, 1);
            _warSpoilTimer.Tick += _warSpoilTimer_Tick;
            logFile.WriteLine("MAIN WINDOW - Loaded warspoil timer");

        }

        private void _warSpoilTimer_Tick(object sender, EventArgs e) {
            _warspoilTimeSpan = _warspoilTimeSpan.Add(new TimeSpan(0, 0, 0, 1, 0));
            TimeSpan resTm = tm.Subtract(_warspoilTimeSpan);
            if (resTm.TotalSeconds > 0) {
                match_drop.Text = "Approx. Warspoil countdown: " + resTm.ToString("c") + " (F4 Resets)";
            } else {
                match_drop.Text = "Approx. Warspoil countdown: Finished (F4 Resets)";
            }

        }

        private void _failsafeTimer_Tick(object sender, EventArgs e) {
            if (_matchBeingPlayed && _isListWritten) {
                if (_players.Count <= 0 || playerList.Items.Count <= 0) {
                    logFile.WriteLine("FAILSAFE TRIGGERED");
                    _players.Clear();
                    _matchBeingPlayed = false;
                    _isListWritten = false;
                    _readerTimer.IsEnabled = false;
                    _readerTimer = null;

                    _readerTimer = new DispatcherTimer(DispatcherPriority.Background);
                    _readerTimer.Interval = TimeSpan.FromMilliseconds(_cfgTimerInt);
                    _readerTimer.IsEnabled = false;
                    _readerTimer.Tick += _readerTimer_Tick;

                    _readerTimer.IsEnabled = true;
                }
            }

            _failsafeTimer.Stop();
        }

        private async void _logTimer_Tick(object sender, EventArgs e) {
            cpuCounter.NextValue();
            await TaskEx.Delay(1000);
            logFile.WriteLine("===== LOG TIMER START =====");
            logFile.WriteLine("Total Memory: " + (((new ComputerInfo().TotalPhysicalMemory) / 1024) / 1024) + " MBytes");
            logFile.WriteLine("Used Memory: " + ((Process.GetCurrentProcess().WorkingSet64 / 1024) / 1024) + " MBytes");
            logFile.WriteLine("Free Memory: " + ramCounter.NextValue() + " MBytes");
            logFile.WriteLine("CPU Usage: " + Math.Round(cpuCounter.NextValue()) + "% ");
            logFile.WriteLine("===== LOG TIMER ENDED =====");
        }

        private void _updTimer_Tick(object sender, EventArgs e) {
            if (_cfgCheckUpdates) {
                if (mnuNewUpdate.Visibility != Visibility.Visible) {
                    logFile.WriteLine("MAIN WINDOW - UPDATE TIMER - Checking for updates");
                    var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                    var version = Version.Parse(fvi.FileVersion);
                    var response = Updater.CheckForUpdates(version);
                    if (response != null) {
                        logFile.WriteLine("MAIN WINDOW - UPDATE TIMER - Update found!");
                        mnuNewUpdate.Visibility = Visibility.Visible;
                        mnuNewUpdate.Opacity = 1;
                        mnuNewUpdate.IsEnabled = true;
                    }
                }
            }
        }

        private void _cpmTimer_Tick(object sender, EventArgs e) {
            ++mins;
            if (_cfgShowCPM) {
                game_cpm.Content = "CPM: " + (clicks / mins);
            } else {
                game_cpm.Content = "CPM: Disabled";
            }
        }

        private void _pingTimer_Tick(object sender, EventArgs e) {
            if (_cfgShowPing) {
                th = new Thread(RetrievePing);
                th.Start();
            } else {
                game_ping.Content = "Battle-Servers Ping: Disabled";
            }
        }

        private async void _readerTimer_Tick(object sender, EventArgs e) {
            try {
                switch (_gameSelected) {
                    case 0:
                        if (IsGameRunning(0)) {
                            if (await CopyLog()) {
                                if (!_matchBeingPlayed) {
                                    setStatus("Waiting for a match to start");
                                    FindMatch(0);
                                }

                                if (_matchBeingPlayed && _isListWritten == false) {
                                    setStatus("A match has been found");
                                    playerList.ItemsSource = _players;
                                    repeater = true;
                                    SetUpInfo(0, true);
                                }
                                if (_matchBeingPlayed && _isListWritten) {
                                    if (CheckEnd(0)) {
                                        setStatus("Current match ended");
                                        FactorCreator();
                                        SetUpInfo(0, false);
                                        setStatus("Waiting for the next match");
                                        repeater = true;
                                    }
                                }
                            }
                        } else {
                            setStatus("Company of Heroes is not running!");
                            setStatus("Waiting for the game to start");
                        }
                        break;
                    case 1:
                        if (IsGameRunning(1) && !isLocked) {
                            if (await CopyLog()) {
                                if (!_matchBeingPlayed) {
                                    setStatus("Waiting for a match to start");
                                    FindMatch(1);
                                }
                                if (_matchBeingPlayed && _isListWritten == false) {
                                    _isListWritten = true;
                                    setStatus("A match has been found");
                                    playerList.ItemsSource = _players;
                                    SetUpInfo(1, true);
                                }
                                if (_matchBeingPlayed && _isListWritten) {
                                    if (CheckEnd(1)) {
                                        setStatus("Current match ended");
                                        FactorCreator();
                                        SetUpInfo(1, false);
                                        setStatus("Waiting for the next match");
                                        notificationTooggle = 0;
                                        repeater = true;
                                    }
                                    if (CheckLoaded()) {
                                        _osdList.All(x => x.Hide());
                                        _osdList.Clear();
                                    }
                                }
                            }
                        } else {
                            setStatus("Company of Heroes 2 is not running!");
                            setStatus("Waiting for the game to start");
                        }
                        break;
                    default:
                        _gameSelected = 1;
                        break;
                }
            } catch (Exception ex) {
                logFile.WriteLine("EXCEPTION - READ TIMER: " + ex.ToString());
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
        private bool _cfgOSDEnabled;
        private bool _cfgOSDShowRank = true;
        private bool _cfgOSDForce;
        private bool _cfgOSDShowLevel = true;
        private bool _cfgOSDShowHours = true;
        private bool _cfgOSDUseAnimation = true;
        private Color _cfgOSDColor = Color.LimeGreen;
        private bool _cfgStartGW;
        private int _cfgTimerInt = 1500;
        private bool _cfgWindowTop;
        private int _gameSelected = 1;

        private string ReadV(String section, String key) {
            try {
                return cfg.IniReadValue(section, key);
            } catch (Exception ex) {
                logFile.WriteLine("EXCEPTION Reading Config: " + ex);
            }
            return null;
        }

        private void WriteV(String section, String key, String value) {
            try {
                cfg.IniWriteValue(section, key, value);
            } catch (Exception ex) {
                logFile.WriteLine("EXCEPTION Writing Config: " + ex);
            }
        }

        private Boolean GetConfigs() {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini")) {
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
                _cfgOSDEnabled = ReadV("OSD", "Enabled").ToLower() == "true";
                _cfgOSDShowRank = ReadV("OSD", "ShowRank").ToLower() == "true";
                _cfgOSDShowLevel = ReadV("OSD", "ShowLevel").ToLower() == "true";
                _cfgOSDShowHours = ReadV("OSD", "ShowHours").ToLower() == "true";
                _cfgOSDForce = ReadV("OSD", "Force").ToLower() == "true";
                _cfgOSDUseAnimation = ReadV("OSD", "UseAnimation").ToLower() == "true";
                try {
                    _cfgOSDColor = Color.FromArgb(255, Byte.Parse(ReadV("OSD", "TextColorR")),
                        Byte.Parse(ReadV("OSD", "TextColorG")), Byte.Parse(ReadV("OSD", "TextColorB")));
                } catch (Exception) {
                    _cfgOSDColor = Color.LimeGreen;
                }
                _cfgHistoryEnabled = ReadV("Match_History_Viewer", "Enabled").ToLower() == "true";
                _cfgLsdEnabled = ReadV("Livestream_Displayer", "Enabled").ToLower() == "true";
                _cfgLsdOutput = ReadV("Livestream_Displayer", "OutputFolder");
                _cfgWindowTop = ReadV("Game_Watcher", "WindowTop").ToLower() == "true";
                if (_cfgWindowTop) {
                    mnuItemWindowTop.IsChecked = true;
                } else {
                    mnuItemWindowTop.IsChecked = false;
                }
                return true;
            }
            return false;
        }

        private int GetCOH2Height() {
            if (_gameSelected == 1) {
                var FullText = File.ReadAllLines(_cfgDocPath + @"\configuration_system.lua");
                for (var i = 0; i < FullText.Length; i++) {
                    if (FullText[i].Contains("height")) {
                        return Int32.Parse(Regex.Split(FullText[i + 1], " = ")[1].Replace(",", "").Trim());
                    }
                }
            }
            return -1;
        }

        private int GetCOH2Width() {
            if (_gameSelected == 1) {
                var FullText = File.ReadAllLines(_cfgDocPath + @"\configuration_system.lua");
                for (var i = 0; i < FullText.Length; i++) {
                    if (FullText[i].Contains("width")) {
                        return Int32.Parse(Regex.Split(FullText[i + 1], " = ")[1].Replace(",", "").Trim());
                    }
                }
            }
            return -1;
        }

        #endregion

        #region Load Main

        private readonly Utilities.Log logFile = new Utilities.Log(AppDomain.CurrentDomain.BaseDirectory + @"\logs");
        private readonly PerformanceCounter cpuCounter = new PerformanceCounter();
        private readonly PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        private void Load_Essential() {
            logFile.WriteLine("MAIN WINDOW - START Loading essential");
            pLoadpic.Image = Properties.Resources.preLoader;
            logFile.WriteLine("MAIN WINDOW - START Loading configs");
            if (!GetConfigs()) {
                MessageBox.Show(this, "There was an error loading the configuration files",
                    "Error loading configuration",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                logFile.WriteLine("MAIN WINDOW - Configs ERROR");
            }
            logFile.WriteLine("MAIN WINDOW - Game: " + _gameSelected);
            switch (_gameSelected) {
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

            if (_cfgStartGW) {
                ToggleGW();
            }

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            logFile.WriteLine("MAIN WINDOW - ENDED Loading essential");
        }

        private void Celo_Main_Loaded(object sender, RoutedEventArgs e) {
            logFile.WriteLine("MAIN WINDOW - STARTED");
            try {
                logFile.WriteLine("MAIN WINDOW - Interface FPS => 60");
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 30 }
                    );
                logFile.WriteLine("MAIN WINDOW - Interface FPS => 30");
            } catch (Exception ex) {
                logFile.WriteLine("EXCEPTION: " + ex);
            }
            Load_Essential();

        }

        #endregion

        #region Menus

        private void mnuSendFb_Click(object sender, RoutedEventArgs e) {
            Feedback fb = new Feedback();
            fb.ShowDialog();
        }

        private void mnuDonate_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://www.neffware.com");
        }

        private void mnuMHV_Click(object sender, RoutedEventArgs e) {
            var mhv = new MatchHistoryViewer(_gameSelected, "");
            mhv.ShowDialog();
        }

        private void mnu_ahk_Click(object sender, RoutedEventArgs e) {
            var hk = new HotKeyGen();
            hk.ShowDialog();
        }

        private void mnuHelp_Click(object sender, RoutedEventArgs e) {
            var bt = new About();
            bt.ShowDialog();
        }

        private void mnuReplayManager_Click(object sender, RoutedEventArgs e) {
            var rep = new ReplayManager(_cfgDocPath, _cfgGamePath, _gameSelected);
            rep.ShowDialog();
        }

        private void mnuRestart_Click(object sender, RoutedEventArgs e) {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void mnuItemWindowTop_Click(object sender, RoutedEventArgs e) {
            if (mnuItemWindowTop.IsChecked) {
                Topmost = true;
                WriteV("Game_Watcher", "WindowTop", "true");
            } else {
                Topmost = false;
                WriteV("Game_Watcher", "WindowTop", "false");
            }
        }

        private void playerList_MouseEnter(object sender, MouseEventArgs e) {
            if (playerList.Items.Count > 0 && playerList.SelectedIndex != -1) {
                mnu_p_Copy.IsEnabled = true;
                mnu_p_Open.IsEnabled = true;
            } else {
                mnu_p_Copy.IsEnabled = false;
                mnu_p_Open.IsEnabled = false;
            }
        }

        private void mnu_p_copyNick_Click(object sender, RoutedEventArgs e) {
            if (playerList.SelectedIndex != -1) {
                var n = (Player)playerList.SelectedItem;
                Clipboard.SetText(n.Nickname);
            }
        }

        private void mnu_p_copyRank_Click(object sender, RoutedEventArgs e) {
            if (playerList.SelectedIndex != -1) {
                var n = (Player)playerList.SelectedItem;
                Clipboard.SetText(n.Ranking);
            }
        }

        private void mnu_p_copyTimePlayed_Click(object sender, RoutedEventArgs e) {
            if (playerList.SelectedIndex != -1) {
                var n = (Player)playerList.SelectedItem;
                Clipboard.SetText(n.TimePlayed.ToString());
            }
        }

        private void mnu_p_copyLevel_Click(object sender, RoutedEventArgs e) {
            if (playerList.SelectedIndex != -1) {
                var n = (Player)playerList.SelectedItem;
                Clipboard.SetText(n.Level);
            }
        }

        private void mnu_p_copyID_Click(object sender, RoutedEventArgs e) {
            if (playerList.SelectedIndex != -1) {
                var n = (Player)playerList.SelectedItem;
                Clipboard.SetText(n.SteamID.ToString());
            }
        }

        private void mnu_p_open_coh2org_Click(object sender, RoutedEventArgs e) {
            if (playerList.SelectedIndex != -1) {
                //var n = (Player)playerList.SelectedItem;
                //Process.Start("http://www.coh2.org/ladders/playercard/steamid/" + n.SteamID);
            }
        }

        private void playerList_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            if (playerList.Items.Count > 0 && playerList.SelectedIndex != -1) {
                mnu_p_Copy.IsEnabled = true;
                mnu_p_Open.IsEnabled = true;
            } else {
                mnu_p_Copy.IsEnabled = false;
                mnu_p_Open.IsEnabled = false;
            }
        }

        private void mnu_p_open_coh_Click(object sender, RoutedEventArgs e) {
            if (playerList.SelectedIndex != -1) {
                //var n = (Player)playerList.SelectedItem;
                //Process.Start("http://www.companyofheroes.com/leaderboards#profile/steam/" + n.SteamID);
            }
        }

        private void mnu_p_open_steampage_Click(object sender, RoutedEventArgs e) {
            if (playerList.SelectedIndex != -1) {
                //var n = (Player)playerList.SelectedItem;
                //Process.Start("http://steamcommunity.com/profiles/" + n.SteamID);
            }
        }

        private void playerList_MouseMove(object sender, MouseEventArgs e) {
            if (playerList.Items.Count > 0 && playerList.SelectedIndex != -1) {
                mnu_p_Copy.IsEnabled = true;
                mnu_p_Open.IsEnabled = true;
            } else {
                mnu_p_Copy.IsEnabled = false;
                mnu_p_Open.IsEnabled = false;
            }
        }

        private void mnuPref_Click(object sender, RoutedEventArgs e) {
            var pref = new Preferences();
            pref.ShowDialog();
            GetConfigs();
            switch (_gameSelected) {
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

        private void mnuCheckUpd_Click(object sender, RoutedEventArgs e) {
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var version = Version.Parse(fvi.FileVersion);
            var response = Updater.CheckForUpdates(version);
            if (response != null) {
                if (MessageBox.Show(this,
                    "A new version is available for download (" + response + ")\nDo you wish to update CELO?",
                    "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                    MessageBoxResult.Yes) {
                    var dp = new Updater();
                    dp.ShowDialog();
                }
            } else {
                Utilities.showMessage(this, "CELO Enhanced is up-to-date.", "Update not available");
            }
        }

        private void mnuNewUpdate_Click(object sender, RoutedEventArgs e) {
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var version = Version.Parse(fvi.FileVersion);
            var response = Updater.CheckForUpdates(version);
            if (response != null) {
                if (MessageBox.Show(this,
                    "A new version is available for download (" + response + ")\nDo you wish to update CELO?",
                    "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                    MessageBoxResult.Yes) {
                    var dp = new Updater();
                    dp.ShowDialog();
                }
            } else {
                Utilities.showMessage(this, "CELO Enhanced is up-to-date.", "Update not available");
            }
        }

        private void mnuLogs_Click(object sender, RoutedEventArgs e) {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\logs");
        }

        #endregion

        #region Buttons

        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        private void btn_GameWatcher_Click(object sender, RoutedEventArgs e) {
            ToggleGW();
        }

        private void ToggleGW() {
            if (IsLoaded) {
                if (btn_GameWatcher.Tag.ToString() == "en") {
                    if (_cfgPlaySound) {
                        var NewVolume = (1100);
                        var NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
                        waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
                        var uri = new Uri(@"pack://application:,,,/Resources/beep_01.wav");
                        var player = new SoundPlayer(Application.GetResourceStream(uri).Stream);
                        player.Play();
                    }
                    _readerTimer.IsEnabled = true;
                    LoadFormHost.Visibility = Visibility.Visible;

                    btn_GameWatcher.Tag = "dis";
                    txt_GameWatcher.Text = "Stop Game Watcher";
                    isLocked = false;
                } else {
                    isLocked = true;
                    _matchBeingPlayed = false;
                    _isListWritten = false;
                    isMakingList = false;
                    _readerTimer.IsEnabled = false;
                    _readerTimer.Tick -= _readerTimer_Tick;
                    _readerTimer = null;

                    _readerTimer = new DispatcherTimer(DispatcherPriority.Background);
                    _readerTimer.Interval = TimeSpan.FromMilliseconds(_cfgTimerInt);
                    _readerTimer.IsEnabled = false;
                    _readerTimer.Tick += _readerTimer_Tick;

                    _readerTimer.IsEnabled = true;


                    btn_GameWatcher.Tag = "en";
                    LoadFormHost.Visibility = Visibility.Hidden;
                    txt_GameWatcher.Text = "Start Game Watcher";
                }
            }
        }

        private void btn_ReplayManager_Click(object sender, RoutedEventArgs e) {
            var rep = new ReplayManager(_cfgDocPath, _cfgGamePath, _gameSelected);
            rep.Owner = this;
            rep.ShowDialog();
        }

        private void mnu_lsd_Click(object sender, RoutedEventArgs e) {
            var lsd = new LivestreamDisplayer();
            lsd.ShowDialog();
        }

        #endregion

        #region Main Functions

        public enum Teams {
            Axis,
            Allies
        }

        public static readonly String _AssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly String _copyLogPath = AppDomain.CurrentDomain.BaseDirectory + @"\data\log.tempf";
        private readonly ObservableCollection<Player> _players = new ObservableCollection<Player>();
        private readonly List<FloatingOSDWindow> _osdList = new List<FloatingOSDWindow>();
        private IKeyboardMouseEvents globalHook;
        private bool isMouseHookEnabled = false;
        private bool isKeyboardHookEnabled = false;
        private WebBrowser webFlags;
        private Thread InfoThread;
        private List<String> _logContent = new List<String>();
        private long clicks;
        private int notificationStop, notificationTooggle;
        private Boolean isMakingList;
        private Boolean repeater = true;
        private int curFlag;

        private Boolean IsGameRunning(int game) {
            switch (game) {
                case 0:

                    foreach (var process in Process.GetProcesses()) {
                        if (process.ProcessName.Equals("RelicCOH")) {
                            return true;
                        }
                    }
                    break;
                case 1:
                    foreach (var process in Process.GetProcesses()) {
                        if (process.ProcessName.Equals("RelicCoH2")) {
                            return true;
                        }
                    }
                    break;
                default:
                    foreach (var process in Process.GetProcesses()) {
                        if (process.ProcessName.Equals("RelicCoH2")) {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }

        private async Task<Boolean> CopyLog() {
            var res = await TaskEx.Run(() => CopyLogTask());
            return res;
        }

        private Boolean CopyLogTask() {
            try {
                File.Copy(_cfgDocPath + @"\warnings.log", _copyLogPath, true);
                setLogContents(_gameSelected);
                return true;
            } catch (Exception ex) {
                logFile.WriteLine("EXCEPTION Copying log: " + ex);
                return false;
            }
        }

        private Boolean CheckLoaded() {
            for (var i = _lastLine; i < _logContent.Count; i++) {
                if (_logContent[i].Contains("GAME -- Recording game")) {
                    var str = _logContent[i].Split('.')[0];
                    var str2 = str.Split(':');
                    if (CheckTime(Int32.Parse(str2[0]), Int32.Parse(str2[1]))) {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ClearList() {
            if (_players.Count > 0) {
                try {
                    _players.Clear();
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(playerList.ItemsSource);
                    if (view.GroupDescriptions != null)
                        view.GroupDescriptions.Clear();
                    if (view.SortDescriptions != null)
                        view.SortDescriptions.Clear();
                } catch (Exception ex) {
                    logFile.WriteLine("EXCEPTION Clearing list: " + ex);
                }
            }
        }

        private Boolean CheckTime(int hour, int minute) {
            var currentTime = DateTime.Now.TimeOfDay;
            var gameTime = new TimeSpan(hour, minute, 00);
            var gameTimeLess = gameTime.Subtract(new TimeSpan(0, 2, 0));
            var gameTimeMore = gameTime.Add(new TimeSpan(0, 2, 0));
            if (gameTimeLess <= currentTime && gameTimeMore >= currentTime) {
                return true;
            }
            return false;
        }

        private void CleanLSD() {
            if (_cfgLsdOutput != "") {
                if (_cfgLsdEnabled) {
                    if (!Directory.Exists(_cfgLsdOutput)) {
                        Directory.CreateDirectory(_cfgLsdOutput);
                    }
                    for (var i = 0; i < 8; i++) {
                        var path = _cfgLsdOutput + @"\player_" + (i + 1) + ".txt";
                        if (File.Exists(path)) {
                            try {
                                File.WriteAllText(path, " ", Encoding.UTF8);
                            } catch (Exception ex) {
                                logFile.WriteLine("EXCEPTION Cleaning LSD: " + ex);
                            }
                        }
                    }
                }
            }
        }

        private Boolean CheckEnd(int game) {
            switch (game) {
                case 0:
                    for (var i = _lastLine; i < _logContent.Count; i++) {
                        if (_logContent[i].Contains("APP -- Game Stop")) {
                            var str = _logContent[i].Split('.')[0];
                            var str2 = str.Split(':');
                            if (CheckTime(Int32.Parse(str2[0]), Int32.Parse(str2[1]))) {
                                _matchBeingPlayed = false;
                                _pingTimer.IsEnabled = false;

                                _cpmTimer.IsEnabled = false;
                                CleanLSD();
                                return true;
                            }
                        }
                    }
                    break;
                case 1:
                    for (var i = _lastLine; i < _logContent.Count; i++) {
                        if (_logContent[i].Contains("MOD -- Game Over at")) {
                            var str = _logContent[i].Split('.')[0];
                            var str2 = str.Split(':');
                            if (CheckTime(Int32.Parse(str2[0]), Int32.Parse(str2[1]))) {
                                isMouseHookEnabled = false;
                                _warSpoilTimer.IsEnabled = false;
                                _matchBeingPlayed = false;
                                _pingTimer.IsEnabled = false;

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

        private void setStatus(string text) {
            Dispatcher.Invoke(new Action(() => status_cont_text.Text = text));
        }

        private void setLogContents(int game) {
            switch (game) {
                case 0:
                    _logContent = File.ReadAllLines(_copyLogPath, Encoding.UTF8).ToList();
                    break;
                case 1:
                    if (isSteamIdSearch == false) {
                        try {
                            _logContent = File.ReadAllLines(_copyLogPath, Encoding.UTF8).ToList();
                            for (int i = 0; i < _logContent.Count; i++) {
                                if (_logContent[i].Contains("Found profile: /steam/")) {
                                    string[] str = Regex.Split(_logContent[i], @"/steam/");
                                    mysteamId = Convert.ToInt64(str[1].ToString());
                                    break;
                                }
                            }
                            isSteamIdSearch = true;
                            _logContent.Clear();
                        } catch (Exception ex) {
                            logFile.WriteLine("EXCEPTION - Copy Log - " + ex.ToString());
                        }
                    }


                    try {
                        _logContent = File.ReadLines(_copyLogPath).Reverse().Take(1000).Reverse().ToList();
                    } catch (Exception ex) {
                        logFile.WriteLine("EXCEPTION - Copy Log 2 - " + ex.ToString());
                    }

                    break;
            }
        }

        private async void FindMatch(int game) {
            switch (game) {
                case 0:
                    for (var i = 0; i < _logContent.Count; i++) {
                        var str = _logContent[i].Split('.')[0];
                        try {
                            if (Regex.IsMatch(str.Split(':')[0], @"^\d+$")) {
                                if (CheckTime(Int32.Parse(str.Split(':')[0]),
                                    Int32.Parse(str.Split(':')[1]))) {
                                    if (
                                        _logContent[i].Contains(
                                            "AutomatchInternal::OnStartComplete - detected successful game start") ||
                                        _logContent[i].Contains("RLINK -- Match Started") ||
                                        _logContent[i].Contains("MOD - Setting player")) {
                                        logFile.WriteLine("MAIN WINDOW - READER - Found a game match");
                                        _stopPoint = i - 30;
                                        ProcessLog(game);
                                        break;
                                    }
                                }
                            }
                        } catch (Exception ex) {
                            logFile.WriteLine("EXCEPTION - FIND MATCH - " + ex.ToString());
                        }
                    }
                    break;
                case 1:

                    for (var i = 0; i < _logContent.Count; i++) {
                        var str = _logContent[i].Split('.')[0];
                        try {
                            if (Regex.IsMatch(str.Split(':')[0], @"^\d+$")) {
                                if (CheckTime(Int32.Parse(str.Split(':')[0]),
                                    Int32.Parse(str.Split(':')[1]))) {
                                    if (_logContent[i].Contains("GAME -- Human") ||
                                        _logContent[i].Contains("GAME -- AI Player")) {
                                        logFile.WriteLine("MAIN WINDOW - READER - Found a game match");
                                        _stopPoint = i - 50;
                                        ProcessLog(game);
                                        break;
                                    }
                                }
                            }
                        } catch (Exception ex) {
                            logFile.WriteLine("EXCEPTION - FIND MATCH - " + ex.ToString());
                        }
                    }


                    break;
            }
        }

        private async void ProcessLog(int game) {
            if (isLocked) {
                _readerTimer.IsEnabled = false;
                return;
            }

            _failsafeTimer.Start();
            logFile.WriteLine("MAIN WINDOW - Watcher - Processing Log data - START");
            ClearList();
            pgBarLoading.IsEnabled = true;
            pgBarLoading.IsIndeterminate = true;
            pgBarLoading.Value = 50;
            logFile.WriteLine("MAIN WINDOW - Watcher - Progressbar Enabled");
            switch (game) {
                case 0:
                    if (!_matchBeingPlayed) {
                        int matches = 0, mods = 0, order = 0, z = 0;
                        var isModFirst = false;
                        _lastLine = 0;
                        for (var i = _stopPoint; i < _logContent.Count; i++) {
                            if (_logContent[i].Contains("RLINK -- Match Started")) {
                                isModFirst = false;
                                logFile.WriteLine("MAIN WINDOW - Watcher - (COH1) Mod is NOT first");
                                break;
                            }
                            if (_logContent[i].Contains("MOD - Setting player")) {
                                logFile.WriteLine("MAIN WINDOW - Watcher - (COH1) Mod is first");
                                isModFirst = true;
                                break;
                            }
                        }
                        if (isModFirst) {
                            logFile.WriteLine("MAIN WINDOW - Watcher - Finding players info");
                            for (var i = _stopPoint; i < _logContent.Count; i++) {
                                if (_logContent[i].Contains("MOD - Setting player") &&
                                    Regex.IsMatch(_logContent[i], @"\d+(\.\d+)?$")) {
                                    _matchBeingPlayed = true;
                                    _isListWritten = false;

                                    var str1 = _logContent[i].Substring(48, 1).Trim();
                                    _players.Insert(z, new Player {
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
                            for (var i = _stopPoint; i < _logContent.Count; i++) {
                                if (_logContent[i].Contains("RLINK -- Match Started")) // Setting race
                                {
                                    _matchBeingPlayed = true;
                                    _isListWritten = false;
                                    var sID = Convert.ToInt64(_logContent[i].Substring(65, 17)); // Gets Steam ID
                                    var rank = 0;
                                    try {
                                        rank = Convert.ToInt32(_logContent[i].Substring(105, 6).Trim());
                                    } catch (Exception ex) {
                                        logFile.WriteLine("EXCEPTION Processing log: " + ex);
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
                        } else {
                            z = 0;
                            for (var i = _stopPoint; i < _logContent.Count; i++) {
                                if (_logContent[i].Contains("RLINK -- Match Started")) // Setting race
                                {
                                    var sID = Convert.ToInt64(_logContent[i].Substring(65, 17)); // Gets Steam ID
                                    var rank = 0;
                                    try {
                                        rank = Convert.ToInt32(_logContent[i].Substring(105, 6).Trim());
                                    } catch (Exception ex) {
                                        logFile.WriteLine("EXCEPTION Processing log: " + ex);
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
                            for (var i = _stopPoint; i < _logContent.Count; i++) {
                                if (_logContent[i].Contains("MOD - Setting player") &&
                                    Regex.IsMatch(_logContent[i], @"\d+(\.\d+)?$")) // Setting race
                                {
                                    _matchBeingPlayed = true;
                                    _isListWritten = false;

                                    var race = Int32.Parse(_logContent[i].Substring(48, 1).Trim());
                                    _players[z].Race = race;
                                    mods++;
                                    z++;
                                    _lastLine = i;
                                }
                            }
                        }
                        if (mods > matches) {
                            if (isModFirst) {
                                for (var i = matches; i < mods; i++) {
                                    _players[order].Ranking = "0";
                                    _players[order].SteamID = 0;
                                    order++;
                                }
                            }
                        }
                    }


                    break;
                case 1:

                    if (!_matchBeingPlayed) {
                        try {
                            _lastLine = 0;
                            var z = 0;
                            var slot = 0;

                            String pattern = @"(\w+)\s+(\d+)\s+([\-\d]+)\s+([\w\s+]*)";
                            logFile.WriteLine("MAIN WINDOW - Watcher - Starting Step - START");
                            for (var i = _stopPoint; i < _logContent.Count; i++) {
                                #region Starting Step

                                if (_logContent[i].Contains("GAME -- Human") || _logContent[i].Contains("GAME -- AI Player")) {
                                    _matchBeingPlayed = true;
                                    _isListWritten = false;
                                    var g_race = 0;
                                    Teams g_team;
                                    var is_bot = !(_logContent[i].Contains("GAME -- Human"));

                                    var raw = Regex.Split(_logContent[i], @"Player: \d ")[1];
                                    var reversed = Utilities.reverseString(String.Join(" ", raw));
                                    var matches = Regex.Matches(reversed, pattern, RegexOptions.Multiline);

                                    var groups = matches[0].Groups;

                                    var race = Utilities.reverseString(groups[1].Value);
                                    var team = Utilities.reverseString(groups[2].Value);
                                    var rank = Utilities.reverseString(groups[3].Value);
                                    var nick = Utilities.reverseString(groups[4].Value);

                                    if (race.Equals("aef"))
                                    {
                                        g_race = 3;
                                    }
                                    else if (race.Equals("soviet"))
                                    {
                                        g_race = 1;
                                    }
                                    else if (race.Equals("west_german"))
                                    {
                                        g_race = 2;
                                    }
                                    else if (race.Equals("german"))
                                    {
                                        g_race = 0;
                                    }
                                    else if (race.Equals("british"))
                                    {
                                        g_race = 4;
                                    }

                                    g_team = (g_race == 0 || g_race == 2) ? Teams.Axis : Teams.Allies;
                                    Player pl = new Player()
                                    {
                                        Ranking = rank,
                                        Team = g_team,
                                        Nickname = (is_bot ? "(BOT) " + nick : nick),
                                        Slot = z,
                                        SteamID = -1,
                                        Race = g_race,
                                        TimePlayed = -1,
                                        Level = "N/A"
                                    };

                                    _players.Add(pl);

                                    z++;
                                    _lastLine = i;
                                }

                                #endregion
                            }
                            logFile.WriteLine("MAIN WINDOW - Watcher - Second Step - ENDED");
                        } catch (Exception ex) {
                            logFile.WriteLine("EXCEPTION - Process Log - " + ex.ToString());
                        }
                    }


                    break;
            }
            logFile.WriteLine("MAIN WINDOW - Watcher - Processing Log data - ENDED");
            _matchBeingPlayed = true;
            pgBarLoading.IsEnabled = false;
            pgBarLoading.Value = 0;
        }

        private void SetUpList(int game) {
            isMakingList = true;
            _isListWritten = true;
            currPlayer = 0;
            playerList.ItemsSource = null;

            switch (game) {
                case 0:
                    for (var i = 0; i < _players.Count; i++) {
                        string final = null;
                        currPlayer = i;


                        while (String.IsNullOrEmpty(final)) {
                            if (_players[i].SteamID != 0) {
                                final = Utilities.Steam.getNick(_players[i].SteamID);
                            } else {
                                final = "Computer (CPU)";
                            }
                        }
                        _players[i].Nickname = final;


                        if (_players[i].Ranking == "-1") {
                            _players[i].Ranking = "Unranked (Placements)";
                        } else if (_players[i].Ranking == "0") {
                            _players[i].Ranking = "Unranked (Custom Game)";
                        } else {
                            _players[i].Ranking = _players[i].Ranking;
                        }


                        switch (_players[i].Race) {
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
                                          String.Format(
                                              "Player {0}; Nickname: {1}; Race: {2}; Rank: {3}; SteamID: {4}", i,
                                              _players[i].Nickname, _players[i].RaceName, _players[i].Ranking,
                                              _players[i].SteamID));
                    }


                    break;
                case 1:


                    for (var i = 0; i < _players.Count; i++) {
                        currPlayer = i;

                        _players[i].Country = SourceToImage("Resources/flags/fail.png");
                        _players[i].CountryName = "Unavailable";
                        _players[i].Level = "";
                        _players[i].RankingAfter = "";

                        if (_players[i].Ranking == "-1" || _players[i].Ranking == "-2" || _players[i].Ranking == "0") {
                            _players[i].Ranking = "Unranked";
                        } else {
                            _players[i].Ranking = _players[i].Ranking;
                        }

                        if (_players[i].SteamID == 0) {
                            _players[i].Level = "No Level";
                        }

                        switch (_players[i].Race) {
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
                                          String.Format(
                                              "Player {0}; Nickname: {1}; Race: {2}; Rank: {3}; SteamID: {4}", i,
                                              _players[i].Nickname, _players[i].RaceName, _players[i].Ranking,
                                              _players[i].SteamID));
                    }
                    //RetrieveSteamID(231430);

                    isMakingList = false;

                    break;
            }


            playerList.ItemsSource = _players;
            var view = (CollectionView)CollectionViewSource.GetDefaultView(playerList.ItemsSource);
            if (view != null) {
                view.GroupDescriptions.Clear();
                var groupDescription = new PropertyGroupDescription("Team");
                view.GroupDescriptions.Add(groupDescription);


                view.SortDescriptions.Clear();
                var sort = new SortDescription("Ranking", ListSortDirection.Ascending);
                view.SortDescriptions.Add(sort);
            }


            playerList.Items.Refresh();

            if (_cfgLsdEnabled) {
                RenderLSD();
            }
        }

        private void SetUpMatchInfo(int game) {
            switch (game) {
                case 1:
                    if (IsLoaded) {
                        for (var i = 0; i < _logContent.Count; i++) {
                            if (_logContent[i].Contains("GAME -- Scenario:")) {
                                var scenarioName = (Regex.Split(_logContent[i], "scenarios")[1]);
                                var MapData = File.ReadAllLines(_AssemblyDir + @"\data\maps\coh2\maps.data");
                                foreach (var line in MapData) {
                                    if (!line.StartsWith("#") || !line.Contains("#")) {
                                        var mapf = (Regex.Split(line, "==")[1]);
                                        if (mapf.Equals(scenarioName)) {
                                            match_mapName.Text = Regex.Split(line, "==")[0];
                                            var mapFilename = mapf.Split('\\')[3];
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


                    }

                    break;
            }
            if (_gameSelected == 1) {
                match_map.Content = "Map: " + match_mapName.Text;
            } else {
                match_mapName.Text = "Map: N/A";
            }
        }

        private void SetUpGameInfo(int game) {
            logFile.WriteLine("MAIN WINDOW - Watcher - Setting game info - START");
            try {
                logFile.WriteLine("MAIN WINDOW - Watcher - Ping timer enabled");
                _pingTimer.IsEnabled = true;
                clicks = 0;
                mins = 0;
                isKeyboardHookEnabled = true;
                _warSpoilTimer.IsEnabled = true;
                logFile.WriteLine("MAIN WINDOW - Watcher - Keyboard Hook enabled (Warspoils Drop)");
                isMouseHookEnabled = true;
                logFile.WriteLine("MAIN WINDOW - Watcher - Mouse Hook enabled (CPM)");
                logFile.WriteLine("MAIN WINDOW - Watcher - CPM Timer is enabled");
                _cpmTimer.IsEnabled = true;
                game_cpm.Content = "CPM: 0";
                game_cpmTotal.Content = "Clicks: 0";

                switch (game) {
                    case 0:

                        var file1 = new FileInfo(_cfgGamePath + @"\RelicCoH.exe");
                        if (file1.Exists) {
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
                        if (file2.Exists) {
                            var version = FileVersionInfo.GetVersionInfo(_cfgGamePath + @"\RelicCoH2.exe");
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
            } catch (Exception ex) {
                logFile.WriteLine("EXCEPTION Setting up game info: " + ex);
            }
        }
       

        private async void CreateOSD() {
            if (_gameSelected == 1) {
                if (_cfgOSDEnabled) {
                    var app = GetActiveWindowTitle();
                    if (app != null) {
                        if (app.Contains("Company Of Heroes 2") || app.Contains("Company Of Heroes")) {
                            // 1080p ONLY
                            if ((GetCOH2Width() == 1920 && GetCOH2Height() == 1080) || _cfgOSDForce == true) {
                                _osdList.Clear();
                                var StopMS = 27000;
                                uint animeMs = 2000;
                                var pl_Allies = _players.Where(x => x.Team == Teams.Allies).ToList();
                                var pl_Axis = _players.Where(x => x.Team == Teams.Axis).ToList();

                                var ft = new Font(System.Drawing.FontFamily.GenericSansSerif, 13,
                                    System.Drawing.FontStyle.Bold);

                                try {
                                    for (var i = 0; i < pl_Allies.Count; i++) {
                                        var slot = i;
                                        var height = 108 + (165 * slot) + (25 * slot);
                                        var nOsd = new FloatingOSDWindow();

                                        var pointPos = new Point(210, height);
                                        var bd = new StringBuilder();
                                        if (_cfgOSDShowRank) {
                                            bd.Append(" • Rank: " + pl_Allies[i].Ranking);
                                        }
                                        if (_cfgOSDShowLevel) {
                                            bd.Append(" • Level: " + pl_Allies[i].Level);
                                        }
                                        if (_cfgOSDShowHours) {
                                            bd.Append(" • Hours: " + pl_Allies[i].Ranking);
                                        }
                                        var text = bd.ToString();
                                        if (_cfgOSDUseAnimation == false) {
                                            animeMs = 0;
                                        }
                                        nOsd.Show(pointPos, 245, _cfgOSDColor, ft, StopMS,
                                            FloatingWindow.AnimateMode.Blend,
                                            animeMs, text);

                                        _osdList.Add(nOsd);
                                        await TaskEx.Delay(300);
                                    }


                                    for (var i = 0; i < pl_Axis.Count; i++) {
                                        var slot = i;
                                        var height = 108 + (165 * slot) + (25 * slot);
                                        var nOsd = new FloatingOSDWindow();
                                        var bd = new StringBuilder();
                                        if (_cfgOSDShowRank) {
                                            bd.Append(" • Rank: " + pl_Axis[i].Ranking);
                                        }
                                        if (_cfgOSDShowLevel) {
                                            bd.Append(" • Level: " + pl_Axis[i].Level);
                                        }
                                        if (_cfgOSDShowHours) {
                                            bd.Append(" • Hours: " + pl_Axis[i].Ranking);
                                        }
                                        var text = bd.ToString();
                                        if (_cfgOSDUseAnimation == false) {
                                            animeMs = 0;
                                        }
                                        var pointPos = new Point(1355, height);
                                        nOsd.Show(pointPos, 245, _cfgOSDColor, ft, StopMS,
                                            FloatingWindow.AnimateMode.Blend,
                                            animeMs, text);

                                        _osdList.Add(nOsd);
                                        await TaskEx.Delay(300);
                                    }

                                } catch (Exception ex) {
                                    logFile.WriteLine("EXCEPTION - OSD - " + ex.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RetrievePing() {
            if (Utilities.CheckInternet()) {
                var info = new ProcessStartInfo(_AssemblyDir + @"\data\assemblies\paping.exe");
                var RelicServerIp = "3.227.250.157";
                info.Arguments = "--nocolor -c 3 -p 27020 " + RelicServerIp;

                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.RedirectStandardOutput = true;

                var pr = new Process();
                pr.StartInfo = info;
                var outputList = new List<String>();
                if (pr.Start()) {
                    var sum = 0;
                    while (!pr.StandardOutput.EndOfStream) {
                        outputList.Add(pr.StandardOutput.ReadLine());
                    }
                    foreach (var line in outputList) {
                        if (line.StartsWith("Connected to")) {
                            var t1 = Regex.Split(line, "=");
                            var t2 = Regex.Split(t1[1], "ms");
                            var millis = Int32.Parse(t2[0].Split('.')[0]);
                            sum += millis;
                        }
                    }
                    ping = sum / 3;
                    Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        (MethodInvoker)delegate { game_ping.Content = "Battle-Servers Ping: " + ping + " ms"; });
                }
            }
        }

        private void SetUpInfo(int game, bool setting) {
            logFile.WriteLine("MAIN WINDOW - Watcher - Setting up info - START");
            if (setting) {
                try {
                    logFile.WriteLine("MAIN WINDOW - Watcher - Setting up list - START");
                    SetUpList(game);
                    logFile.WriteLine("MAIN WINDOW - Watcher - Setting up list - ENDED");
                } catch (Exception ex) {
                    logFile.WriteLine("EXCEPTION Setting up list: " + ex);
                }
                setStatus("Information parsed");
                LoadFormHost.Visibility = Visibility.Visible;
                SetUpGameInfo(game);
                try {
                    logFile.WriteLine("MAIN WINDOW - Watcher - Setting up Match Info - START");
                    SetUpMatchInfo(game);
                    logFile.WriteLine("MAIN WINDOW - Watcher - Setting up Match Info - ENDED");
                } catch (Exception ex) {
                    logFile.WriteLine("EXCEPTION Setting up Match info: " + ex);
                }
            } else {
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

        

        #endregion

        #region CoH Functions

        private void RenderLSD() {
            if (_cfgLsdOutput != "") {
                if (_cfgLsdEnabled) {
                    if (!Directory.Exists(_cfgLsdOutput)) {
                        Directory.CreateDirectory(_cfgLsdOutput);
                    }

                    if (File.Exists(_AssemblyDir + @"\lsd.ini") && new FileInfo(_AssemblyDir + @"\lsd.ini").Length > 5) {

                        var lsdcfg = new Utilities.INIFile(_AssemblyDir + @"\lsd.ini");
                        var outputFolder = _cfgLsdOutput;
                        Teams myteam = Teams.Allies;
                        for (var i = 0; i < _players.Count; i++) {
                            var pCont = lsdcfg.IniReadValue("Players", "P" + (i + 1));
                            if (!String.IsNullOrEmpty(pCont)) {

                                var Pass2 = pCont.Replace("%LEVEL%", _players[i].Level)
                                                 .Replace("%STEAMID%", _players[i].SteamID.ToString())
                                                 .Replace("%RANK%", _players[i].Ranking)
                                                 .Replace("%NICK%", _players[i].Nickname)
                                                 .Replace("%FACTION%", _players[i].RaceName.ToString())
                                                 .Replace("%TEAM%", _players[i].Team.ToString())
                                                 .Replace("%HOURS%", _players[i].TimePlayed.ToString());

                                var output = Pass2;
                                try {
                                    File.WriteAllText(outputFolder + @"\player_" + (i + 1) + ".txt", output,
                                        Encoding.UTF8);
                                } catch (Exception ex) {
                                    logFile.WriteLine("EXCEPTION Rendering LSD: " + ex);
                                }
                            }


                        }




                    }
                }
            }
        }

        private async void FactorCreator() {
            logFile.WriteLine("FACTOR CREATOR - START");
            try {
                if (_cfgHistoryEnabled) {
                    await TaskEx.Delay(6500);
                    await GenerateMatchHistory();
                }
                if (_cfgLsdEnabled) {
                    CleanLSD();
                }
                if (_cfgCleanList) {
                    ClearList();
                }
            } catch (Exception ex) {
                logFile.WriteLine("EXCEPTION Creating factors: " + ex);
            }
            logFile.WriteLine("FACTOR CREATOR - END");
        }

        private Task GenerateMatchHistory() {
            logFile.WriteLine("MHV: START");
            return Task.Factory.StartNew(() => {
                try {
                    var dbFile = _AssemblyDir + @"\data\history\coh" + (_gameSelected + 1) + @"\mhv.xml";
                    var repFolder = _AssemblyDir + @"\data\history\coh" + (_gameSelected + 1) + @"\replays";
                    logFile.WriteLine("MHV: DBFILE = " + dbFile);
                    logFile.WriteLine("MHV: REPFOLDER = " + repFolder);

                    if (!File.Exists(dbFile)) {
                        logFile.WriteLine("MHV: FILE NOT FOUND");
                        XmlWriter xWriter = new XmlTextWriter(new StreamWriter(dbFile));
                        xWriter.WriteStartElement("Matches");
                        logFile.WriteLine("MHV: CREATING NEW FILE");
                    }
                    if (File.Exists(dbFile)) {
                        logFile.WriteLine("MHV: FILE FOUND");
                        Directory.CreateDirectory(repFolder);
                        var ReplayFile = _cfgDocPath + @"\playback\temp.rec";
                        var ReplayCopy = Guid.NewGuid() + ".rec";
                        logFile.WriteLine("MHV: REPLAY SAVE NAME = " + ReplayCopy);
                        File.Copy(ReplayFile, repFolder + @"\" + ReplayCopy, true);
                        var gameDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                        var MapFileName = ReplayManager.RetrieveMap(repFolder + @"\" + ReplayCopy, _gameSelected);

                        var document = new XmlDocument();
                        logFile.WriteLine("MHV: LOADING FILE FOR EDIT");
                        document.Load(dbFile);
                        logFile.WriteLine("MHV: LOADED FILE");
                        XmlNode MatchNode = document.CreateElement("Match");
                        XmlNode ReplayNode = document.CreateElement("Replay");
                        ReplayNode.InnerText = ReplayCopy;
                        XmlNode DateNode = document.CreateElement("Date");
                        DateNode.InnerText = gameDate;
                        XmlNode MapNode = document.CreateElement("Map");
                        MapNode.InnerText = MapFileName;
                        XmlNode TypeNode = document.CreateElement("Type");
                        TypeNode.InnerText = (_players.Count / 2).ToString();
                        XmlNode PlayersNode = document.CreateElement("Players");
                        logFile.WriteLine("MHV: APPENDING ELEMENTS");
                        MatchNode.AppendChild(ReplayNode);
                        MatchNode.AppendChild(DateNode);
                        MatchNode.AppendChild(MapNode);
                        MatchNode.AppendChild(TypeNode);
                        MatchNode.AppendChild(PlayersNode);
                        logFile.WriteLine("MHV: ELEMENTS ADDED");
                        logFile.WriteLine("MHV: WRITING TO ELEMENTS");
                        for (var i = 0; i < _players.Count; i++) {
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
                        logFile.WriteLine("MHV: ELEMENTS NOW HAVE VALUES");
                        document.DocumentElement.AppendChild(MatchNode);
                        logFile.WriteLine("MHV: SAVING DB FILE");
                        try {
                            document.Save(dbFile);
                            logFile.WriteLine("MHV: FILE SAVED");
                        } catch (Exception ex) {
                            logFile.WriteLine("EXCEPTION at XML save: " + ex.ToString());
                        }


                    }
                } catch (Exception ex) {
                    logFile.WriteLine("EXCEPTION writing to XML (MHV): " + ex);
                }
                logFile.WriteLine("MHV: END");
            });

        }

        #endregion
    }
}