using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;

namespace CELO_Enhanced
{
    public partial class ReplayManager : Window
    {
        private readonly string doc_path;
        private readonly StringBuilder errorNames = new StringBuilder();
        private readonly string exe_path;
        private readonly int game;
        private readonly Utilities.Log logFile = new Utilities.Log(AppDomain.CurrentDomain.BaseDirectory + @"\logs");
        private readonly ObservableCollection<Replay> replays = new ObservableCollection<Replay>();
        private FileSystemWatcher fsw;
        private bool isChatEnabled = true;
        private Utilities.INIFile mainINI;
        private Boolean pauseWatch;
        private LoadingScreen sc;
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

        private long DetectNextPlayerPos(ref FileStream f, long pos, int failsafeNum = 450)
        {
            var fs = f;
            fs.Position = pos;
            var failsafe = 0;
            while (failsafe < failsafeNum)
            {
                var bt = fs.ReadByte();
                var streamPos = fs.Position;
                if (bt == 0)
                {
                    var bt2 = fs.ReadByte();
                    if (bt2 != 0 && bt2 > 31 && bt2 < 225) // POSSIBLE PLAYER
                    {
                        var bt3 = fs.ReadByte();
                        if (bt3 == 0)
                        {
                            var bt4 = fs.ReadByte();
                            if (bt4 != 0 && bt4 > 31 && bt4 < 225)
                            {
                                var bt5 = fs.ReadByte();
                                if (bt5 == 0)
                                {
                                    var bt6 = fs.ReadByte();
                                    if (bt6 != 0 && bt6 > 31 && bt6 < 225)
                                    {
                                        var bt7 = fs.ReadByte();
                                        if (bt7 == 0)
                                        {
                                            var bt8 = fs.ReadByte();
                                            if (bt8 != 0 && bt8 > 31 && bt8 < 225) // CERTAIN PLAYER
                                            {
                                                fs.Position = streamPos;
                                                return streamPos;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                failsafe++;
            }

            return -1;
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
            Dispatcher.Invoke(new Action(sc.Close));
            Dispatcher.Invoke(new Action(SetUpTag));
        }

        private void SetUpTag()
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = DateTime.Today.AddDays(-1);

                var index = 0;
                foreach (var replay in replays)
                {
                    var file = new FileInfo(doc_path + @"\playback\" + replay.name + ".rec");
                    var matchDay = new DateTime();
                    var daz = replay.game_date;
                    try
                    {
                        matchDay = DateTime.ParseExact(daz, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
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
                var view = CollectionViewSource.GetDefaultView(replayList.ItemsSource);
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
            }
        }

        private void LoadList()
        {
            replays.Clear();
            errorNames.Clear();
            sc = new LoadingScreen(this, "Parsing Replays Information");
            var thread = new Thread(DetectReplays);
            thread.Start();
            sc.ShowDialog();
        }

        public static string RetrieveMap(string replay, int game)
        {
            var fs = File.OpenRead(replay);
            var returnStr = "unknown";
            switch (game)
            {
                case 0:
                    fs.Seek(325, SeekOrigin.Begin);
                    var z_map = new byte[200];
                    var fl = 0;
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
                    var MF = ((Utilities.Convertions.ByteArrToAscii(z_map).Split('\\'))[3]);
                    var MapData = File.ReadAllLines(MainWindow._AssemblyDir + @"\data\maps\coh1\maps.data");
                    foreach (var line in MapData)
                    {
                        if (!line.StartsWith("#") || !line.Contains("#"))
                        {
                            var mapf = (Regex.Split(line, "==")[1].Split('\\'))[3];
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
                    var fl2 = 0;
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
                    var MF2 = ((Utilities.Convertions.ByteArrToAscii(z_map2).Split('\\'))[3]);
                    var MapData2 = File.ReadAllLines(MainWindow._AssemblyDir + @"\data\maps\coh2\maps.data");
                    foreach (var line in MapData2)
                    {
                        if (!line.Contains("#"))
                        {
                            var mapf = (Regex.Split(line, "==")[1].Split('\\'))[3];
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

        public long totalTime = 0;

        private void ParseCOH2_Replays()
        {
            var z = 0;
            var di = new DirectoryInfo(doc_path + @"\playback");
            Byte[] btNeedle = {68, 65, 84, 65, 80, 76, 65, 83};
           
            foreach (var file in di.GetFiles("*.rec"))
            {
                
                var bytArray = File.ReadAllBytes(file.FullName);
                try
                {
                    var fs = File.OpenRead(file.FullName);

                    byte[] z_version = {0, 0, 0, 0};
                    fs.Read(z_version, 0, 4);
                    byte[] versionMod = {0, 0, z_version[3], z_version[2]};
                    var vs = BitConverter.ToString(versionMod).Replace("-", "");
                    vs = vs.Remove(0, 4);
                    var ver = Int32.Parse(vs, NumberStyles.HexNumber);
                    var buffer = new byte[31];
                    fs.Seek(12, SeekOrigin.Begin);
                    fs.Read(buffer, 0, 31);
                    var dateStr = Utilities.Convertions.ByteArrToAscii(buffer).Trim().Replace("¼", "P");
                    var lastT = new DateTime();
                    var tmp2 = Regex.Split(dateStr, " ");
                    var tmp_time = tmp2[1];
                    var tmp_day = tmp2[0];
                    var time = "";
                    var date = "";
                    var isUS = false;
                    try
                    {
                        if (tmp2.Length == 3) // AM PM
                        {
                            isUS = true;
                            if (tmp2[2].Contains("P"))
                            {
                                var h = Int32.Parse(Regex.Split(tmp_time, ":")[0]) + 12;
                                time = h + ":" + Regex.Split(tmp_time, ":")[1];
                            }
                            else
                            {
                                time = tmp_time;
                            }
                        }
                        else
                        {
                            time = tmp_time;
                        }

                        tmp_day = tmp_day.Replace(".", "/").Replace("-", "/");
                    }
                    catch (FormatException)
                    {
                    }
                    try
                    {
                        var dt = Regex.Split(tmp_day, "/");
                        if (dt[0].Length > 2)
                        {
                            var t = dt[0];
                            dt[0] = dt[2];
                            dt[2] = t;
                        }

                        if (dt[0].Length != 2)
                        {
                            dt[0] = "0" + dt[0];
                        }

                        if (dt[1].Length != 2)
                        {
                            dt[1] = "0" + dt[1];
                        }

                        if (isUS && Int32.Parse(dt[0]) <= 12)
                        {
                            var t = dt[0];
                            dt[0] = dt[1];
                            dt[1] = t;
                        }

                        date = String.Format("{0}/{1}/{2} {3}", dt[0], dt[1], dt[2], time);
                        lastT = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }


                    fs.Seek(282, SeekOrigin.Begin);
                    var z_map = new byte[200];
                    var fl = 0;
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

                        break;
                    }
                    var MF = ((Utilities.Convertions.ByteArrToAscii(z_map).Split('\\'))[3]);
                    var MapData = File.ReadAllLines(MainWindow._AssemblyDir + @"\data\maps\coh2\maps.data");
                    String MapFile = "", MapName = "";
                    foreach (var line in MapData)
                    {
                        if (!line.StartsWith("#") || !line.Contains("#"))
                        {
                            var mapf = (Regex.Split(line, "==")[1].Split('\\'))[3];
                            if (mapf.Equals(MF.Remove(MF.Length - 1, 1)))
                            {
                                MapName = Regex.Split(line, "==")[0];
                                MapFile = mapf;
                                break;
                            }
                        }
                    }

                    // PLAYERS
                    long PlayersStartPos = 0;
                    var endedPlayers = false;
                    var timesFound = 0;
                    var replayPlayers = new List<Player>();
                    try
                    {
                        fs.Seek(1100, SeekOrigin.Begin);
                        var failsafe = 0;
                        while (failsafe < 3000)
                        {
                            var bt = fs.ReadByte();
                            if (bt == 255)
                            {
                                timesFound++;
                            }

                            if (timesFound == 4)
                            {
                                PlayersStartPos = fs.Position;

                                break;
                            }

                            failsafe++;
                        }
                    }
                    catch (Exception ex)
                    {
                        logFile.WriteLine("EXCEPTION (1) AT REPLAY MANAGER : " + ex);
                    }

                    var detectPos = PlayersStartPos;
                    while (endedPlayers != true)
                    {
                        try
                        {
                            var nPos = DetectNextPlayerPos(ref fs, detectPos);

                            if (nPos != -1)
                            {
                                var failsafe3 = 0;
                                var nicknameRep = "";
                                var interval = 0;
                                long stPos = 0;
                                while (failsafe3 < 250 && interval < 3)
                                {
                                    var byt = fs.ReadByte();
                                    if (byt == 0)
                                    {
                                        interval++;
                                        stPos = fs.Position;
                                    }
                                    else
                                    {
                                        interval = 0;
                                        nicknameRep += Convert.ToString(Convert.ToChar(byt));
                                    }


                                    failsafe3++;
                                }

                                stPos = stPos - interval;

                                fs.Position = stPos + 9;
                                var raceName = "";
                                var byt2 = 0;
                                do
                                {
                                    byt2 = fs.ReadByte();
                                    if (byt2 != 05)
                                    {
                                        raceName += Convert.ToString(Convert.ToChar(byt2));
                                    }
                                } while (byt2 != 05);
                                var raceNumber = 0;
                                var racename = "Unknown Race";

                                if (raceName.Equals("st_german"))
                                {
                                    raceNumber = 2;
                                    racename = "OKW";
                                }
                                else if (raceName.Equals("rman"))
                                {
                                    raceNumber = 0;
                                    racename = "Wehrmacht";
                                }
                                else if (raceName.Equals("british"))
                                {
                                    raceNumber = 4;
                                    racename = "UK Forces";
                                }
                                else if (raceName.Equals("aef"))
                                {
                                    raceNumber = 3;
                                    racename = "US Forces";
                                }
                                else if (raceName.Equals("soviet"))
                                {
                                    raceNumber = 1;
                                    racename = "Soviet Union";
                                }

                                if (racename != "Unknown Race")
                                {
                                    replayPlayers.Add(new Player
                                    {
                                        race = raceNumber,
                                        nickname = nicknameRep,
                                        race_name = racename,
                                        icon = "Resources/coh2_" + raceNumber + ".png"
                                    });
                                }

                                detectPos = nPos + 65;
                            }
                            else
                            {
                                endedPlayers = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            logFile.WriteLine("EXCEPTION (2) AT REPLAY MANAGER : " + ex);
                        }
                    }


                    // END PLAYERS

                    // START CHAT
                    var ChatEntries = new List<ChatEntry>();
                    
                    if (isChatEnabled)
                    {
                        try
                        {
                            var startPos = Utilities.SearchBytes(bytArray, btNeedle) + 75;
                            var curPos = startPos;
                            long PlayerPos = 0;
                            long fs1 = 0;

                            for (;;)
                            {
                                PlayerPos = DetectNextPlayerPos(ref fs, curPos, bytArray.Length);
                                if (PlayerPos != -1 && fs1 != PlayerPos)
                                {
                                    fs.Position = PlayerPos - 16;
                                    fs1 = PlayerPos;

                                    if (fs.ReadByte() == 1)
                                    {
                                        fs.Position = PlayerPos;
                                        var tmp = 0;
                                        var newPos = PlayerPos - 8;
                                        fs.Position = newPos;
                                        tmp = fs.ReadByte();
                                        if (tmp == 2 || tmp == 4 || tmp == 6)
                                        {
                                            fs.Position = PlayerPos - 4;
                                            var size = (fs.ReadByte())*2;
                                            fs.Position = PlayerPos;
                                            var vBytes = new byte[size];
                                            fs.Read(vBytes, 0, size);
                                            var bt = vBytes.Where(x => x != 0).ToArray();
                                            var nick = Utilities.Convertions.ByteToASCII(bt);
                                            size = (fs.ReadByte())*2;
                                            var vBytes2 = new byte[size];
                                            fs.Position += 3;
                                            fs.Read(vBytes2, 0, size);
                                            var bt2 = vBytes2.Where(x => x != 0).ToArray();
                                            var textChat = Utilities.Convertions.ByteToASCII(bt2);

                                            ChatEntries.Add(new ChatEntry
                                            {
                                                nickname = nick,
                                                text = textChat
                                            });

                                            curPos = fs.Position + 50;
                                        }
                                    }
                                    else
                                    {
                                        curPos += 75;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logFile.WriteLine("EXCEPTION (3) AT REPLAY MANAGER : " + ex);
                        }
                    }
                    // END CHAT


                    fs.Close();

                    try
                    {
                        replays.Add(new Replay
                        {
                            name = file.Name.Replace(".rec", ""),
                            version = "4.0.0." + ver,
                            map_file = MainWindow._AssemblyDir + @"data\maps\coh2\" + MapFile + ".jpg",
                            map_name = MapName,
                            game_date = lastT.ToString("dd/MM/yyyy HH:mm"),
                            id = z,
                            players = replayPlayers,
                            chat = ChatEntries,
                            filename = file.Name
                        });
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                }
                z++;
                

            }
            
        }

        private void ParseCOH_Replays()
        {
            replays.Clear();
            errorNames.Clear();
            var z = 0;
            var di = new DirectoryInfo(doc_path + @"\playback");

            foreach (var file in di.GetFiles("*.rec"))
            {
                var fs = File.OpenRead(file.FullName);

                byte[] z_version = {0, 0, 0, 0};
                fs.Read(z_version, 0, 4);
                byte[] versionMod = {0, 0, z_version[3], z_version[2]};
                var vs = BitConverter.ToString(versionMod).Replace("-", "");
                vs = vs.Remove(0, 4);
                var ver = Int32.Parse(vs, NumberStyles.HexNumber);
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
                    var s =
                        (Utilities.Convertions.ByteArrToAscii(buffer2).Replace("/", "-")).Split('-');
                    var finalString = "";
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

                    date = DateTime.Now;
                }
                fs.Seek(325, SeekOrigin.Begin);
                var z_map = new byte[200];
                var fl = 0;
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

                    break;
                }
                var MF = ((Utilities.Convertions.ByteArrToAscii(z_map).Split('\\'))[3]);
                var MapData = File.ReadAllLines(MainWindow._AssemblyDir + @"\data\maps\coh1\maps.data");
                String MapFile = "", MapName = "";
                foreach (var line in MapData)
                {
                    if (!line.StartsWith("#") || !line.Contains("#"))
                    {
                        var mapf = (Regex.Split(line, "==")[1].Split('\\'))[3];
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
                    try
                    {
                        pic_map.Image = new Bitmap(rep.map_file);
                    }
                    catch (Exception)
                    {
                        pic_map.Image = new Bitmap(MainWindow._AssemblyDir + @"data\maps\unknown.png");
                    }
                    txt_name.Content = "Name: " + rep.name.Trim();
                    txt_mapname.Content = "Map: " + rep.map_name.Trim();
                    txt_version.Content = "Version: " + rep.version.Trim();
                    txt_date.Content = "Date: " + (Regex.Split(rep.game_date, " ")[0]).Trim() + " (dd/mm/yyyy)";
                    txt_time.Content = "Time: " + (Regex.Split(rep.game_date, " ")[1]).Trim();


                    if (game == 1)
                    {
                        var axis = new List<Player>();
                        var allies = new List<Player>();

                        foreach (var player in rep.players)
                        {
                            if (player.race == 0 || player.race == 2)
                            {
                                axis.Add(player);
                            }
                            else
                            {
                                allies.Add(player);
                            }
                        }

                        if (axis.Count > 4)
                        {
                            axis.RemoveRange(4, axis.Count - 4);
                        }

                        if (allies.Count > 4)
                        {
                            allies.RemoveRange(4, allies.Count - 4);
                        }

                        AxisList.ItemsSource = axis;
                        AlliesList.ItemsSource = allies;
                        var para = new Paragraph();
                        txtReplayChat.Document = new FlowDocument(para);

                        for (var i = 0; i < rep.chat.Count; i++)
                        {
                            var name = rep.chat[i].nickname.Trim();
                            var rn = new Run();
                            rn.Text = name;
                            rn.Foreground = Brushes.Black;
                            foreach (var player in rep.players)
                            {
                                if (player.nickname.Contains(name))
                                {
                                    if (player.race == 0 || player.race == 2)
                                    {
                                        rn.Foreground = Brushes.Red;
                                    }
                                    else
                                    {
                                        rn.Foreground = Brushes.DodgerBlue;
                                    }
                                }
                            }


                            var message = rep.chat[i].text.Trim();
                            para.Inlines.Add(new Bold(rn));
                            para.Inlines.Add(" : " + message);
                            para.Inlines.Add(new LineBreak());
                        }
                    }
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
                for (var i = 0; i < replayList.SelectedItems.Count; i++)
                {
                    var rp = replayList.SelectedItems[i] as Replay;
                    StrBuild.AppendLine("• " + rp.name);
                }

                if (MessageBox.Show(this, "You are about to delete the following replay(s):\n"
                                          + StrBuild + "\nAre you sure you want to continue?", "Confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    pauseWatch = true;
                    while (replayList.SelectedIndex != -1)
                    {
                        var rp = replayList.Items[replayList.SelectedIndex] as Replay;
                        replays.Remove(rp);
                        File.Delete(doc_path + @"\playback\" + rp.filename);
                    }
                    pauseWatch = false;
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
                    var z = replayList.SelectedItems;
                    foreach (var rep in z)
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
                    for (var i = 0; i < replayList.SelectedItems.Count; i++)
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
                            var z = replayList.SelectedItems;
                            foreach (var rep in z)
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
                var GameDate = DateTime.Parse((Regex.Split(item.game_date, "~")[0]).Trim());
                var StartDate = date_start.SelectedDate.Value;
                var EndDate = date_end.SelectedDate.Value;
                // apply the filter  
                if (GameDate <= EndDate && GameDate >= StartDate)
                {
                    return true;
                }
            }
            else
            {
                var te = tBox_search.Text.ToLower().Trim();
                var itemn = item.name.ToLower().Trim();
                if (itemn.Contains(te))
                {
                    return true;
                }
            }

            return false;
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(replayList.ItemsSource);
            view.Filter = null;
            view.Filter = FilterReplays;
        }

        private void btnCancelFilter_Click(object sender, RoutedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(replayList.ItemsSource);
            view.Filter = null;
        }

        private void repMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (game == 1)
            {
                mainINI = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
                tabPlayers.IsEnabled = true;
                if (mainINI.IniReadValue("ReplayManager", "ShowChat").ToLower() == "true")
                {
                    isChatEnabled = true;
                    tabChat.IsEnabled = true;
                }
                else
                {
                    isChatEnabled = false;
                    tabChat.IsEnabled = false;
                }
            }

            CleanInfo();

            LoadList();
            InitializeFileSystemWatcher();
        }

        private void repMainWindow_Closing(object sender, CancelEventArgs e)
        {
            fsw.Created -= Fsw_Created;
            fsw.Deleted -= Fsw_Deleted;
            fsw.Renamed -= Fsw_Renamed;
            
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
            public List<ChatEntry> chat { get; set; }
            public List<Player> players { get; set; }
        }

        public class ChatEntry
        {
            public string nickname { get; set; }
            public string text { get; set; }
        }

        public class Player
        {
            public string icon { get; set; }
            public int race { get; set; }
            public string race_name { get; set; }
            public string nickname { get; set; }
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

            var maxZ = parent.Children.OfType<UIElement>()
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