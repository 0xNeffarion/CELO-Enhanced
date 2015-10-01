using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CELO_Enhanced
{
    public partial class VersionChanger : Window
    {
        private readonly string DocPath;
        private readonly string FileName;
        private readonly int Game = 1;
        private readonly string GamePath;
        private readonly string Name;
        private readonly string Version;
        private int LastV;

        public VersionChanger(string filename, string name, string version, string gamePath, string docPath, int game)
        {
            InitializeComponent();
            FileName = filename;
            Name = name;
            Version = version;
            GamePath = gamePath;
            DocPath = docPath;
            Game = game;
        }

        private void VersionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txt_original_name.Content = "Name: " + Name;
            txt_original_version.Content = "Version: " + Version;
            String verz = null;
            if (Game == 0)
            {
                var file = new FileInfo(GamePath + @"\RelicCoH.exe");
                if (file.Exists)
                {
                    verz = FileVersionInfo.GetVersionInfo(GamePath + @"\RelicCoH.exe").FileVersion;
                }
            }
            else
            {
                var file = new FileInfo(GamePath + @"\RelicCoH2.exe");
                if (file.Exists)
                {
                    verz = FileVersionInfo.GetVersionInfo(GamePath + @"\RelicCoH2.exe").FileVersion;
                }
            }
            txt_lastVersion.Content = "Last Version: " + Environment.NewLine + verz;
            LastV = Int32.Parse(verz.Replace("4.0.0.", ""));
        }

        private void rBtn_backup_Checked(object sender, RoutedEventArgs e)
        {
            if (tBox_name != null)
            {
                tBox_name.IsEnabled = true;
            }
            if (tBox_version.Text != "" && tBox_name.Text != "")
            {
                btn_apply.IsEnabled = true;
            }
            else
            {
                btn_apply.IsEnabled = false;
            }
        }

        private void rBtn_overwrite_Checked(object sender, RoutedEventArgs e)
        {
            if (tBox_name != null)
            {
                tBox_name.IsEnabled = false;
                tBox_name.Text = "";
                if (tBox_version.Text != "")
                {
                    btn_apply.IsEnabled = true;
                }
                else
                {
                    btn_apply.IsEnabled = false;
                }
            }
        }

        private void tBox_version_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rBtn_backup.IsChecked == true)
            {
                if (!string.IsNullOrEmpty(tBox_name.Text) && tBox_version.Text.Length == 5)
                {
                    btn_apply.IsEnabled = true;
                }
                else
                {
                    btn_apply.IsEnabled = false;
                }
            }
            else
            {
                if (tBox_version.Text.Length == 5)
                {
                    btn_apply.IsEnabled = true;
                }
                else
                {
                    btn_apply.IsEnabled = false;
                }
            }
        }

        private void btn_lastVersion_Click(object sender, RoutedEventArgs e)
        {
            tBox_version.Text = LastV.ToString();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void tBox_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rBtn_backup.IsChecked == true)
            {
                if (!string.IsNullOrEmpty(tBox_name.Text) && tBox_version.Text.Length == 5)
                {
                    btn_apply.IsEnabled = true;
                }
                else
                {
                    btn_apply.IsEnabled = false;
                }
            }
            else
            {
                btn_apply.IsEnabled = true;
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_apply_Click(object sender, RoutedEventArgs e)
        {
            var rng = new Random();
            var tempName = "temp_rec" + rng.Next(1, 90000) + ".bin";
            var OriginalPath = DocPath + @"\playback\" + FileName;
            var TempPath = DocPath + @"\playback\" + tempName;

            try
            {
                if (rBtn_backup.IsChecked == true)
                {
                    if (tBox_name.Text != "" && tBox_version.Text.Length == 5)
                    {
                        File.Copy(OriginalPath, TempPath, true);
                        File.Copy(OriginalPath, DocPath + @"\playback\" + tBox_name.Text + ".rec", true);
                        var stream = File.Open(TempPath, FileMode.Open, FileAccess.ReadWrite);
                        byte[] gameVersion = {0, 0};
                        var WantedVersion =
                            Utilities.Convertions.StringToByteArray(
                                Utilities.Convertions.IntToHex(Int32.Parse((tBox_version.Text))));
                        gameVersion[0] = WantedVersion[1];
                        gameVersion[1] = WantedVersion[0];
                        using (stream)
                        {
                            stream.Seek(2, SeekOrigin.Begin);
                            stream.Write(gameVersion, 0, 2);
                            stream.Close();
                        }
                        File.Copy(TempPath, OriginalPath, true);
                        File.Delete(TempPath);
                        Utilities.showMessage(this, "Version has been changed!\nA backup replay has also been created",
                            "Success");
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(tBox_name.Text))
                        {
                            Utilities.showError(this, "Please insert replay backup name.");
                        }
                        if (tBox_version.Text.Length != 5)
                        {
                            Utilities.showError(this, "Please insert replay version (5 digits).");
                        }
                    }
                }
                else
                {
                    if (tBox_version.Text.Length == 5)
                    {
                        File.Copy(OriginalPath, TempPath, true);
                        var stream = File.Open(TempPath, FileMode.Open, FileAccess.ReadWrite);
                        byte[] gameVersion = {0, 0};
                        var WantedVersion =
                            Utilities.Convertions.StringToByteArray(
                                Utilities.Convertions.IntToHex(Int32.Parse((tBox_version.Text))));
                        gameVersion[0] = WantedVersion[1];
                        gameVersion[1] = WantedVersion[0];
                        using (stream)
                        {
                            stream.Seek(2, SeekOrigin.Begin);
                            stream.Write(gameVersion, 0, 2);
                            stream.Close();
                        }
                        File.Copy(TempPath, OriginalPath, true);
                        File.Delete(TempPath);
                        Utilities.showMessage(this, "Version has been changed!", "Success");
                    }
                    else
                    {
                        if (tBox_version.Text.Length != 5)
                        {
                            Utilities.showError(this, "Please insert replay version (5 digits).");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.showError(this, "Error while changing version!\nError information:\n" + ex.StackTrace);
            }
            finally
            {
                Close();
            }
        }
    }
}