   M S B A M L   `   `   `  ���� [  WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms   CELO!   CELO_Enhanced.ReplayManager   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     RForms@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     -     �Name$  repMainWindow�� � Title$ CELO - Replay Manager��$	��627��$	��793��$��Roboto)���.  � 
ResizeMode$ CanMinimize=� � SizeToContent$ WidthAndHeight=�&   System.Windows.Media.TextOptions   TextFormattingMode$ Display=� � Icon$ icon-modified.ico�� � UseLayoutRounding .  � WindowStartupLocation$ CenterOwner=�$	��625��$	��790�� � 	Resources 	 �� 
TargetType
 �� 	Resources �� Color��%�   '��       �� "	 ��
 ���� $ 	#FF63A6A3��(��       * SystemColors.HighlightBrushKey+)�� $ 	#FFC0E4F0��(��       *SystemColors.ControlBrushKey+).��� $	��600��$	��790�� � RowDefinitions �� $
��463* ��� $
��137* �.+��� ���	#FFD5DFE5$��1q�$
��Left=�$	��110��$��	10,12,0,0q�$	��Top=�$	��440�� �� CornerRadius$ 4��$��1��._�� .+��� -    ��Name$ 	btnFilter��$
��Left=�$	��Top=�$��34��$��37��$��
393,14,0,0q� � ToolTip$ Apply Filter��$y�Hidden=� �� 	IsDefault . .���� $��Resources/btn_01.png��$��16��$��16�� �� BitmapScalingMode$ NearestNeighbor=� �� EdgeMode$ Aliased=� . �� -   $ btnCancelFilter��$
��Left=�$	��Top=�$��34��$��37��$��
393,62,0,0q�$ Remove Filter��$y�Hidden=�.���� $��Resources/defilter.png��$��16��$��16��$ NearestNeighbor=�$ Aliased=� . �� -    ��Name$ 
replayList��$
��Left=�$	��453��$��	10,10,0,0q�$��Center=�$	��438�� . $��Visible=� J� RenderTransformOrigin$ 0.5,0.5&����	#FFE0E0E0
���   � �� 
GroupStyle (  "System.Windows.Controls.GroupStyle     ContainerStyle �� "	 ��.���  �� Property � �� �� Value �� .R�9�  9� 
IsExpanded . ���	#FFB8B8B8$��0,0,0,1q�����  �� Orientation$ 
Horizontal=�.+��� $
��Bold"�$��11.75(�$��Bottom=����� *Name+.����  �� View �� .P��� $	��Map�� �� Width$ 75�� �� CellTemplate �� .R��� .+���  ��Name$ picReplayList��$��Center=�$
��Auto��$��75��$��Uniform=�$ Aliased=� . $��Center=����� *
map_file+�� $��Replay��$	 380�� �� .R��� .+��� $��Center=� �� TextAlignment$
 Left=�$
��Bold"�$��14(����� *name+�� $��Bottom=�$
 Left=�$��Italic$����� *
map_name+9� -     9�Name$  filterDates��$��Filter Dates��$
��Left=�$��	11,14,0,0q�$	��Top=�$��82��$	��380��$��0q�$��1����Z� .��� ! � ColumnDefinitions! �� $	��49* ��� $
��389* �.+�(  "System.Windows.Controls.DatePicker  -   "  Name$" date_end��$
��Left=�$��	49,34,0,0q�$	��Top=�$	��281��#   FirstDayOfWeek$# Monday=�$��1��  -   $" 
date_start��$
��Left=�$��49,5,0,0q�$	��Top=�$	��281��$# Monday=�$��1���� $��Starting date:��$
��Left=�$��10,4,0,0q�$	��Top=�$��2���� $��Ending date:��$
��Left=�$��	10,30,0,0q�$	��Top=�$��2��9� -   $  
filterName��$��Filter Names��$
��Left=�$��	11,39,0,0q�$	��Top=�$��82��$	��380��$ 9� ExpandDirection$$ Up=�$��0q�$��1����Z� .��� .+��� $��Search by name:��$
��Left=�$��	10,10,0,0q�$	��Top=�$	��102���� -	   % ��Name$% tBox_search��$
��Left=�$��23��$��
117,13,0,0q�& �� TextWrapping$
& Wrap=�$	��Top=�$	��257���� $��Actions��$
��Left=�$��455,325,0,15q�$	��312��$��2��.���� $ Vertical=�$��Center=�$��Center=�.+��� -
   $ 
btn_delete��$
��Left=�$��38��$	��Top=�$	��278��$��3��$	��0,7q�.��]� $��Center=�$��Center=�.+��� $!��Resources/replay_delete.png��$
��None=�$��27��$ NearestNeighbor=�$ Aliased=� . �� -   ' ��Name$' txt_delrep2��$!��Delete seletected replay(s)��$��Center=�$ Display=��� -   $ 
btn_rename��$
��Left=�$��38��$	��0,7q�$	��Top=�$	��278��$��3��.��]� $��Center=�$��Center=�.+��� $#��Resources/replay_editname.png��$
��None=�$��27��$ NearestNeighbor=�$ Aliased=� . �� -   $' txt_delrep3��$!��Rename seletected replay(s)��$��Center=�$ Display=��� -   $ btn_changeVersion��$
��Left=�$��38��$	��0,7q�$	��Top=�$	��278��$��3��.��]� $��Center=�$��Center=�.+��� $ ��Resources/replays_edit.png��$
��None=�$��27��$ NearestNeighbor=�$ Aliased=� . �� -   $' txt_editVersion��$%��Edit selected replay(s) version��$��Center=�$ Display=��� -   $ 
btn_upload��$
��Left=�$��38��$	��0,7q�$	��Top=�$	��278��$��3��.��]� $��Center=�$��Center=�.+��� $��Resources/coh2org.png��$��Uniform=�$��16��$ NearestNeighbor=�$ Aliased=� . �� -   $' txt_uploadReplay��$-��'  Upload selected replay(s) to COH2.ORG��$��Center=�$ Display=��� -   $ 	pic_error��$��Resources/pic_error.png��$��423,10,348,431q�$ Aliased=� . ( � Cursor$
( Hand��$  Replay's parsing warnings!��$��19��$y�Hidden=��� -   ) ��Name$) 
tabControl��$
��Left=�$	��310��$��
455,10,0,0q�$	��Top=�$	��312����Z� .@��� $��Replay Information��.��� $	��300��$��3,0,3,4q�.+�7  1System.Windows.Forms.Integration.WindowsFormsHost  $
��Left=�$	��125��$��	10,10,0,0q�$	��Top=�$	��280��*   
Background#* � �� . +   Child.+ %   System.Windows.Forms.PictureBox  -   pic_map��,   SizeMode$
, Zoom=�"   System.Windows.Forms.Control-   Enabled- . �� -   . ��Name$. txt_date��$��Date:��$
��Left=�$��
10,193,0,0q�$	��Top=�$	��280���� -   $. txt_time��$��Time:��$
��Left=�$��
10,219,0,0q�$	��Top=�$	��280���� -   $. txt_name��$��Name:��$
��Left=�$��
10,141,0,0q�$	��Top=�$	��280���� -   $. txt_mapname��$
��Map:��$
��Left=�$��
10,167,0,0q�$	��Top=�$	��280���� -   $. txt_version��$��Version:��$
��Left=�$��
10,246,0,0q�$	��Top=�$	��280���� -   / ��Name$/ 
tabPlayers��$��Players List��{�.  .��� $��4q� �� �� .+��� $��0��$��Allies��.���� -   0 ��Name$0 
AlliesList�� . $��Stretch=�$��Stretch=�$��Disabled=�$��Disabled=���Z�  ��%�   (��       * SystemColors.HighlightBrushKey+)(��      *$"SystemColors.HighlightTextBrushKey+)(�� (     *SystemColors.ControlBrushKey+)�� $ Transparent���� $ Black���� $ Transparent������ .R�A� 1 A� Columns$1 2��2 K� IsItemsHost2 . ���� .R��� $ 
Horizontal=�$��Center=�.+��� $ HighQuality=�$��Uniform=�$��23��$
��Left=�$��Center=����� *icon+ �� .+��� ���� *	race_name+�� $��Center=�$��14(�$��SemiBold"�$��CharacterEllipsis=�$
��Wrap=�3 ^� ToolTip3 �� *
nickname+�� $� ���� $� ���� ��� *
nickname+�� $��1��$
��Axis��.���� -   $0 AxisList�� . $��Stretch=�$��Stretch=�$��Disabled=�$��Disabled=���Z�  ��%�   (��       * SystemColors.HighlightBrushKey+)(��      *$"SystemColors.HighlightTextBrushKey+)(�� (     *SystemColors.ControlBrushKey+)�� $ Transparent���� $ Black���� $ Transparent������ .R�A� $1 2��2 . ���� .R��� $ 
Horizontal=�$��Center=�.+��� $ HighQuality=�$��Uniform=�$��23��$
��Left=�$��Center=����� *icon+ �� .+��� ���� *	race_name+�� $��Center=�$��14(�$��SemiBold"�$��CharacterEllipsis=�$
��Wrap=�3 �� *
nickname+�� $� ���� $� ���� ��� *
nickname+�� -   $/ tabChat��$
��Chat��{�.  .��� .+��� -   4 ��Name$4 txtReplayChat��5 �� AcceptsReturn5 . $��10q�6 �� 
IsReadOnly6 . 7 �� IsReadOnlyCaretVisible7 . "8 �� VerticalScrollBarVisibility$
8 Auto=�$��1q�$��12(���Z� ��Z� 