﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = System.Windows.Forms.Application;
using Color = System.Drawing.Color;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        private readonly Boolean isFirstTime;
        private readonly Utilities.INIFile mainINI;
        private int _SelectedGame = 1;
        private string COH1_DocPath = "";
        private string COH1_GamePath = "";
        private string COH2_DocPath = "";
        private string COH2_GamePath = "";
        private Color OsdColor = Color.LimeGreen;

        public Preferences(bool first = false)
        {
            InitializeComponent();
            isFirstTime = first;
            mainINI = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
        }

        private void prefWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            RefreshPaths();
            rectangle_colorPick.Fill =
                new SolidColorBrush(System.Windows.Media.Color.FromRgb(OsdColor.R, OsdColor.G, OsdColor.B));
        }

        private void LoadSettings()
        {
            try
            {
                _SelectedGame = Int32.Parse(ReadV(mainINI, "Essencial", "Game"));
            }
            catch (Exception)
            {
                _SelectedGame = 1;
            }
            if (!isFirstTime)
            {
                COH1_GamePath = ReadV(mainINI, "Essencial", "CoH1_GamePath");
                COH1_DocPath = ReadV(mainINI, "Essencial", "CoH1_DocPath");
                COH2_GamePath = ReadV(mainINI, "Essencial", "CoH2_GamePath");
                COH2_DocPath = ReadV(mainINI, "Essencial", "CoH2_DocPath");
                check_osd_enabled.IsChecked = ReadV(mainINI, "OSD", "Enabled").ToLower() == "true";
                check_osd_rank.IsChecked = ReadV(mainINI, "OSD", "ShowRank").ToLower() == "true";
                check_osd_level.IsChecked = ReadV(mainINI, "OSD", "ShowLevel").ToLower() == "true";
                check_osd_hours.IsChecked = ReadV(mainINI, "OSD", "ShowHours").ToLower() == "true";
                check_osd_animation.IsChecked = ReadV(mainINI, "OSD", "UseAnimation").ToLower() == "true";
                check_osd_force.IsChecked = ReadV(mainINI, "OSD", "Force").ToLower() == "true";
                try
                {
                    rectangle_colorPick.Fill =
                        new SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(Byte.Parse(ReadV(mainINI, "OSD", "TextColorR")),
                                Byte.Parse(ReadV(mainINI, "OSD", "TextColorG")),
                                Byte.Parse(ReadV(mainINI, "OSD", "TextColorB"))));
                    OsdColor = Color.FromArgb(255, Byte.Parse(ReadV(mainINI, "OSD", "TextColorR")),
                        Byte.Parse(ReadV(mainINI, "OSD", "TextColorG")), Byte.Parse(ReadV(mainINI, "OSD", "TextColorB")));
                }
                catch (Exception)
                {
                    rectangle_colorPick.Fill =
                        new SolidColorBrush(System.Windows.Media.Color.FromRgb(OsdColor.R, OsdColor.G, OsdColor.B));
                }

                Main_checkShowStart.IsChecked = ReadV(mainINI, "Main", "ShowStartScreen").ToLower() == "true";
                Main_checkUpdateAuto.IsChecked = ReadV(mainINI, "Main", "CheckForUpdates").ToLower() == "true";
                GW_checkCleanPlayers.IsChecked = ReadV(mainINI, "Game_Watcher", "CleanPlayers").ToLower() == "true";
                GW_checkGWStart.IsChecked = ReadV(mainINI, "Game_Watcher", "AutoStart").ToLower() == "true";
                GW_checkWindowTop.IsChecked = ReadV(mainINI, "Game_Watcher", "WindowTop").ToLower() == "true";
                GW_checkPlaySound.IsChecked = ReadV(mainINI, "Game_Watcher", "PlaySound").ToLower() == "true";
                GW_checkBSPing.IsChecked = ReadV(mainINI, "Game_Watcher", "Show_Ping").ToLower() == "true";
                GW_checkCPM.IsChecked = ReadV(mainINI, "Game_Watcher", "Show_CPM").ToLower() == "true";
                LSD_checkEnableFeature.IsChecked = ReadV(mainINI, "Livestream_Displayer", "Enabled").ToLower() == "true";
                LSD_tBoxOutputFolder.Text = ReadV(mainINI, "Livestream_Displayer", "OutputFolder");
                Main_checkHardwareAcc.IsChecked = ReadV(mainINI, "Main", "HardwareAcceleration").ToLower() == "true";
                check_match_chat.IsChecked = ReadV(mainINI, "ReplayManager", "ShowChat").ToLower() == "true";
                MatchHistoryViewer_checkEnableFeature.IsChecked =
                    ReadV(mainINI, "Match_History_Viewer", "Enabled").ToLower() == "true";

                if (ReadV(mainINI, "ReplayManager", "RememberUser").ToLower() == "true")
                {
                    ReplayManager_checkRememberUser.IsChecked = true;
                    if (!String.IsNullOrEmpty(ReadV(mainINI, "ReplayManager", "Username")))
                    {
                        ReplayManager_tBoxUsername.Text =
                            Utilities.SimpleTripleDES.Decrypt3DES(ReadV(mainINI, "ReplayManager", "Username"),
                                "xCb54nZs235mi8", true);
                    }
                    if (ReadV(mainINI, "ReplayManager", "RememberPass").ToLower() == "true")
                    {
                        ReplayManager_checkRememberPassword.IsChecked = true;
                        if (!String.IsNullOrEmpty(ReadV(mainINI, "ReplayManager", "Password")))
                        {
                            ReplayManager_tBoxPassword.Password =
                                Utilities.SimpleTripleDES.Decrypt3DES(ReadV(mainINI, "ReplayManager", "Password"),
                                    "xCb54nZs235mi8", true);
                        }
                    }
                }
            }
            else
            {
                check_osd_enabled.IsChecked = true;
                check_osd_rank.IsChecked = true;
                check_osd_level.IsChecked = true;
                check_osd_hours.IsChecked = true;
                check_osd_animation.IsChecked = true;
            }
        }

        private void Main_btnDeleteConfigs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                foreach (var file in dir.GetFiles("*.ini", SearchOption.TopDirectoryOnly))
                {
                    file.Delete();
                }
                File.WriteAllText(MainWindow._AssemblyDir + @"\data\first.txt", "");
            }
            catch
            {
            }
            finally
            {
                Utilities.showMessage(this, "Configurations have been reset. CELO is now going to restart",
                    "Restart application");
                Application.Restart();
                Environment.Exit(0);
            }
        }

        private void LSD_btnBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            var fd = new FolderBrowserDialog();
            using (fd)
            {
                fd.Description = "Select a folder to save player files";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LSD_tBoxOutputFolder.Text = fd.SelectedPath;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            applySettings();
        }

        private void applySettings()
        {
            if (ValidatePaths())
            {
                #region Essencial

                WriteV(mainINI, "Essencial", "Game", Essential_cBoxGame.SelectedIndex.ToString());
                WriteV(mainINI, "Essencial", "COH1_GamePath", COH1_GamePath);
                WriteV(mainINI, "Essencial", "COH1_DocPath", COH1_DocPath);
                WriteV(mainINI, "Essencial", "COH2_GamePath", COH2_GamePath);
                WriteV(mainINI, "Essencial", "COH2_DocPath", COH2_DocPath);
                if (isFirstTime)
                {
                    File.Delete(MainWindow._AssemblyDir + @"\data\first.txt");
                }

                #endregion

                #region Main

                WriteV(mainINI, "Main", "ShowStartScreen", Main_checkShowStart.IsChecked.ToString());
                WriteV(mainINI, "Main", "CheckForUpdates", Main_checkUpdateAuto.IsChecked.ToString());
                WriteV(mainINI, "Main", "HardwareAcceleration", Main_checkHardwareAcc.IsChecked.ToString());

                #endregion

                #region Game Watcher

                WriteV(mainINI, "Game_Watcher", "CleanPlayers", GW_checkCleanPlayers.IsChecked.ToString());
                WriteV(mainINI, "Game_Watcher", "AutoStart", GW_checkGWStart.IsChecked.ToString());
                WriteV(mainINI, "Game_Watcher", "WindowTop", GW_checkWindowTop.IsChecked.ToString());
                WriteV(mainINI, "Game_Watcher", "PlaySound", GW_checkPlaySound.IsChecked.ToString());
                WriteV(mainINI, "Game_Watcher", "Show_Ping", GW_checkBSPing.IsChecked.ToString());
                WriteV(mainINI, "Game_Watcher", "Show_CPM", GW_checkCPM.IsChecked.ToString());

                #endregion

                #region ReplayManager

                WriteV(mainINI, "ReplayManager", "ShowChat", check_match_chat.IsChecked.ToString());
                if (ReplayManager_checkRememberUser.IsChecked == true && ReplayManager_tBoxUsername.Text.Length > 0)
                {
                    WriteV(mainINI, "ReplayManager", "RememberUser",
                        ReplayManager_checkRememberUser.IsChecked.ToString());
                    WriteV(mainINI, "ReplayManager", "Username",
                        Utilities.SimpleTripleDES.Encrypt3DES(ReplayManager_tBoxUsername.Text, "xCb54nZs235mi8", true));
                    if (ReplayManager_checkRememberPassword.IsChecked == true &&
                        ReplayManager_tBoxPassword.Password.Length > 0)
                    {
                        WriteV(mainINI, "ReplayManager", "RememberPass",
                            ReplayManager_checkRememberPassword.IsChecked.ToString());
                        WriteV(mainINI, "ReplayManager", "Password",
                            Utilities.SimpleTripleDES.Encrypt3DES(ReplayManager_tBoxPassword.Password, "xCb54nZs235mi8",
                                true));
                    }
                }

                #endregion

                #region Livestream Displayer

                WriteV(mainINI, "Livestream_Displayer", "Enabled", LSD_checkEnableFeature.IsChecked.ToString());
                WriteV(mainINI, "Livestream_Displayer", "OutputFolder", LSD_tBoxOutputFolder.Text);

                #endregion

                #region Match History Viewer

                WriteV(mainINI, "Match_History_Viewer", "Enabled",
                    MatchHistoryViewer_checkEnableFeature.IsChecked.ToString());

                #endregion

                #region OSD

                WriteV(mainINI, "OSD", "Enabled", check_osd_enabled.IsChecked.ToString());
                WriteV(mainINI, "OSD", "ShowRank", check_osd_rank.IsChecked.ToString());
                WriteV(mainINI, "OSD", "ShowLevel", check_osd_level.IsChecked.ToString());
                WriteV(mainINI, "OSD", "ShowHours", check_osd_hours.IsChecked.ToString());
                WriteV(mainINI, "OSD", "UseAnimation", check_osd_animation.IsChecked.ToString());
                WriteV(mainINI, "OSD", "TextColorR", OsdColor.R.ToString());
                WriteV(mainINI, "OSD", "TextColorG", OsdColor.G.ToString());
                WriteV(mainINI, "OSD", "TextColorB", OsdColor.B.ToString());
                WriteV(mainINI, "OSD", "Force", check_osd_force.IsChecked.ToString());

                #endregion
            }
        }

        private void WriteV(Utilities.INIFile inf, String section, String key, String value)
        {
            try
            {
                inf.IniWriteValue(section, key, value);
            }
            catch (Exception)
            {
            }
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

        private Boolean ValidatePaths()
        {
            Boolean test1 = false, test2 = false;
            if (COH1_DocPath != "" & COH1_GamePath != "")
            {
                test1 = true;
            }
            else
            {
                test1 = false;
            }
            if (COH2_DocPath != "" & COH2_GamePath != "")
            {
                test2 = true;
            }
            else
            {
                test2 = false;
            }
            if (test1 || test2)
            {
                return true;
            }
            return false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            applySettings();

            Hide();
        }

        private void prefWindow_Closed(object sender, EventArgs e)
        {
        }

        private void ReplayManager_checkRememberPassword_Click(object sender, RoutedEventArgs e)
        {
            if (ReplayManager_checkRememberPassword.IsChecked == true &&
                ReplayManager_checkRememberUser.IsChecked == true)
            {
                ReplayManager_tBoxPassword.IsEnabled = true;
            }
            else if (ReplayManager_checkRememberPassword.IsChecked == false)
            {
                ReplayManager_tBoxPassword.IsEnabled = false;
                ReplayManager_tBoxPassword.Password = "";
            }
        }

        private void ReplayManager_checkRememberUser_Click(object sender, RoutedEventArgs e)
        {
            if (ReplayManager_checkRememberUser.IsChecked == false)
            {
                ReplayManager_checkRememberPassword.IsChecked = false;
                ReplayManager_checkRememberPassword.IsEnabled = false;
                ReplayManager_tBoxUsername.IsEnabled = false;
                ReplayManager_tBoxUsername.Text = "";
            }
            else
            {
                ReplayManager_checkRememberPassword.IsEnabled = true;

                ReplayManager_tBoxUsername.IsEnabled = true;
                ReplayManager_tBoxPassword.IsEnabled = false;
                ReplayManager_checkRememberPassword.IsChecked = false;
                ReplayManager_tBoxUsername.Text = "";
                ReplayManager_tBoxPassword.Password = "";
            }
        }

        private void ReplayManager_tBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void ReplayManager_tBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
        }

        private void Main_checkHardwareAcc_Click(object sender, RoutedEventArgs e)
        {
            Utilities.showMessage(this, "Please restart CELO so that Hardware Acceleration changes take effect.",
                "Restart required");
        }

        private void btnChooseColor_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                var dialog = new ColorDialog();
                dialog.AllowFullOpen = true;
                dialog.AnyColor = true;
                dialog.Color = OsdColor;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    rectangle_colorPick.Fill =
                        new SolidColorBrush(System.Windows.Media.Color.FromRgb(dialog.Color.R, dialog.Color.G,
                            dialog.Color.B));
                    OsdColor = Color.FromArgb(255, dialog.Color.R, dialog.Color.G, dialog.Color.B);
                }
            }
        }

        #region Essential

        private Boolean isDocValid;
        private Boolean isGameValid;

        private void Essential_btnBrowseDocuments_Click(object sender, RoutedEventArgs e)
        {
            var fd = new FolderBrowserDialog();
            using (fd)
            {
                fd.Description = "Find game folder in Documents\\my games";
                fd.ShowNewFolderButton = false;
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var DocDir = fd.SelectedPath;
                    Essential_tBoxDocumentsPath.Text = DocDir;
                }
            }
        }

        private void Essential_cBoxGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                if (Essential_cBoxGame.SelectedIndex == 0)
                {
                    Essential_ImgGame.Source =
                        new BitmapImage(new Uri(@"pack://application:,,,/Resources/coh1_icon.png"));
                    _SelectedGame = 0;
                }
                else
                {
                    Essential_ImgGame.Source =
                        new BitmapImage(new Uri(@"pack://application:,,,/Resources/coh2_icon.png"));
                    _SelectedGame = 1;
                }
                RefreshPaths();
            }
        }

        private void RefreshPaths()
        {
            Essential_txtValidateGame.Foreground = new SolidColorBrush(Colors.Red);
            Essential_txtValidateGame.Text = "Path is Invalid!";
            Essential_txtDocumentValidate.Foreground = new SolidColorBrush(Colors.Red);
            Essential_txtDocumentValidate.Text = "Path is Invalid!";
            isDocValid = false;
            isGameValid = false;
            Essential_tBoxDocumentsPath.Text = "";
            Essential_tBoxGamesPath.Text = "";

            if (_SelectedGame == 0)
            {
                Essential_tBoxDocumentsPath.Text = COH1_DocPath;
                Essential_tBoxGamesPath.Text = COH1_GamePath;
            }
            else if (_SelectedGame == 1)
            {
                Essential_tBoxDocumentsPath.Text = COH2_DocPath;
                Essential_tBoxGamesPath.Text = COH2_GamePath;
            }
        }

        private void Essential_tBoxDocumentsPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Boolean test1 = false, test2 = false, test3 = false;
                var dir = new DirectoryInfo(Essential_tBoxDocumentsPath.Text);
                switch (_SelectedGame)
                {
                    case 0:
                        if (dir.Exists)
                        {
                            var files = dir.GetFiles();
                            foreach (var file in files)
                            {
                                if (file.Name.Contains("Local.ini"))
                                {
                                    test1 = true;
                                }
                                if (file.Name.Contains("configuration.lua"))
                                {
                                    test2 = true;
                                }
                                if (file.Name.Contains("playercfg.lua"))
                                {
                                    test3 = true;
                                }
                            }


                            if (test1 && test2 && test3)
                            {
                                Essential_txtDocumentValidate.Foreground = new SolidColorBrush(Colors.ForestGreen);
                                Essential_txtDocumentValidate.Text = "Path Validated!";
                                COH1_DocPath = Essential_tBoxDocumentsPath.Text;
                                isDocValid = true;
                            }
                            else
                            {
                                Essential_txtDocumentValidate.Foreground = new SolidColorBrush(Colors.Red);
                                Essential_txtDocumentValidate.Text = "Path is Invalid!";
                                isDocValid = false;
                            }
                        }
                        else
                        {
                            Essential_txtDocumentValidate.Foreground = new SolidColorBrush(Colors.Red);
                            Essential_txtDocumentValidate.Text = "Path is Invalid!";
                            isDocValid = false;
                        }
                        break;

                    case 1:

                        if (dir.Exists)
                        {
                            var files = dir.GetFiles();
                            foreach (var file in files)
                            {
                                if (file.Name.Contains("configuration_system.lua"))
                                {
                                    test1 = true;
                                }
                                if (file.Name.Contains("warnings.log"))
                                {
                                    test2 = true;
                                }
                                if (file.Name.Contains("local.ini"))
                                {
                                    test3 = true;
                                }
                            }

                            if (test1 && test2 && test3)
                            {
                                Essential_txtDocumentValidate.Foreground = new SolidColorBrush(Colors.ForestGreen);
                                Essential_txtDocumentValidate.Text = "Path Validated!";
                                COH2_DocPath = Essential_tBoxDocumentsPath.Text;
                                isDocValid = true;
                            }
                            else
                            {
                                Essential_txtDocumentValidate.Foreground = new SolidColorBrush(Colors.Red);
                                Essential_txtDocumentValidate.Text = "Path is Invalid!";
                                isDocValid = false;
                            }
                        }
                        else
                        {
                            Essential_txtDocumentValidate.Foreground = new SolidColorBrush(Colors.Red);
                            Essential_txtDocumentValidate.Text = "Path is Invalid!";
                            isDocValid = false;
                        }
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        private void Essential_tBoxGamesPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var dir = new DirectoryInfo(Essential_tBoxGamesPath.Text);
                Boolean test1 = false, test2 = false;
                switch (_SelectedGame)
                {
                    case 0:
                        if (dir.Exists)
                        {
                            var files = dir.GetFiles();
                            foreach (var file in files)
                            {
                                if (file.Name.Contains("RelicCOH.exe"))
                                {
                                    test1 = true;
                                }
                                if (file.Name.Contains("RelicCOHO.exe"))
                                {
                                    test2 = true;
                                }
                            }

                            if (test1 && test2)
                            {
                                Essential_txtValidateGame.Foreground = new SolidColorBrush(Colors.ForestGreen);
                                Essential_txtValidateGame.Text = "Path Validated!";
                                COH1_GamePath = Essential_tBoxGamesPath.Text;
                                isGameValid = true;
                            }
                            else
                            {
                                Essential_txtValidateGame.Foreground = new SolidColorBrush(Colors.Red);
                                Essential_txtValidateGame.Text = "Path is Invalid!";
                                isGameValid = false;
                            }
                        }
                        else
                        {
                            Essential_txtValidateGame.Foreground = new SolidColorBrush(Colors.Red);
                            Essential_txtValidateGame.Text = "Path is Invalid!";
                            isGameValid = false;
                        }
                        break;
                    case 1:
                        if (dir.Exists)
                        {
                            var files = dir.GetFiles();
                            foreach (var file in files)
                            {
                                if (file.Name.Contains("RelicCoH2.exe"))
                                {
                                    test1 = true;
                                }
                                if (file.Name.Contains("sysconfig.lua"))
                                {
                                    test2 = true;
                                }
                            }

                            if (test1 && test2)
                            {
                                Essential_txtValidateGame.Foreground = new SolidColorBrush(Colors.ForestGreen);
                                Essential_txtValidateGame.Text = "Path Validated!";
                                COH2_GamePath = Essential_tBoxGamesPath.Text;
                                isGameValid = true;
                            }
                            else
                            {
                                Essential_txtValidateGame.Foreground = new SolidColorBrush(Colors.Red);
                                Essential_txtValidateGame.Text = "Path is Invalid!";
                                isGameValid = false;
                            }
                        }
                        else
                        {
                            Essential_txtValidateGame.Foreground = new SolidColorBrush(Colors.Red);
                            Essential_txtValidateGame.Text = "Path is Invalid!";
                            isGameValid = false;
                        }
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        private void Essential_btnBrowseGame_Click(object sender, RoutedEventArgs e)
        {
            var fd = new FolderBrowserDialog();
            using (fd)
            {
                fd.Description = "Find game folder in Steam/SteamApps/common folder";
                fd.ShowNewFolderButton = false;
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var GameDir = fd.SelectedPath;
                    Essential_tBoxGamesPath.Text = GameDir;
                }
            }
        }

        #endregion
    }
}