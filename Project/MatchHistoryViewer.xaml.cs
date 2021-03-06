﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;


namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for MatchHistoryViewer.xaml
    /// </summary>
    public partial class MatchHistoryViewer : Window
    {
        private readonly string _docPath;
        private readonly int _gameSelected = 1;
        private readonly ObservableCollection<Match> _matches = new ObservableCollection<Match>();
        private readonly Utilities.Log logFile = new Utilities.Log(AppDomain.CurrentDomain.BaseDirectory + @"\logs");
        private long SteamID_z;

        public MatchHistoryViewer(int game, string docPath)
        {
            _docPath = docPath;
            _gameSelected = game;
            InitializeComponent();
        }

        private string GetMapName(string filename)
        {
            if (filename != "")
            {
                var namesFile = String.Format(MainWindow._AssemblyDir + @"\data\maps\coh{0}\maps.data",
                    (_gameSelected + 1));
                foreach (var line in File.ReadAllLines(namesFile))
                {
                    if (!line.Contains("#"))
                    {
                        if (line.Contains(filename))
                        {
                            var n1 = Regex.Split(line, "==")[0];
                            return n1;
                        }
                    }
                }
            }
            else
            {
                return "Unknown";
            }

            return "";
        }

        private void LoadMatches()
        {
            var dbFile = MainWindow._AssemblyDir + @"\data\history\coh" + (_gameSelected + 1) + @"\mhv.xml";
            var mapFolder = MainWindow._AssemblyDir + @"\data\maps\coh" + (_gameSelected + 1) + @"\";


            var doc = new XmlDocument();
            doc.Load(dbFile);
            var XnList = doc.SelectNodes("/Matches/Match");
            var z = 0;
            if (XnList != null)
            {
                foreach (XmlNode xnode in XnList)
                {
                    try
                    {
                        var ID = z;
                        var Replay = xnode["Replay"].InnerText;
                        var GameDate = xnode["Date"].InnerText;
                        var MapFileName = mapFolder + xnode["Map"].InnerText + ".jpg";
                        var MapName = GetMapName(xnode["Map"].InnerText);
                        var Type = Convert.ToInt32(xnode["Type"].InnerText);

                        var Pls = new List<Player>();
                        var plNodeList = xnode.SelectNodes("Players/Player");
                        var Ic = Enumerable.Range(0, plNodeList.Count).ToList().Randomize().ToList();
                        for (var i = 0; i < plNodeList.Count; i++)
                        {
                            var xnode2 = plNodeList.Item(i);
                            var Nickname = xnode2["Nickname"].InnerText;
                            var SteamID = Int64.Parse(xnode2["SteamID"].InnerText);
                            var Rank = xnode2["Ranking"].InnerText;
                            var Level = xnode2["Level"].InnerText;
                            var HoursPlayed = Int32.Parse(xnode2["Timeplayed"].InnerText);
                            var ra = 0;
                            ra = Int32.Parse(xnode2["Race"].InnerText);
                            var ic = "";
                            if (_gameSelected == 1)
                            {
                                ic = "Resources/coh2_" + ra + ".png";
                            }
                            else
                            {
                                ic = "Resources/coh1_" + ra + ".png";
                            }
                            var rcName = "";
                            switch (ra)
                            {
                                case 0:
                                    if (_gameSelected == 1)
                                    {
                                        rcName = "Wehrmacht";
                                    }
                                    else
                                    {
                                        rcName = "Commonwealth";
                                    }
                                    break;
                                case 1:
                                    if (_gameSelected == 1)
                                    {
                                        rcName = "Soviet Union";
                                    }
                                    else
                                    {
                                        rcName = "USA";
                                    }
                                    break;
                                case 2:
                                    if (_gameSelected == 1)
                                    {
                                        rcName = "Oberkommando West";
                                    }
                                    else
                                    {
                                        rcName = "Wehrmacht";
                                    }
                                    break;
                                case 3:
                                    if (_gameSelected == 1)
                                    {
                                        rcName = "US Forces";
                                    }
                                    else
                                    {
                                        rcName = "Panzer Elite";
                                    }
                                    break;
                                case 4:
                                    if (_gameSelected == 1)
                                    {
                                        rcName = "UK Forces";
                                    }

                                    break;
                            }
                            Pls.Add(new Player
                            {
                                Nickname = Nickname,
                                Icon = ic,
                                Level = Level,
                                Race = ra,
                                RaceName = rcName,
                                Ranking = Rank,
                                SteamID = SteamID,
                                TimePlayed = HoursPlayed
                            });
                        }
                        _matches.Add(new Match
                        {
                            Date = GameDate,
                            Id = z,
                            MapFile = MapFileName,
                            MapName = MapName,
                            Type = Type,
                            ReplayFileName = Replay,
                            Players = Pls
                        });

                        z++;
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }
                }
            }
        }

        private void MatchHistoryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var cfg2 = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
            if (cfg2.IniReadValue("Match_History_Viewer", "Enabled").ToLower() == "false")
            {
                if (MessageBox.Show(this,
                    "Match History Viewer feature is currently disabled.\nDo you want to enable it?",
                    "Match History Viewer", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                    MessageBoxResult.Yes)
                {
                    cfg2.IniWriteValue("Match_History_Viewer", "Enabled", "true");
                }
                else
                {
                    Close();
                }
            }
            LoadMatches();
            MatchList.ItemsSource = _matches;
        }

        private void MatchList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (MatchList.Items.Count > 0)
            {
                if (MatchList.SelectedIndex != -1)
                {
                    var players = _matches[MatchList.SelectedIndex].Players;
                    playersList.ItemsSource = players;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to delete ALL matches?", "Delete Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var dir = MainWindow._AssemblyDir + @"\data\history\coh" + (_gameSelected + 1);
                var dbFile = dir + @"\mhv.xml";
                File.WriteAllText(dbFile, @"<Matches></Matches>");
                MatchList.ItemsSource = null;
                playersList.ItemsSource = null;
                lbl_Level.Content = "Level:";
                lbl_Rank.Content = "Rank:";
                lbl_TimePlayed.Content = "Time Played:";
                if (Directory.Exists(dir + @"\replays"))
                {
                    foreach (var file in Directory.GetFiles(dir + @"\replays"))
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                    }
                }
                MatchList.Items.Refresh();
            }
        }

        private void tBox_FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tBox_FilterText.Text.Length >= 3)
            {
                var icv = CollectionViewSource.GetDefaultView(MatchList.ItemsSource);
                icv.Filter = null;
                icv.Filter = Filter;
            }
            else if (tBox_FilterText.Text == "")
            {
                var icv = CollectionViewSource.GetDefaultView(MatchList.ItemsSource);
                icv.Filter = null;
            }
        }

        private bool Filter(object o)
        {
            if (o == null) return false;

            var match = o as Match;
            if (FilterDecision.SelectedIndex == 0)
            {
                if (match.MapName.ToLower().Contains(tBox_FilterText.Text.ToLower()))
                {
                    return true;
                }
            }
            else if (FilterDecision.SelectedIndex == 1)
            {
                foreach (var pl in match.Players)
                {
                    if (pl.Nickname.ToLower().Contains(tBox_FilterText.Text.ToLower()))
                    {
                        return true;
                    }
                }
            }
            else if (FilterDecision.SelectedIndex == 2)
            {
                foreach (var pl in match.Players)
                {
                    if (pl.SteamID.ToString().ToLower().Contains(tBox_FilterText.Text.ToLower()))
                    {
                        return true;
                    }
                }
            }
            else if (FilterDecision.SelectedIndex == 3)
            {
                foreach (var pl in match.Players)
                {
                    if (match.Date.ToLower().Contains(tBox_FilterText.Text.ToLower()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void FilterDecision_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                tBox_FilterText.Text = "";
                if (tBox_FilterText.Text.Length >= 3)
                {
                    var icv = CollectionViewSource.GetDefaultView(MatchList.ItemsSource);
                    icv.Filter = null;
                    icv.Filter = Filter;
                }
                else if (tBox_FilterText.Text == "")
                {
                    var icv = CollectionViewSource.GetDefaultView(MatchList.ItemsSource);
                    icv.Filter = null;
                }
            }
        }

        private void playersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                if (playersList.Items.Count > 0)
                {
                    if (playersList.SelectedIndex != -1)
                    {
                        var mc = MatchList.Items[MatchList.SelectedIndex] as Match;
                        SteamID_z = mc.Players[playersList.SelectedIndex].SteamID;
                        lbl_Level.Content = "Level: " + mc.Players[playersList.SelectedIndex].Level;
                        lbl_Rank.Content = "Rank: " + mc.Players[playersList.SelectedIndex].Ranking;
                        lbl_TimePlayed.Content = "Time Played: " + mc.Players[playersList.SelectedIndex].TimePlayed +
                                                 " Hours";
                    }
                }
            }
        }

        private void btnSteam_Click(object sender, RoutedEventArgs e)
        {
            if (playersList.Items.Count > 0)
            {
                if (playersList.SelectedIndex != -1)
                {
                    var mc = MatchList.Items[MatchList.SelectedIndex] as Match;
                    Process.Start("http://steamcommunity.com/profiles/" + mc.Players[playersList.SelectedIndex].SteamID);
                }
            }
        }

        private void btnPlayerCard_Click(object sender, RoutedEventArgs e)
        {
            if (playersList.Items.Count > 0)
            {
                if (playersList.SelectedIndex != -1)
                {
                    var mc = MatchList.Items[MatchList.SelectedIndex] as Match;
                    Process.Start("http://www.coh2.org/ladders/playercard/steamid/" +
                                  mc.Players[playersList.SelectedIndex].SteamID);
                }
            }
        }

        private void playersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (playersList.Items.Count > 0)
            {
                if (playersList.SelectedIndex != -1)
                {
                    expInfo.IsExpanded = !expInfo.IsExpanded;
                }
            }
        }

        private void rpl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var _cfgDocPath = "";
            var vImg = sender as Image;
            var mc = vImg.DataContext as Match;

            var cfg = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini"))
            {
                _cfgDocPath = cfg.IniReadValue("Essencial", "CoH" + (_gameSelected + 1) + "_DocPath");
            }

            if (MessageBox.Show(this, "Do you want to copy this replay to playback folder?", "Copy Replay",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var inp = new InputBox("Write a name for the replay:", "Replay name",
                    mc.MapName + "_" + DateTime.Now.ToString("M"));
                inp.ShowDialog();
                try
                {
                    logFile.WriteLine("MATCH HISTORY VIEWER - START COPYING REPLAY");
                    var value = inp.val.Replace(".rec", "");

                    File.Copy(MainWindow._AssemblyDir + @"\data\history\coh" + (_gameSelected + 1) + @"\replays\" +
                              mc.ReplayFileName, _cfgDocPath + @"\playback\" + value + ".rec", true);
                    Utilities.showMessage(this, "Replay saved.\nLocation:" + _cfgDocPath + @"\playback\" + value + ".rec",
                        "Saved");
                }
                catch (Exception ex)
                {
                    logFile.WriteLine("EXCEPTION Copying replay (MHV): " + ex);
                }
                logFile.WriteLine("MATCH HISTORY VIEWER - ENDED COPYING REPLAY");
            }
        }

        internal class Match
        {
            public int Id { get; set; }
            public string Date { get; set; }
            public string MapFile { get; set; }
            public string MapName { get; set; }
            public string ReplayFileName { get; set; }
            public int Type { get; set; }
            public List<Player> Players { get; set; }
        }

        internal class Player
        {
            public long SteamID { get; set; }
            public string Ranking { get; set; }
            public int Race { get; set; }
            public string Nickname { get; set; }
            public string RaceName { get; set; }
            public long TimePlayed { get; set; }
            public string Level { get; set; }
            public string Icon { get; set; }
        }
    }
}