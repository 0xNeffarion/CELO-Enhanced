   M S B A M L   `   `   `  ���� ?  ;CELO, Version=1.1.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:CELO_Enhanced.PropertiesCELO_Enhanced.Properties  O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem [ WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms -clr-namespace:CELO_EnhancedCELO_Enhanced      CELO_Enhanced.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     =
properties&clr-namespace:CELO_Enhanced.Properties     9System&clr-namespace:System;assembly=mscorlib     RForms@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     4celoEnhancedclr-namespace:CELO_Enhanced     -     �Name$  	Celo_Main�� � Title$ CELO - Main��$	��575��$	��873��$��Roboto)� � 
ResizeMode$ CanResizeWithGrip=� � Icon$ icon-modified.ico��$	��450��$	��611��#��� �� � ShowActivated .  &   System.Windows.Media.TextOptions   TextFormattingMode$ Display=�$��Disabled=� � WindowStartupLocation$ CenterScreen=� � UseLayoutRounding .  J� IsHitTestVisible . ��. $��13(�	 � 	Resources	    CELO_Enhanced.Converters   	ImageConv��%   &	          .��� -   
 �Name$
 grdMain�� � ColumnDefinitions �� $
��366* ��� $
��307* ��� $
��194* � � RowDefinitions �� $��25 ��� $	��89* ��� $
��100* ��� $
��174* ��� $
��134* ��� $	��23* �.+�]� -    ]�Name$ 
docPanName����.  $��3��.+�o� -    o�Name$ menuMain��$��25��$	��Top=�$	��Top=�$��0,0,0,1q�$��Roboto)�$��2,1,0,0q�$��1,2,0,0q�#��� ������ .R�]� $��Stretch=�.@�m� -    m�Name$ mnuFile��$
��File��$��1��.@�m� -   $ 
mnuRestart��$��Restart CELO�� m� Icon �� $��Resources/menu_02.png��$��16��$��16��$
��Fill=��� m� -   $ mnuExit��$
��Exit�� �� $��Resources/menu_01.png��m� -   $ mnuView��$
��View��$��2��.@�m� -	   $ mnuItemWindowTop��$��Window always on top�� m� IsCheckable .  �� $��Resources/menu_03.png��$��16��$��16��$
��Fill=�m� -
   $ mnuTools��$��Tools��$��3��.@�m� -   $ mnuReplayManager��$��Replay Manager�� �� $��Resources/menu_04.png��$��16��$��16��$
��Fill=�m� -   $ mnuMHV��$��Match History Viewer�� �� $��Resources/menu_05.png��$��16��$��16��$
��Fill=�m� -   $ mnu_lsd��$��Livestream Displayer�� �� $��Resources/vidcap.png��$��16��$��16��$
��Fill=�m� -   $ mnu_ahk��$��Hot-Key Generator�� �� $��Resources/menu_017.png��$��16��$��16��$
��Fill=�m� -   $ mnuSettings��$��Settings��$��4��.@�m� -   $ mnuPref��$��Preferences�� �� $��Resources/menu_06.png��$��16��$��16��$
��Fill=�m� -   $ mnuHelp��$
��Help��$��5��.@�m� -   $ mnuCheckUpd��$��Check for Updates�� �� $��Resources/menu_07.png��$��16��$��16��$
��Fill=�m� -   $ mnuLogs��$��Open Logs Folder�� �� $��Resources/menu_018.png��$��16��$��16��$
��Fill=�m� -   $��About�� �� $��Resources/menu_015.png��$��16��$��16��$
��Fill=�m� -   $ mnuNewUpdate��$��14(�$
��Bold"����	#FFFF8000$��!NEW UPDATE IS AVAILABLE!��$	��Top=�$��0q� � Cursor$
 Hand��$y�	Collapsed=�$��Disabled=�{�.   J� Opacity$ 0Q�$��0q�|�.  ��.  ��Z� ��Z� m� -   $ 	mnuDonate��$��Donate��$��5��$��Right=�m� -   $ 	mnuSendFb��$��Send feedback��$��6��$��95��$��Right=��� -    ��Name$ groupMatchInfo��$��Match Information��$��1��$��2��$��2��$��6,6,13,6q�.��� .+��� $��2q�.+��� -    ��Name$ 
match_drop��$6��0Approx. Warspoil countdown: 03:00:00 (F4 Resets)��$��38��$��11(�$ Display=�$
��Wrap=� �� TextAlignment$ Center=�$��0,2,0,0q��� -    ��Name$ 	match_map��$��Map: N/A��$��25��$��11(�$ Display=�$��Stretch=�$��Stretch=��� -    ��Name$ match_mapImg��$��75�� �� BitmapScalingMode$ HighQuality=� �� EdgeMode$ Aliased=� . $��6,1,6,6q� � ToolTip ��  �� Orientation$ Vertical=�.+��� -   $ match_imgTooltip��$ NearestNeighbor=�$ Aliased=� . $��Uniform=�$	��250���� -   $ match_mapName��$��14(�
���   �$��Center=�$��SemiBold"� �� Padding$	 0,2q��� -   $ groupGameInfo��$��Game Information��$��3��$��6,6,13,6q�$��2��.���� .+��� -   $ 
game_build��$��Game Build: N/A��$��1q�$��25��$��11(�$ Display=��� -    $ 	game_ping��$��Battle-Servers Ping: N/A��$��25��$��1q�$��11(�$ Display=��� -!   $ game_replaysCount��$��Replays Recorded: N/A��$��1q�$��25��$��11(�$ Display=��� -"   $ game_cpm��$��CPM: N/A��$��25��$��1q�$��11(�$ Display=��� -#   $ game_cpmTotal��$��Clicks: N/A��$��25��$��1q�$��11(�$ Display=��� -$    ��Name$ 
playerList��$��2�� �� SelectionMode$ Single=�$��13(����	#FF343434
���   �$��1q�  �� IsTextSearchEnabled  .  $��Visible=���. $��3��$��2��$��12,5,5,3q�$��8��! � ContextMenu! �� .@�m� -%   $ 
mnu_p_Copy��$
��Copy��{�.   �� $��Resources/menu_09.png��$��16��$��16��$
��Fill=�.@�m� -&   $ mnu_p_copyNick��$��Nickname�� �� $��Resources/menu_011.png��$��16��$��16��$
��Fill=�m� -'   $ mnu_p_copyRank��$��Ladder Rank�� �� $��Resources/menu_012.png��$��16��$��16��$
��Fill=�m� -(   $ mnu_p_copyTimePlayed��$��Hours Played�� �� $��Resources/menu_013.png��$��16��$��16��$
��Fill=�m� -)   $ mnu_p_copyLevel��$��Level�� �� $��Resources/menu_016.png��$��16��$��16��$
��Fill=�m� -*   $ mnu_p_copyID��$��Steam ID�� �� $��Resources/menu_014.png��$��16��$��16��$
��Fill=�m� -+   $ 
mnu_p_Open��$
��Open��{�.   �� $��Resources/menu_010.png��$��16��$��16��$
��Fill=�.@�m� -,   $ mnu_p_open_coh2org��$��CoH2.org Player Card�� �� $��Resources/coh2org.png��$��16��$��16��$
��Fill=�m� --   $ mnu_p_open_coh��$%��Companyofheroes.com Player Card�� �� $��Resources/coh2_icon.png��$��16��$��16��$
��Fill=�m� -.   $ mnu_p_open_steampage��$��Steam Profile�� �� $��Resources/steam.png��$��16��$��16��$
��Fill=����� " �� 
TargetType"" ��.��� # �� Property# � ��$ �� Value$$ Stretch=�% �� 
GroupStyle% (  "System.Windows.Controls.GroupStyle  &   ContainerStyle& �� "" ��.��� # � ��$ �� .R�9� ' 9� 
IsExpanded' . ���	#FFB8B8B8$��0,0,0,1q����� $ 
Horizontal=�.+��� $
��Bold"�$��11.75(�$��Bottom=����� *Name+.���� ( �� View( �� ) �� ColumnHeaderContextMenu) �� .P��� * �� Width$	* 250��$��Steam Nickname��+ �� CellTemplate+ �� .R��� $ 
Horizontal=�$
��Left=�.+��� $
��Left=�$��19��$ HighQuality=�$��19������ *Icon+, �� 	Converter#, [   �� .+��� ���� *
RaceName+�� $
 Left=�$
��Left=�$��2,0,0,0q�$��CharacterEllipsis=�$��Normal"����� *
Nickname+�� $	* 300��$��Ladder Rank��+ �� .R��� $ 
Horizontal=�$��Center=�.+��� $ Center=�$��Center=�$��Medium"�$��13(����� *	Ranking+�� $ Center=�$��Center=�$��SemiBold"����� *RankingAfter+�� -/   $ groupWatcher��$��Game Watcher��$��1��$��0��$��12,5,5,5q�.��� .+�7  1System.Windows.Forms.Integration.WindowsFormsHost  -0   -  Name$- LoadFormHost��$��20��$��20��$��Center=�$��Right=�$��0,0,20,0q�$y�Hidden=�.   Child.. %  System.Windows.Forms.PictureBox  -1   pLoadpic��"  System.Windows.Forms.Control/   Enabled/ . 0   SizeU QSystem.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"  System.Drawing.SizeConverter$0 20, 20 1   SizeMode$
1 Zoom=�2   
WaitOnLoad2 . �� -2   3 ��Name$3 btn_GameWatcher��
4 � Tag$4 en��$��10,10,57,10q�$��6��.��]� $��Center=�$��Center=�.+��� $"��Resources/GameWatcherPic.png��$
��None=�$��27��$ NearestNeighbor=�$ Aliased=� . �� -3   $ txt_GameWatcher��$��Start Game Watcher��$��Center=�$ Display=�$��Roboto)��� -4   $ groupReplays��$��Replays��$��1��$��1��$��5q�.���� -5   $3 btn_ReplayManager��$��10q�$��7��.��]� $��Center=�$��Center=�.+��� $!��Resources/ReplayManager.png��$
��None=�$��36��$ NearestNeighbor=�$ Aliased=� . �� -6   $ txt_ReplayManager��$��Replay Manager��$��Center=�$ Display=�9� -7   5 9�Name$5 statusExpander��$��Status��$��4��$��6,6,13,6q�' . $��2��$��2����Z� .���  �� $	��40* ��� $
��100* �.+��� ���	#FF8D8D8D$��5q�$��1q�$��0��$��0��._�� .+��� $ 
Horizontal=�$��Center=�.+��� -8   $ status_gameIcon��$ NearestNeighbor=�$ Aliased=� . $��16��$��16��$
��None=�$��5,0,0,0q��� -9   $ status_gameName��$�� ��$��25��$��11(�$��Stretch=�$��Stretch=��� -:   $ status_cont��$��8q����	#FF8D8D8D$��1q�$��11(�$��1��$��Center=�$��Center=���Z� .���� -;   6 ��Name$6 status_cont_text��7 �� TextAlignment$7 Center=�8 �� TextWrapping$
8 Wrap=�$
��Idle��� -<   9 �Name$9 pgBarLoading��$��5�����	#FF1C951C{�.  $��2��$��12,6,5,7q�