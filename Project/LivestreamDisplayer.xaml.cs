using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for LivestreamDisplayer.xaml
    /// </summary>
    public partial class LivestreamDisplayer : Window
    {
        private readonly String[] PlayersContent = new String[8];
        private int A1;
        private String After = "";
        private String Last = "";
        private String OutputFolder = "";
        private Utilities.INIFile cfg, cfg2;
        private Boolean derp;

        public LivestreamDisplayer()
        {
            InitializeComponent();
        }


        private void DisplayerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            cfg = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\lsd.ini");
            cfg2 = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
            if (cfg2.IniReadValue("Livestream_Displayer", "Enabled").ToLower() == "false")
            {
                if (MessageBox.Show(this,
                    "Livestream Displayer feature is currently disabled.\nDo you want to enable it?",
                    "Livestream Displayer", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                    MessageBoxResult.Yes)
                {
                    cfg2.IniWriteValue("Livestream_Displayer", "Enabled", "true");
                }
                else
                {
                    Close();
                }
            }
            if (!String.IsNullOrEmpty(cfg2.IniReadValue("Livestream_Displayer", "OutputFolder")))
            {
                OutputFolder = cfg2.IniReadValue("Livestream_Displayer", "OutputFolder");
            }
            else
            {
                OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                           @"\CELO\Livestream Displayer";
            }
            Directory.CreateDirectory(OutputFolder);
            
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\lsd.ini"))
            {
                for (int p = 0; p < PlayersContent.Length; p++)
                {
                    PlayersContent[p] = cfg.IniReadValue("Players", "P" + (p + 1));
                }
                
            }
            else
            {
                for (int p = 0; p < PlayersContent.Length; p++)
                {
                    PlayersContent[p] = "%NICK% Ladder rank is %RANK% with a total of %HOURS% hours played";
                }
                tBox_FileContent.Text = "%NICK% Ladder rank is %RANK% with a total of %HOURS% hours played";

                FixText();
                for (int i = 0; i < PlayersContent.Length; i++)
                {
                    cfg.IniWriteValue("Players", "P" + (i + 1), tBox_FileContent.Text);
                }
                for (int p = 0; p < PlayersContent.Length; p++)
                {
                    File.WriteAllText(OutputFolder + @"\player_" + (p + 1) + ".txt", "", Encoding.UTF8);
                }
            }

            for (int p = 0; p < PlayersContent.Length; p++)
            {
                PlayersContent[p] = cfg.IniReadValue("Players", "P" + (p + 1));
            }
            tBox_FileContent.Text = PlayersContent[playerListBox.SelectedIndex];
            Last = PlayersContent[playerListBox.SelectedIndex];
            tBox_path.Text = OutputFolder;
            cfg2.IniWriteValue("Livestream_Displayer", "OutputFolder", tBox_path.Text);
            
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fd = new FolderBrowserDialog();
            fd.Description = "Select a path to store the players text files";
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tBox_path.Text = fd.SelectedPath;
                OutputFolder = fd.SelectedPath;
                for (int p = 0; p < PlayersContent.Length; p++)
                {
                    File.WriteAllText(OutputFolder + @"\player_" + (p + 1) + ".txt", "", Encoding.UTF8);
                }
            }
        }

        private void btnCreateAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FixText();
                for (int i = 0; i < PlayersContent.Length; i++)
                {
                    cfg.IniWriteValue("Players", "P" + (i + 1), tBox_FileContent.Text);
                }
                for (int p = 0; p < PlayersContent.Length; p++)
                {
                    PlayersContent[p] = cfg.IniReadValue("Players", "P" + (p + 1));
                }
                Utilities.showMessage(this, "All players files have been saved.", "Saved");
                for (int p = 0; p < PlayersContent.Length; p++)
                {
                    File.WriteAllText(OutputFolder + @"\player_" + (p + 1) + ".txt", "", Encoding.UTF8);
                }
            }
            catch (Exception)
            {
                Utilities.showError(this, "Error while saving players files, Please check the output folder.");
            }
            finally
            {
                Last = "";
                After = "";
            }
        }

        private void playerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!derp)
            {
                try
                {
                    if (IsLoaded)
                    {
                        if (Last != After)
                        {
                            if (
                                MessageBox.Show(this,
                                    "Changes have been made to the current player, are you sure you want to change without saving?",
                                    "Player Selection Save", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                                MessageBoxResult.Yes)
                            {
                            }
                            else
                            {
                                derp = true;
                                playerListBox.SelectedIndex = A1;
                                tBox_FileContent.Text = After;
                                return;
                            }
                        }
                        for (int p = 0; p < PlayersContent.Length; p++)
                        {
                            PlayersContent[p] = cfg.IniReadValue("Players", "P" + (p + 1));
                        }
                        tBox_FileContent.Text = PlayersContent[playerListBox.SelectedIndex];
                        Last = PlayersContent[playerListBox.SelectedIndex];
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                derp = false;
            }
        }

        private void btnCreateSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FixText();
                cfg.IniWriteValue("Players", "P" + (playerListBox.SelectedIndex + 1), tBox_FileContent.Text);
                for (int p = 0; p < PlayersContent.Length; p++)
                {
                    PlayersContent[p] = cfg.IniReadValue("Players", "P" + (p + 1));
                }
                Utilities.showMessage(this, "Player " + (playerListBox.SelectedIndex + 1) + " file has been saved.",
                    "Saved");
                for (int p = 0; p < PlayersContent.Length; p++)
                {
                    File.WriteAllText(OutputFolder + @"\player_" + (p + 1) + ".txt", "", Encoding.UTF8);
                }
            }
            catch (Exception)
            {
                Utilities.showError(this, "Error while saving players files, Please check the output folder.");
            }
            finally
            {
                Last = "";
                After = "";
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            cfg2.IniWriteValue("Livestream_Displayer", "OutputFolder", tBox_path.Text);
            for (int i = 0; i < PlayersContent.Length; i++)
            {
                cfg.IniWriteValue("Players", "P" + (i + 1), PlayersContent[i]);
            }
            for (int p = 0; p < PlayersContent.Length; p++)
            {
                File.WriteAllText(OutputFolder + @"\player_" + (p + 1) + ".txt", "", Encoding.UTF8);
            }
            Close();
        }

        private void tBox_FileContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlayersContent[playerListBox.SelectedIndex] = tBox_FileContent.Text;
            After = tBox_FileContent.Text;
            A1 = playerListBox.SelectedIndex;
        }

        private void FixText()
        {
            String[] Keys = {"RANK", "NICK", "STEAMID", "HOURS", "LEVEL"};
            foreach (string word in Keys)
            {
                if (tBox_FileContent.Text.Contains("%" + word.ToLower() + "%"))
                {
                    tBox_FileContent.Text = tBox_FileContent.Text.Replace("%" + word.ToLower() + "%", "%" + word + "%");
                }
            }
        }
    }
}