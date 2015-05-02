using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        private readonly Utilities.INIFile cfgFile;
        private readonly SoundPlayer sp;
        private int SelectedGame;

        public StartWindow()
        {
            InitializeComponent();

            cfgFile = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
            if (cfgFile.IniReadValue("Main", "CheckForUpdates").ToLower() == "true")
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
            }
            if (cfgFile.IniReadValue("Main", "ShowStartScreen").ToLower() == "false")
            {
                var mn = new MainWindow();
                mn.Show();
                Close();
            }
            sp = new SoundPlayer(Properties.Resources.btnStart);
            sp.Load();
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            sp.PlaySync();
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/GameWatcher_1.png"));
            imgGameWatcher.Source = sr;
        }

        private void imgGameWatcher_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/GameWatcher_0.png"));
            imgGameWatcher.Source = sr;
        }

        private void imgRepManager_MouseEnter(object sender, MouseEventArgs e)
        {
            sp.PlaySync();
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/ReplayManager_1.png"));
            imgRepManager.Source = sr;
        }

        private void imgRepManager_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/ReplayManager_0.png"));
            imgRepManager.Source = sr;
        }

        private void imgMHV_MouseEnter(object sender, MouseEventArgs e)
        {
            sp.PlaySync();
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/MatchHistoryViewer_1.png"));
            imgMHV.Source = sr;
        }

        private void imgMHV_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/MatchHistoryViewer_0.png"));
            imgMHV.Source = sr;
        }

        private void imgLSD_MouseEnter(object sender, MouseEventArgs e)
        {
            sp.PlaySync();
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/LSD_1.png"));
            imgLSD.Source = sr;
        }

        private void imgLSD_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/LSD_0.png"));
            imgLSD.Source = sr;
        }


        private void imgGameWatcher_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isFirstTime())
            {
                MessageBox.Show(this,
                    "It seems that this is your first run with CELO.\nIn order for it to work please go to settings and fill out the Essencial information.",
                    "Essencial information", MessageBoxButton.OK, MessageBoxImage.Information);
                var pr = new Preferences(true);
                pr.ShowDialog();
            }
            else
            {
                setcfg();
                SelectedGame = cBoxGame.SelectedIndex;
                var mn = new MainWindow();
                Hide();
                mn.Show();
                mn.Focus();
            }
        }

        private void imgRepManager_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isFirstTime())
            {
                MessageBox.Show(this,
                    "It seems that this is your first run with CELO.\nIn order for it to work please go to settings and fill out the Essencial information.",
                    "Essencial information", MessageBoxButton.OK, MessageBoxImage.Information);
                var pr = new Preferences(true);
                pr.ShowDialog();
            }
            else
            {
                setcfg();
                SelectedGame = cBoxGame.SelectedIndex;
                var rp = new ReplayManager(cfgFile.IniReadValue("Essencial", "CoH" + (SelectedGame + 1) + "_DocPath"),
                    cfgFile.IniReadValue("Essencial", "CoH" + (SelectedGame + 1) + "_GamePath"), SelectedGame);
                rp.ShowDialog();
            }
        }

        private void imgMHV_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isFirstTime())
            {
                MessageBox.Show(this,
                    "It seems that this is your first run with CELO.\nIn order for it to work please go to settings and fill out the Essencial information.",
                    "Essencial information", MessageBoxButton.OK, MessageBoxImage.Information);
                var pr = new Preferences(true);
                pr.ShowDialog();
            }
            else
            {
                setcfg();
                SelectedGame = cBoxGame.SelectedIndex;
                var mhv = new MatchHistoryViewer(SelectedGame,
                    cfgFile.IniReadValue("Essencial", "CoH" + (SelectedGame + 1) + "_DocPath"));
                mhv.ShowDialog();
            }
        }

        private void imgLSD_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isFirstTime())
            {
                MessageBox.Show(this,
                    "It seems that this is your first run with CELO.\nIn order for it to work please go to settings and fill out the Essencial information.",
                    "Essencial information", MessageBoxButton.OK, MessageBoxImage.Information);
                var pr = new Preferences(true);
                pr.ShowDialog();
            }
            else
            {
                setcfg();
                SelectedGame = cBoxGame.SelectedIndex;
                var lsd = new LivestreamDisplayer();
                lsd.ShowDialog();
            }
        }

        private bool isFirstTime()
        {
            bool te = File.Exists(MainWindow._AssemblyDir + @"\data\first.txt");
            return te;
        }

        private void startWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (isFirstTime())
            {
                MessageBox.Show(this,
                    "It seems that this is your first run with CELO.\nIn order for it to work please go to settings and fill out the Essencial information.",
                    "Essencial information", MessageBoxButton.OK, MessageBoxImage.Information);
                var pr = new Preferences(true);
                pr.ShowDialog();
            }
            try
            {
                cBoxGame.SelectedIndex = Int32.Parse(cfgFile.IniReadValue("Essencial", "Game"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            SelectedGame = cBoxGame.SelectedIndex;
        }

        private void setcfg()
        {
            if (cBoxDontShow.IsChecked == true)
            {
                cfgFile.IniWriteValue("Main", "ShowStartScreen", "false");
            }
            else
            {
                cfgFile.IniWriteValue("Main", "ShowStartScreen", "true");
            }

            cfgFile.IniWriteValue("Essencial", "Game", cBoxGame.SelectedIndex.ToString());
            SelectedGame = cBoxGame.SelectedIndex;
        }

        private void cBoxDontShow_Checked(object sender, RoutedEventArgs e)
        {
            if (cBoxDontShow.IsChecked == true)
            {
                cfgFile.IniWriteValue("Main", "ShowStartScreen", "false");
            }
            else
            {
                cfgFile.IniWriteValue("Main", "ShowStartScreen", "true");
            }
        }

        private void cBoxGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                cfgFile.IniWriteValue("Essencial", "Game", cBoxGame.SelectedIndex.ToString());
        }

        private void imgHotKey_MouseEnter(object sender, MouseEventArgs e)
        {
            sp.PlaySync();
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/HotKey_1.png"));
            imgHotKey.Source = sr;
        }

        private void imgHotKey_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageSource sr = new BitmapImage(new Uri(@"pack://application:,,,/Resources/HotKey_0.png"));
            imgHotKey.Source = sr;
        }

        private void imgHotKey_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isFirstTime())
            {
                MessageBox.Show(this,
                    "It seems that this is your first run with CELO.\nIn order for it to work please go to settings and fill out the Essencial information.",
                    "Essencial information", MessageBoxButton.OK, MessageBoxImage.Information);
                var pr = new Preferences(true);
                pr.ShowDialog();
            }
            else
            {
                setcfg();
                SelectedGame = cBoxGame.SelectedIndex;
                if (SelectedGame == 0)
                {
                    Utilities.showWarning(this,
                        "Hot-Key Creator was made exclusively for CoH2, it will not work as intended for CoH1.");
                }
                var Ht = new HotKeyGen();
                Ht.ShowDialog();
            }
        }
    }
}