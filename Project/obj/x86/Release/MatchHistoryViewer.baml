   M S B A M L   `   `   `  ����   CELO&     CELO_Enhanced.MatchHistoryViewer   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     �Name$  MatchHistoryWindow�� � Title$! CELO - Match History Viewer��$	��506��$	��773�� � Icon$ icon-modified.ico�� � 
ResizeMode$ NoResize=�$��Disabled=� � SizeToContent$ WidthAndHeight=�&   System.Windows.Media.TextOptions   TextFormattingMode$ Display=� � WindowStartupLocation$ CenterOwner=� � UseLayoutRounding . $	��776��$	��504�� � 	Resources 	 �� 
TargetType
 �� Property �� Value��%0   '��       �� "	 ��.��� 
 � ��$ Right=�.��� $��0,0,4,1q�.+��� $��
Filter by:��$
��Left=�$��165,414,0,0q�$	��Top=�$��25��$��65�� J� RenderTransformOrigin$ 
0.615,0.72&��� -    ��Name$ FilterDecision��$
��Left=�$��169,438,0,0q�$	��Top=�$	��137��$��Center=� �� SelectedIndex$ 0��.@��� $��	Match Map���� $��Player Nickname���� $��Player Steam ID���� $��
Match Date���� -    ��Name$ tBox_FilterText��$
��Left=�$��21��$��311,438,0,0q�$	��Top=�$	��142��$��Center=��� -    ��Name$ 	MatchList��$
��Left=�$	��403��$��	10,10,0,0q�$	��Top=�$	��495�� �� SelectionMode$ Single=� . $��Visible=� ��%4   '��       �� "	 ��.��� 
 � y�$ 	Collapsed=� �� View �� .P���  �� CellTemplate �� .R���  �� Orientation$ 
Horizontal=�.+���  �� EdgeMode$ Aliased=� . $��Uniform=�$��50��$
��Left=����� *	MapFile+ � ToolTip �� $ Vertical=�.+���  �� BitmapScalingMode$ NearestNeighbor=�$ Aliased=� . $��Uniform=�$	��270������ *	MapFile+�� $��13(�
���   �$��Center=�$��SemiBold"� �� Padding$	 0,2q����� *	MapName+��  �� .R��� $ Vertical=�.+��� $��12(��� ��� *Type+ �� $�vs�� �� ��� *Type+�� $��Italic$�$��13(����� *	MapName+��  �� Width$ 50�� �� .R��� $ 
Horizontal=�$��Right=� � Cursor$
 Hand��.+���  ��Name$	 rpl��-   $��Resources/replay_copy.png��$ NearestNeighbor=�$ Aliased=� . $��20��$ Copy saved replay���� $	 159�� �� .R�]� $��Center=�$��Right=�.+��� $��13(�$��Right=�$��SemiBold"����� *Date+9� -    9�Name$ expInfo��$��Player Information��$
��Left=�$��510,268,0,0q�$	��Top=�$	��147��$	��246�� 9� 
IsExpanded . ���	#FFD5DFE5.��� .+��� -    ��Name$ 	lbl_Level��$��Level:��$
��Left=�$��10,4,0,0q�$	��Top=��� -   $ lbl_TimePlayed��$��Time played:��$
��Left=�$��	10,28,0,0q�$	��Top=��� -	   $ lbl_Rank��$��Rank:��$
��Left=�$��	12,53,0,0q�$	��Top=��� -
    ��Name$ btnPlayerCard��$��
114,81,0,8q�$
��Left=�$	��120��.��]� $��Center=�$
��Left=�.+��� $��Resources/coh2_ico.gif��$
��None=�$��20��$ NearestNeighbor=�$ Aliased=� . �� $��Player Card��$��Center=�$ Display=��� -   $ btnSteam��$��	10,81,0,8q�$
��Left=�$��99��.��]� $��Center=�$��Center=�.+��� $��Resources/stream_ico.png��$
��None=�$��20��$ NearestNeighbor=�$ Aliased=� . �� $��Steam Profile��$��Center=�$ Display=��� $��Players��$
��Left=�$��	510,5,0,0q�$	��Top=�$	��258��$	��246��.��� .+��� -     ��Name$  playersList��$
��Left=�$	��221��$	��Top=�$	��221��$��3,5,0,0q� . ��Z� ���� .R��� $ 
Horizontal=�$	��Top=�.+��� $��Uniform=�$��23��$
��Left=����� *Icon+ �� .+��� ���� *
RaceName+�� $	��Top=�$��13(�$��SemiBold"��� $� �� �� ��� *
Nickname+�� -   $ btnClose��$��Close��$
��Left=�$��
10,427,0,0q�$	��Top=�$	��150��$��32���� -   $ btnClear��$��Clear matches��$
��Left=�$��575,427,0,0q�$	��Top=�$	��179��$��32��