using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Ionic.Zip;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for HotKeyGenxaml.xaml
    /// </summary>
    public partial class HotKeyGen : Window
    {
        private readonly Dictionary<int, ComboBox> cb1 = new Dictionary<int, ComboBox>();
        private readonly Dictionary<int, ComboBox> cb2 = new Dictionary<int, ComboBox>();
        private readonly Dictionary<int, ComboBox> cb3 = new Dictionary<int, ComboBox>();
        private readonly Dictionary<int, DuoCombos> connectionBox = new Dictionary<int, DuoCombos>();
        private readonly Dictionary<ComboBox, String> defaultValues = new Dictionary<ComboBox, String>();

        private readonly String filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                           @"\" + Guid.NewGuid() + "_ahkcompiler.zip";

        private readonly Dictionary<string, string> helpKeys = new Dictionary<string, string>();
        private readonly Dictionary<string, string> possKeys = new Dictionary<string, string>();

        public HotKeyGen()
        {
            InitializeComponent();
        }

        public Boolean CheckAutoHotKey()
        {
            if (Directory.Exists(MainWindow._AssemblyDir + @"\data\assemblies\ahk"))
            {
                if (Directory.GetFiles(MainWindow._AssemblyDir + @"\data\assemblies\ahk").Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void HotKey_Generator_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CheckAutoHotKey())
            {
                if (MessageBox.Show(this,
                    "CELO will now download AutoHotkey Script compiler. Do you want to continue?",
                    "AutoHotkey Script Compiler", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                    MessageBoxResult.Yes)
                {
                    try
                    {
                        var wb = new WebClient();
                        wb.DownloadFileCompleted += wb_DownloadFileCompleted;
                        using (wb)
                        {
                            wb.DownloadFileAsync(
                                new Uri("http://www.neffware.com/downloads/celo/data/AHK_Compiler.zip"), filename);
                        }
                        var ls = new LoadingScreen(this, "Downloading & Installing AutoHotkey Script Compiler...",
                            CheckAutoHotKey);
                        ls.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Utilities.showError(this,
                            "Could not download or install script compiler. Please try again later");
                    }
                }
                else
                {
                    Close();
                }
            }
            FillKeys();
            FillHelpers();
            FillComboBoxes_1();
            FillComboBoxes_2();
            FillDefaults();
            ApplyDefaults();
            FillConnections();

            var inif = new Utilities.INIFile(MainWindow._AssemblyDir + @"\config.ini");
            tBoxOutput.Text = inif.IniReadValue("HotKey", "Output");
        }

        private void wb_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                var zipFile = new ZipFile(filename);

                Directory.CreateDirectory(MainWindow._AssemblyDir + @"\data\assemblies\ahk");

                zipFile.ExtractAll(MainWindow._AssemblyDir + @"\data\assemblies\ahk");
                MessageBox.Show("Installation Complete!");
            }
            catch (Exception exception)
            {
                Utilities.showError(this,
                    "Error occured while installing AutoHotKey Script Compiler\n" + exception.StackTrace);
            }
            
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to reset all keys?", "Reset", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                ApplyDefaults();
                foreach (var box in cb3)
                {
                    box.Value.SelectedIndex = 0;
                }
                Utilities.showMessage(this, "Reset complete!", "Reset");
            }
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (tBoxOutput.Text == "")
            {
                Utilities.showMessage(this,
                    "You have not provided an output for the compiled script!\nPlease select a folder now or go to options later.",
                    "Output folder");
                btnBrowseOutput_Click(btnBrowseOutput, null);
            }
            try
            {
                var TempFileName = Path.GetTempPath() + Guid.NewGuid() + ".ahk";
                var ScriptCompilerPath = MainWindow._AssemblyDir + @"\data\assemblies\ahk\Ahk2Exe.exe";
                var OutputPath = tBoxOutput.Text;

                if (!Directory.Exists(OutputPath))
                {
                    Directory.CreateDirectory(OutputPath);
                }
                var builder = new StringBuilder();
                if (File.Exists(ScriptCompilerPath) && Directory.Exists(OutputPath))
                {
                    builder.AppendLine("#IfWinActive, Company Of Heroes");
                    foreach (var duo in connectionBox)
                    {
                        var key = "";
                        var content = "";
                        if (GetCBoxValue(duo.Value.MainBox) != duo.Value.DefaultKey)
                        {
                            foreach (var possKey in possKeys)
                            {
                                if (GetCBoxValue(duo.Value.MainBox) == possKey.Key)
                                {
                                    key = possKey.Value;
                                    break;
                                }
                            }

                            if (GetCBoxValue(duo.Value.HelperBox) != "None")
                            {
                                switch (GetCBoxValue(duo.Value.HelperBox))
                                {
                                    case @"+ Control":
                                        key = "^" + key;
                                        break;
                                    case @"+ Shift":
                                        key = "+" + key;
                                        break;
                                    case @"+ Alt":
                                        key = "!" + key;
                                        break;
                                }
                            }
                            if (!String.IsNullOrEmpty(key))
                            {
                                if ((string) duo.Value?.MainBox?.Tag?.ToString() == "suspend")
                                {
                                    content = key + "::Suspend";
                                    builder.AppendLine(content);
                                }
                                else
                                {
                                    content = key + "::" + duo.Value.Key;
                                    builder.AppendLine(content);
                                }
                            }
                        }
                        else
                        {
                            if (GetCBoxValue(duo.Value.HelperBox) != "None")
                            {
                                key = duo.Value.Key;

                                switch (GetCBoxValue(duo.Value.HelperBox))
                                {
                                    case @"+ Control":
                                        key = "^" + key;
                                        break;
                                    case @"+ Shift":
                                        key = "+" + key;
                                        break;
                                    case @"+ Alt":
                                        key = "!" + key;
                                        break;
                                }
                            }
                            if (!String.IsNullOrEmpty(key))
                            {
                                if ((string)duo.Value?.MainBox?.Tag?.ToString() == "suspend")
                                {
                                    content = key + "::Suspend";
                                    builder.AppendLine(content);
                                }
                                else
                                {
                                    content = key + "::" + duo.Value.Key;
                                    builder.AppendLine(content);
                                }
                            }
                        }
                    }
                    Random rng = new Random();
                    string rn = rng.Next(0, 99999).ToString();
                    var sc = new ScriptCompiler(OutputPath + @"\CoH_Keys_" + rn + ".exe", ScriptCompilerPath, TempFileName, builder);
                    sc.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private string GetCBoxValue(ComboBox cb)
        {
            return cb.SelectionBoxItem.ToString();
        }

        private void btnBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            var fd = new FolderBrowserDialog();
            using (fd)
            {
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tBoxOutput.Text = fd.SelectedPath;
                    var inif = new Utilities.INIFile(MainWindow._AssemblyDir + @"\config.ini");
                    inif.IniWriteValue("HotKey", "Output", tBoxOutput.Text);
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private class DuoCombos
        {
            public ComboBox MainBox { get; set; }
            public ComboBox HelperBox { get; set; }
            public string DefaultKey { get; set; }
            public string Key { get; set; }
        }

        #region Keys

        private void FillComboBoxes_1()
        {
            cb1.Add(0, cBox_c1);
            cb1.Add(1, cBox_c2);
            cb1.Add(2, cBox_c3);
            cb1.Add(3, cBox_c4);
            cb1.Add(4, cBox_c5);
            cb1.Add(5, cBox_c6);
            cb1.Add(6, cBox_c7);
            cb1.Add(7, cBox_c8);
            cb1.Add(8, cBox_c9);
            cb1.Add(9, cBox_sus);
            foreach (var comboBox in cb1)
            {
                foreach (var possKey in possKeys)
                {
                    comboBox.Value.Items.Add(possKey.Key);
                }
            }
        }

        private void FillComboBoxes_2()
        {
            cb2.Add(0, cBox_o1);
            cb2.Add(1, cBox_o2);
            cb2.Add(2, cBox_o3);
            cb2.Add(3, cBox_o4);
            cb2.Add(4, cBox_o5);
            cb2.Add(5, cBox_o6);
            cb2.Add(6, cBox_o7);
            cb2.Add(7, cBox_o8);
            cb2.Add(8, cBox_o9);
            foreach (var comboBox in cb2)
            {
                foreach (var possKey in possKeys)
                {
                    comboBox.Value.Items.Add(possKey.Key);
                }
            }
        }

        private void FillHelpers()
        {
            cb3.Clear();
            cb3.Add(0, cBox_Helper_1_1);
            cb3.Add(1, cBox_Helper_1_2);
            cb3.Add(2, cBox_Helper_1_3);
            cb3.Add(3, cBox_Helper_1_4);
            cb3.Add(4, cBox_Helper_1_5);
            cb3.Add(5, cBox_Helper_1_6);
            cb3.Add(6, cBox_Helper_1_7);
            cb3.Add(7, cBox_Helper_1_8);
            cb3.Add(8, cBox_Helper_1_9);
            cb3.Add(10, cBox_Helper_2_1);
            cb3.Add(11, cBox_Helper_2_2);
            cb3.Add(12, cBox_Helper_2_3);
            cb3.Add(13, cBox_Helper_2_4);
            cb3.Add(14, cBox_Helper_2_5);
            cb3.Add(15, cBox_Helper_2_6);
            cb3.Add(16, cBox_Helper_2_7);
            cb3.Add(17, cBox_Helper_2_8);
            cb3.Add(18, cBox_Helper_2_9);
            cb3.Add(19, cBox_sus_1);
            foreach (var val in cb3)
            {
                foreach (var helpKey in helpKeys)
                {
                    val.Value.Items.Add(helpKey.Key);
                }

                val.Value.SelectedIndex = 0;
            }
        }

        private void FillKeys()
        {
            helpKeys.Clear();
            helpKeys.Add("None", "");
            helpKeys.Add(@"+ Control", @"^");
            helpKeys.Add(@"+ Shift", @"+");
            helpKeys.Add(@"+ Alt", @"!");
            possKeys.Clear();
            possKeys.Add("A", "A".ToLower());
            possKeys.Add("B", "B".ToLower());
            possKeys.Add("C", "C".ToLower());
            possKeys.Add("D", "D".ToLower());
            possKeys.Add("E", "E".ToLower());
            possKeys.Add("F", "F".ToLower());
            possKeys.Add("G", "G".ToLower());
            possKeys.Add("H", "H".ToLower());
            possKeys.Add("I", "I".ToLower());
            possKeys.Add("J", "J".ToLower());
            possKeys.Add("K", "K".ToLower());
            possKeys.Add("L", "L".ToLower());
            possKeys.Add("M", "M".ToLower());
            possKeys.Add("O", "O".ToLower());
            possKeys.Add("P", "P".ToLower());
            possKeys.Add("Q", "Q".ToLower());
            possKeys.Add("R", "R".ToLower());
            possKeys.Add("S", "S".ToLower());
            possKeys.Add("T", "T".ToLower());
            possKeys.Add("U", "U".ToLower());
            possKeys.Add("V", "V".ToLower());
            possKeys.Add("X", "X".ToLower());
            possKeys.Add("Y", "Y".ToLower());
            possKeys.Add("W", "W".ToLower());
            possKeys.Add("Z", "Z".ToLower());
            possKeys.Add("0", "0");
            possKeys.Add("1", "1");
            possKeys.Add("2", "2");
            possKeys.Add("3", "3");
            possKeys.Add("4", "4");
            possKeys.Add("5", "5");
            possKeys.Add("6", "6");
            possKeys.Add("7", "7");
            possKeys.Add("8", "8");
            possKeys.Add("9", "9");
            possKeys.Add("F1", "F1");
            possKeys.Add("F2", "F2");
            possKeys.Add("F3", "F3");
            possKeys.Add("F4", "F4");
            possKeys.Add("F5", "F5");
            possKeys.Add("F6", "F6");
            possKeys.Add("F7", "F7");
            possKeys.Add("F8", "F8");
            possKeys.Add("F9", "F9");
            possKeys.Add("F10", "F10");
            possKeys.Add("F11", "F11");
            possKeys.Add("F12", "F12");
            possKeys.Add("Alt", "Alt");
            possKeys.Add("Apostrophe", "'");
            possKeys.Add("Backslash", @"\");
            possKeys.Add("Backspace", "Backspace");
            possKeys.Add("Caps Lock", "CapsLock");
            possKeys.Add("Comma", ",");
            possKeys.Add("Control", "Control");
            possKeys.Add("Delete", "Delete");
            possKeys.Add("Down", "Down");
            possKeys.Add("Up", "Up");
            possKeys.Add("Left", "Left");
            possKeys.Add("Right", "Right");
            possKeys.Add("End", "End");
            possKeys.Add("Enter", "Enter");
            possKeys.Add("Equal", "=");
            possKeys.Add("Home", "Home");
            possKeys.Add("Insert", "Insert");
            possKeys.Add("Minus", "-");
            possKeys.Add("Num Lock", "NumLock");
            possKeys.Add("Page Down", "PgDn");
            possKeys.Add("Page Up", "PgUp");
            possKeys.Add("Period", ".");
            possKeys.Add("PrintScreen", "PrintScreen");
            possKeys.Add("Scroll Lock", "ScrollLock");
            possKeys.Add("Shift", "Shift");
            possKeys.Add("Slash", "/");
            possKeys.Add("Space", "Space");
            possKeys.Add("Tab", "Tab");
            possKeys.Add("Numpad 0", "Numpad0");
            possKeys.Add("Numpad 1", "Numpad1");
            possKeys.Add("Numpad 2", "Numpad2");
            possKeys.Add("Numpad 3", "Numpad3");
            possKeys.Add("Numpad 4", "Numpad4");
            possKeys.Add("Numpad 5", "Numpad5");
            possKeys.Add("Numpad 6", "Numpad6");
            possKeys.Add("Numpad 7", "Numpad7");
            possKeys.Add("Numpad 8", "Numpad8");
            possKeys.Add("Numpad 9", "Numpad9");
            possKeys.Add("Numpad Minus", "NumpadSub");
            possKeys.Add("Numpad Multiply", "NumpadMult");
            possKeys.Add("Numpad Period", "NumpadDot");
            possKeys.Add("Numpad Plus", "NumpadAdd");
            possKeys.Add("Numpad Slash", "NumpadDiv");
        }

        private void FillDefaults()
        {
            defaultValues.Add(cBox_o1, "G");
            defaultValues.Add(cBox_o2, "A");
            defaultValues.Add(cBox_o3, "O");
            defaultValues.Add(cBox_o4, "E");
            defaultValues.Add(cBox_o5, "T");
            defaultValues.Add(cBox_o6, "U");
            defaultValues.Add(cBox_o7, "S");
            defaultValues.Add(cBox_o8, "D");
            defaultValues.Add(cBox_o9, "R");
            defaultValues.Add(cBox_c1, "Backspace");
            defaultValues.Add(cBox_c2, "Page Up");
            defaultValues.Add(cBox_c3, "Page Down");
            defaultValues.Add(cBox_c4, "Left");
            defaultValues.Add(cBox_c5, "Right");
            defaultValues.Add(cBox_c6, "Up");
            defaultValues.Add(cBox_c7, "Down");
            defaultValues.Add(cBox_c8, "Alt");
            defaultValues.Add(cBox_c9, "Numpad 0");
            defaultValues.Add(cBox_sus, "F11");
        }

        private void ApplyDefaults()
        {
            foreach (var value in defaultValues)
            {
                value.Key.SelectedValue = value.Value;
            }
        }

        private void FillConnections()
        {
            connectionBox.Clear();
            connectionBox.Add(0,
                new DuoCombos {MainBox = cBox_o1, HelperBox = cBox_Helper_1_1, DefaultKey = "G", Key = "g"});
            connectionBox.Add(1,
                new DuoCombos {MainBox = cBox_o2, HelperBox = cBox_Helper_1_2, DefaultKey = "A", Key = "a"});
            connectionBox.Add(2,
                new DuoCombos {MainBox = cBox_o3, HelperBox = cBox_Helper_1_3, DefaultKey = "O", Key = "o"});
            connectionBox.Add(3,
                new DuoCombos {MainBox = cBox_o4, HelperBox = cBox_Helper_1_4, DefaultKey = "E", Key = "e"});
            connectionBox.Add(4,
                new DuoCombos {MainBox = cBox_o5, HelperBox = cBox_Helper_1_5, DefaultKey = "T", Key = "t"});
            connectionBox.Add(5,
                new DuoCombos {MainBox = cBox_o6, HelperBox = cBox_Helper_1_6, DefaultKey = "U", Key = "u"});
            connectionBox.Add(6,
                new DuoCombos {MainBox = cBox_o7, HelperBox = cBox_Helper_1_7, DefaultKey = "S", Key = "s"});
            connectionBox.Add(7,
                new DuoCombos {MainBox = cBox_o8, HelperBox = cBox_Helper_1_4, DefaultKey = "D", Key = "d"});
            connectionBox.Add(8,
                new DuoCombos {MainBox = cBox_o9, HelperBox = cBox_Helper_1_4, DefaultKey = "R", Key = "r"});
            connectionBox.Add(9,
                new DuoCombos {MainBox = cBox_c1, HelperBox = cBox_Helper_2_1, DefaultKey = "Backspace", Key = "BS"});
            connectionBox.Add(10,
                new DuoCombos {MainBox = cBox_c2, HelperBox = cBox_Helper_2_2, DefaultKey = "Page Up", Key = "PgUp"});
            connectionBox.Add(11,
                new DuoCombos {MainBox = cBox_c3, HelperBox = cBox_Helper_2_3, DefaultKey = "Page Down", Key = "PgDn"});
            connectionBox.Add(12,
                new DuoCombos {MainBox = cBox_c4, HelperBox = cBox_Helper_2_4, DefaultKey = "Left", Key = "Left"});
            connectionBox.Add(13,
                new DuoCombos {MainBox = cBox_c5, HelperBox = cBox_Helper_2_5, DefaultKey = "Right", Key = "Right"});
            connectionBox.Add(14,
                new DuoCombos {MainBox = cBox_c6, HelperBox = cBox_Helper_2_6, DefaultKey = "Up", Key = "Up"});
            connectionBox.Add(15,
                new DuoCombos {MainBox = cBox_c7, HelperBox = cBox_Helper_2_7, DefaultKey = "Down", Key = "Down"});
            connectionBox.Add(16,
                new DuoCombos {MainBox = cBox_c8, HelperBox = cBox_Helper_2_8, DefaultKey = "Alt", Key = "Alt"});
            connectionBox.Add(17,
                new DuoCombos {MainBox = cBox_c9, HelperBox = cBox_Helper_2_9, DefaultKey = "Numpad 0", Key = "Numpad0"});
            connectionBox.Add(18,
                new DuoCombos { MainBox = cBox_sus, HelperBox = cBox_sus_1, DefaultKey = "", Key = "F11" });
        }

        #endregion
    }
}