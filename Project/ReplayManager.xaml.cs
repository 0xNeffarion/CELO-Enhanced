using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace CELO_Enhanced
{
    public partial class ReplayManager : Window
    {
        private readonly string doc_path;
        private readonly StringBuilder errorNames = new StringBuilder();
        private readonly string exe_path;
        private readonly int game;
        private readonly ObservableCollection<Replay> replays = new ObservableCollection<Replay>();
        private FileSystemWatcher fsw;
        private Boolean pauseWatch;
        private int searchMethod;

        public ReplayManager(string doc, string exe, int game)
        {
            InitializeComponent();
            doc_path = doc;
            exe_path = exe;
            this.game = game;
        }

        private void CleanInfo()
        {
            pic_map.InitialImage = new Bitmap(MainWindow._AssemblyDir + @"data\maps\unknown.png");
            pic_map.Image = pic_map.InitialImage;
            txt_name.Content = "Name: ";
            txt_mapname.Content = "Map: ";
            txt_version.Content = "Version: ";
            txt_date.Content = "Date: ";
            txt_time.Content = "Time: ";
        }

        private void InitializeFileSystemWatcher()
        {
            fsw = new FileSystemWatcher(doc_path + @"\playback\");
            fsw.NotifyFilter = NotifyFilters.FileName |
                               NotifyFilters.LastWrite |
                               NotifyFilters.Size |
                               NotifyFilters.DirectoryName
                               | NotifyFilters.CreationTime;
            fsw.Created += Fsw_Created;
            fsw.Deleted += Fsw_Deleted;
            fsw.Renamed += Fsw_Renamed;
            fsw.EnableRaisingEvents = true;
        }

        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            if (!pauseWatch)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => LoadList()));
            }
        }

        private void Fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            if (!pauseWatch)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => LoadList()));
            }
        }

        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            if (!pauseWatch)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => LoadList()));
            }
        }

        private void DetectReplays()
        {
            errorNames.Clear();
            switch (game)
            {
                case 0:
                    ParseCOH_Replays();
                    break;
                case 1:
                    ParseCOH2_Replays();
                    break;
                default:
                    ParseCOH2_Replays();
                    break;
            }
            if (errorNames.Length > 0)
            {
                pic_error.Visibility = Visibility.Visible;
            }
        }

        private void SetUpTag()
        {
            try
            {
                DateTime today = DateTime.Today;
                DateTime yesterday = DateTime.Today.AddDays(-1);

                int index = 0;
                foreach (Replay replay in replays)
                {
                    var file = new FileInfo(doc_path + @"\playback\" + replay.name + ".rec");
                    DateTime matchDay;
                    String daz = replay.game_date.Replace("~ ", "");
                    try
                    {
                        matchDay = DateTime.Parse(daz);
                    }
                    catch (FormatException)
                    {
                        matchDay = file.LastWriteTime;
                    }
                    replays[index].tag = "Other days";
                    if (matchDay.Day == today.Day && matchDay.Day > yesterday.Day) // replay made today
                    {
                        replays[index].tag = "Today";
                    }
                    else if (matchDay.Day == yesterday.Day && matchDay.Day < today.Day) // replay yest
                    {
                        replays[index].tag = "Yesterday";
                    }
                    else if (matchDay.Day < today.Day && matchDay.Day < yesterday.Day) // other
                    {
                        replays[index].tag = "Other days";
                    }
                    index++;
                }
                replayList.ItemsSource = replays;
                ICollectionView view = CollectionViewSource.GetDefaultView(replayList.ItemsSource);
                PropertyGroupDescription groupDescription = groupDescription = new PropertyGroupDescription("tag");
                view.GroupDescriptions.Clear();
                view.GroupDescriptions.Add(groupDescription);
                if (replayList.Items.Count > 0)
                {
                    if (replayList.SelectedIndex == -1)
                    {
                        replayList.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void LoadList()
        {
            DetectReplays();
            SetUpTag();
        }

        public static string RetrieveMap(string replay, int game)
        {
            FileStream fs = File.OpenRead(replay);
            String returnStr = null;
            switch (game)
            {
                case 0:
                    fs.Seek(325, SeekOrigin.Begin);
                    var z_map = new byte[200];
                    int fl = 0;
                    long pos = 0;
                    while (fl != 2)
                    {
                        if (fs.ReadByte() == 00)
                        {
                            fl++;
                            pos = fs.Position - 2;
                        }
                    }
                    fs.Seek(307, SeekOrigin.Begin);
                    try
                    {
                        fs.Read(z_map, 0, (int) pos - 307);
                    }
                    catch (Exception)
                    {
                    }
                    string MF = ((Utilities.Convertions.ByteArrToAscii(z_map).Split(new[] {'\\'}))[3]);
                    String[] MapData = File.ReadAllLines(MainWindow._AssemblyDir + @"\data\maps\coh1\maps.data");
                    foreach (string line in MapData)
                    {
                        if (!line.StartsWith("#") || !line.Contains("#"))
                        {
                            string mapf = (Regex.Split(line, "==")[1].Split(new[] {'\\'}))[3];
                            if (mapf.Equals(MF))
                            {
                                returnStr = mapf;
                                break;
                            }
                        }
                    }
                    break;
                case 1:
                    fs.Seek(282, SeekOrigin.Begin);
                    var z_map2 = new byte[200];
                    int fl2 = 0;
                    long pos2 = 0;
                    while (fl2 != 2)
                    {
                        if (fs.ReadByte() == 00)
                        {
                            fl2++;
                            pos2 = fs.Position - 5;
                        }
                    }
                    fs.Seek(282, SeekOrigin.Begin);
                    try
                    {
                        fs.Read(z_map2, 0, (int) pos2 - 282);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    string MF2 = ((Utilities.Convertions.ByteArrToAscii(z_map2).Split(new[] {'\\'}))[3]);
                    String[] MapData2 = File.ReadAllLines(MainWindow._AssemblyDir + @"\data\maps\coh2\maps.data");
                    foreach (string line in MapData2)
                    {
                        if (!line.Contains("#"))
                        {
                            string mapf = (Regex.Split(line, "==")[1].Split(new[] {'\\'}))[3];
                            if (mapf.Equals(MF2.Remove(MF2.Length - 1, 1)))
                            {
                                returnStr = mapf;
                                break;
                            }
                        }
                    }
                    break;
            }

            fs.Close();
            return returnStr;
        }

        private void ParseCOH2_Replays()
        {
            replays.Clear();
            errorNames.Clear();
            int z = 0;
            var di = new DirectoryInfo(doc_path + @"\playback");

            foreach (FileInfo file in di.GetFiles("*.rec"))
            {
                try
                {
                    Console.WriteLine("Found replay: " + file.Name);

                    FileStream fs = File.OpenRead(file.FullName);

                    byte[] z_version = {0, 0, 0, 0};
                    fs.Read(z_version, 0, 4);
                    byte[] versionMod = {0, 0, z_version[3], z_version[2]};
                    String vs = BitConverter.ToString(versionMod).Replace("-", "");
                    vs = vs.Remove(0, 4);
                    int ver = Int32.Parse(vs, NumberStyles.HexNumber);
                    var buffer = new byte[31];
                    fs.Seek(12, SeekOrigin.Begin);
                    fs.Read(buffer, 0, 31);
                    DateTime date = DateTime.Now;
                    try
                    {
                        date = DateTime.Parse(Utilities.Convertions.ByteArrToAscii(buffer), CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            var buffer2 = new byte[18];
                            fs.Seek(12, SeekOrigin.Begin);
                            fs.Read(buffer2, 0, 17);
                            string[] s =
                                (Utilities.Convertions.ByteArrToAscii(buffer2).Replace("/", "-")).Split(new[] {'-'});
                            string finalString = "";
                            if (s[0].Trim().Length != 2)
                            {
                                finalString = "0" + s[0] + "-";
                            }
                            else
                            {
                                finalString = s[0] + "-";
                            }
                            if (s[1].Trim().Length != 2)
                            {
                                finalString = finalString + "0" + s[1] + "-";
                            }
                            else
                            {
                                finalString = finalString + s[1] + "-";
                            }
                            finalString = finalString + s[2] + " 16:00";
                            date = DateTime.Parse(finalString, CultureInfo.InvariantCulture);

                            Console.WriteLine();
                            Console.WriteLine(ex.ToString());
                        }
                        catch (Exception)
                        {
                            date = DateTime.Now;
                        }
                    }
                    fs.Seek(282, SeekOrigin.Begin);
                    var z_map = new byte[200];
                    int fl = 0;
                    long pos = 0;
                    while (fl != 2)
                    {
                        if (fs.ReadByte() == 00)
                        {
                            fl++;
                            pos = fs.Position - 5;
                        }
                    }
                    fs.Seek(282, SeekOrigin.Begin);
                    try
                    {
                        if ((int) pos - 282 > 0)
                        {
                            fs.Read(z_map, 0, (int) pos - 282);
                        }
                        else
                        {
                            errorNames.AppendLine("• " + file.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorNames.AppendLine("• " + file.Name);
                        Console.WriteLine();
                        Console.WriteLine(ex.ToString());
                        break;
                    }
                    string MF = ((Utilities.Convertions.ByteArrToAscii(z_map).Split(new[] {'\\'}))[3]);
                    String[] MapData = File.ReadAllLines(MainWindow._AssemblyDir + @"\data\maps\coh2\maps.data");
                    String MapFile = "", MapName = "";
                    foreach (string line in MapData)
                    {
                        if (!line.StartsWith("#") || !line.Contains("#"))
                        {
                            string mapf = (Regex.Split(line, "==")[1].Split(new[] {'\\'}))[3];
                            if (mapf.Equals(MF.Remove(MF.Length - 1, 1)))
                            {
                                MapName = Regex.Split(line, "==")[0];
                                MapFile = mapf;
                                break;
                            }
                        }
                    }
                    fs.Close();
                    Console.WriteLine("Replay Details: " + MapName + " - Version: 3.0.0." + ver);
                    try
                    {
                        replays.Add(new Replay
                        {
                            name = file.Name.Replace(".rec", ""),
                            version = "3.0.0." + ver,
                            map_file = MainWindow._AssemblyDir + @"data\maps\coh2\" + MapFile + ".jpg",
                            map_name = MapName,
                            game_date = date.ToString("dd-MM-yyyy ~ HH:mm"),
                            id = z,
                            filename = file.Name
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine(ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                }
            }
            z++;
        }

        private void ParseCOH_Replays()
        {
            replays.Clear();
            errorNames.Clear();
            int z = 0;
            var di = new DirectoryInfo(doc_path + @"\playback");

            foreach (FileInfo file in di.GetFiles("*.rec"))
            {
                FileStream fs = File.OpenRead(file.FullName);

                byte[] z_version = {0, 0, 0, 0};
                fs.Read(z_version, 0, 4);
                byte[] versionMod = {0, 0, z_version[3], z_version[2]};
                String vs = BitConverter.ToString(versionMod).Replace("-", "");
                vs = vs.Remove(0, 4);
                int ver = Int32.Parse(vs, NumberStyles.HexNumber);
                var buffer = new byte[31];
                fs.Seek(12, SeekOrigin.Begin);
                fs.Read(buffer, 0, 31);
                DateTime date;
                try
                {
                    date = DateTime.Parse(Utilities.Convertions.ByteArrToAscii(buffer), CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    var buffer2 = new byte[18];
                    fs.Seek(12, SeekOrigin.Begin);
                    fs.Read(buffer2, 0, 17);
                    string[] s =
                        (Utilities.Convertions.ByteArrToAscii(buffer2).Replace("/", "-")).Split(new[] {'-'});
                    string finalString = "";
                    if (s[0].Trim().Length != 2)
                    {
                        finalString = "0" + s[0] + "-";
                    }
                    else
                    {
                        finalString = s[0] + "-";
                    }
                    if (s[1].Trim().Length != 2)
                    {
                        finalString = finalString + "0" + s[1] + "-";
                    }
                    else
                    {
                        finalString = finalString + s[1] + "-";
                    }
                    finalString = finalString + s[2] + " 16:00";
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                    date = DateTime.Now;
                }
                fs.Seek(325, SeekOrigin.Begin);
                var z_map = new byte[200];
                int fl = 0;
                long pos = 0;
                while (fl != 2)
                {
                    if (fs.ReadByte() == 00)
                    {
                        fl++;
                        pos = fs.Position - 2;
                    }
                }
                fs.Seek(307, SeekOrigin.Begin);
                try
                {
                    if ((int) pos - 307 > 0)
                    {
                        fs.Read(z_map, 0, (int) pos - 307);
                    }
                    else
                    {
                        errorNames.AppendLine("• " + file.Name);
                    }
                }
                catch (Exception ex)
                {
                    errorNames.AppendLine("• " + file.Name);
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                    break;
                }
                string MF = ((Utilities.Convertions.ByteArrToAscii(z_map).Split(new[] {'\\'}))[3]);
                String[] MapData = File.ReadAllLines(MainWindow._AssemblyDir + @"\data\maps\coh1\maps.data");
                String MapFile = "", MapName = "";
                foreach (string line in MapData)
                {
                    if (!line.StartsWith("#") || !line.Contains("#"))
                    {
                        string mapf = (Regex.Split(line, "==")[1].Split(new[] {'\\'}))[3];
                        if (mapf.Equals(MF))
                        {
                            MapName = Regex.Split(line, "==")[0];
                            MapFile = mapf;
                            break;
                        }
                    }
                }
                fs.Close();
                try
                {
                    replays.Add(new Replay
                    {
                        name = file.Name.Replace(".rec", ""),
                        version = "2.700.2.42",
                        map_file = MainWindow._AssemblyDir + @"data\maps\coh1\" + MapFile + ".jpg",
                        map_name = MapName,
                        game_date = date.ToString("dd-MM-yyyy ~ HH:mm"),
                        id = z,
                        filename = file.Name
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                }
            }
            z++;
        }

        private void btn_upload_Click(object sender, RoutedEventArgs e)
        {
            if (game == 1)
            {
                if (replayList.SelectedIndex != -1)
                {
                    if (replayList.Items.Count > 0)
                    {
                        var rp = replayList.Items[replayList.SelectedIndex] as Replay;
                        var ru = new Replay_Uploader(doc_path, rp.filename, game);
                        ru.ShowDialog();
                    }
                }
                else
                {
                    Utilities.showError(this, "You need at least 1 replay selected to continue.");
                }
            }
            else
            {
                Utilities.showError(this, "This feature is only available with Company of Heroes 2.");
            }
        }

        private void replayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (replayList.SelectedIndex != -1)
            {
                if (replayList.Items.Count > 0)
                {
                    var rep = replayList.SelectedItem as Replay;
                    pic_map.Image = new Bitmap(rep.map_file);
                    txt_name.Content = "Name: " + rep.name.Trim();
                    txt_mapname.Content = "Map: " + rep.map_name.Trim();
                    txt_version.Content = "Version: " + rep.version.Trim();
                    txt_date.Content = "Date: " + (Regex.Split(rep.game_date, "~")[0]).Trim();
                    txt_time.Content = "Time: " + (Regex.Split(rep.game_date, "~")[1]).Trim();
                }
            }
            else
            {
                CleanInfo();
            }
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            if (replayList.SelectedIndex != -1)
            {
                var StrBuild = new StringBuilder();
                for (int i = 0; i < replayList.SelectedItems.Count; i++)
                {
                    var rp = replayList.SelectedItems[i] as Replay;
                    StrBuild.AppendLine("• " + rp.name);
                }

                if (MessageBox.Show(this, "You are about to delete the following replay(s):\n"
                                          + StrBuild + "\nAre you sure you want to continue?", "Confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    while (replayList.SelectedIndex != -1)
                    {
                        var rp = replayList.Items[replayList.SelectedIndex] as Replay;
                        replays.Remove(rp);
                        File.Delete(doc_path + @"\playback\" + rp.filename);
                    }
                    if (replayList.Items.Count != 0)
                    {
                        replayList.SelectedIndex = 0;
                    }
                }
            }
            else
            {
                Utilities.showError(this, "You need at least 1 replay selected to continue.");
            }
        }

        private void pic_error_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(this, "CELO was unable to parse the following replay(s):\n" + errorNames
                                  + "\nThis can be caused by the replay(s) being too old or corrupted.", "Error Parsing",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void btn_rename_Click(object sender, RoutedEventArgs e)
        {
            if (replayList.SelectedIndex != -1)
            {
                try
                {
                    pauseWatch = true;
                    IList z = replayList.SelectedItems;
                    foreach (object rep in z)
                    {
                        var rp = rep as Replay;
                        var input = new InputBox("Replay: " + rp.name, "Enter new name for " + rp.name + ": ",
                            rp.name);
                        input.ShowDialog();
                        if (input.DialogResult.HasValue && input.DialogResult.Value && input.val != string.Empty)
                        {
                            File.Move(doc_path + @"\playback\" + rp.name + ".rec",
                                doc_path + @"\playback\" + input.val.Replace(".rec", "") + ".rec");
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    pauseWatch = false;
                    LoadList();
                }
            }
            else
            {
                Utilities.showError(this, "You need at least 1 replay selected to continue.");
            }
        }

        private void btn_changeVersion_Click(object sender, RoutedEventArgs e)
        {
            if (game == 1)
            {
                if (replayList.SelectedIndex != -1)
                {
                    var StrBuild = new StringBuilder();
                    for (int i = 0; i < replayList.SelectedItems.Count; i++)
                    {
                        var rp = replayList.SelectedItems[i] as Replay;
                        StrBuild.AppendLine("• " + rp.name);
                    }

                    if (MessageBox.Show(this, "You are about to change the version(s) of the following replay(s):\n"
                                              + StrBuild + "\nDo you want to continue?", "Replay Version",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            pauseWatch = true;
                            IList z = replayList.SelectedItems;
                            foreach (object rep in z)
                            {
                                var rp = rep as Replay;
                                var vs = new VersionChanger(rp.filename, rp.name, rp.version, exe_path, doc_path, game);
                                vs.ShowDialog();
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            pauseWatch = false;
                            LoadList();
                        }
                    }
                }
                else
                {
                    Utilities.showError(this, "You need at least 1 replay selected to continue.");
                }
            }
            else
            {
                Utilities.showError(this, "This feature is only available with Company of Heroes 2.");
            }
        }

        private void replayList_Loaded(object sender, RoutedEventArgs e)
        {
            if (replayList.Items.Count > 0)
                replayList.SelectedIndex = 0;
        }

        private bool FilterReplays(object obj)
        {
            var item = obj as Replay;
            if (item == null) return false;
            if (searchMethod == 0)
            {
                DateTime GameDate = DateTime.Parse((Regex.Split(item.game_date, "~")[0]).Trim());
                DateTime StartDate = date_start.SelectedDate.Value;
                DateTime EndDate = date_end.SelectedDate.Value;
                // apply the filter  
                if (GameDate <= EndDate && GameDate >= StartDate)
                {
                    return true;
                }
            }
            else
            {
                string te = tBox_search.Text.ToLower().Trim();
                string itemn = item.name.ToLower().Trim();
                if (itemn.Contains(te))
                {
                    return true;
                }
            }

            return false;
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(replayList.ItemsSource);
            view.Filter = null;
            view.Filter = FilterReplays;
        }

        private void btnCancelFilter_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(replayList.ItemsSource);
            view.Filter = null;
        }

        private void repMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CleanInfo();

            LoadList();
            InitializeFileSystemWatcher();
        }

        private class Replay
        {
            public int id { get; set; }
            public string name { get; set; }
            public string filename { get; set; }
            public string version { get; set; }
            public string map_file { get; set; }
            public string map_name { get; set; }
            public string game_date { get; set; }
            public string tag { get; set; }
        }

        #region Expanders

        private void filterDates_Expanded(object sender, RoutedEventArgs e)
        {
            filterName.IsExpanded = false;
            BringToFront(filterDates);
            btnFilter.Visibility = Visibility.Visible;
            btnCancelFilter.Visibility = Visibility.Visible;
            searchMethod = 0;
        }

        private void filterName_Expanded(object sender, RoutedEventArgs e)
        {
            filterDates.IsExpanded = false;
            BringToFront(filterName);
            btnFilter.Visibility = Visibility.Visible;
            btnCancelFilter.Visibility = Visibility.Visible;
            searchMethod = 1;
        }

        private void BringToFront(FrameworkElement element)
        {
            if (element == null) return;

            var parent = element.Parent as Panel;
            if (parent == null) return;

            int maxZ = parent.Children.OfType<UIElement>()
                .Where(x => x != element)
                .Select(x => Panel.GetZIndex(x))
                .Max();
            Panel.SetZIndex(element, maxZ + 1);
        }

        private void filterName_Collapsed(object sender, RoutedEventArgs e)
        {
            if (!filterDates.IsExpanded && !filterName.IsExpanded)
            {
                btnFilter.Visibility = Visibility.Hidden;
                btnCancelFilter.Visibility = Visibility.Hidden;
            }
        }

        private void filterDates_Collapsed(object sender, RoutedEventArgs e)
        {
            if (!filterDates.IsExpanded && !filterName.IsExpanded)
            {
                btnFilter.Visibility = Visibility.Hidden;
                btnCancelFilter.Visibility = Visibility.Hidden;
            }
        }

        #endregion
    }
}