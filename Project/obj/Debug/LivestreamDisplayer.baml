   M S B A M L   `   `   `  ����  CELO'    !CELO_Enhanced.LivestreamDisplayer   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          �Name$  DisplayerWindow��5   	    � Title$! CELO - Livestream Displayer��5   	   $	��403��6-   $	��849��6:    � 
ResizeMode$ CanMinimize=�6F   $��Disabled=�5   	    � SizeToContent$ WidthAndHeight=�6=   &   System.Windows.Media.TextOptions   TextFormattingMode$ Display=�5   	    � WindowStartupLocation$ CenterScreen=�62    � Icon$ icon-modified.ico��6W   $	��857��5   )   $	��413��68   .��5      � .+�5	   
   �� ���	#FFB2C8D4$��1q�6)   $
��Left=�6=   $	��279��6X   $��
612,82,0,0q�6e   $	��Top=�5
      $	��219��6)   ._�5      � .+�5      �� $
��Left=�6   $
��Wrap=�67   $����This way you can put players information into your Livestream. In order to add them using OBS (Open Broadcaster Software), you have a add a TEXT Source in your Scene, and chose the player files.��5      $	��Top=�5      $	��148��64   $	��200��6A   $��
10,125,0,0q�6M    �� TextAlignment$ Center=�6a   $��SemiBold"�5      $��13(�62   �� 5      $
��Left=�6   $��	10,10,0,0q�67   $
��Wrap=�6J   $����The Livestream Displayer creates a file for each player with the specified variables in any order or layout when a game is about to start.��5      $	��Top=�5      $��13(�64   $ Center=�6B   $	��104��6Y   5      5      �� 5   
   ���	#FFB2C8D4$��1q�6)   $
��Left=�6=   $��64��6X   $��
612,13,0,0q�6d   $	��Top=�5      $	��219��6)   �� 5   
   -   5         ��Name$ playerListBox��5      $
��Left=�6)   $	��212��6D   $��8,12,0,0q�6Q   $	��Top=�5      $	��191��6*   $��Center=�66   $��15(�6Z   	 �� SelectedIndex$	 0��5      $��Disabled=�5      $��Disabled=�5      $��SemiBold"�6H   $��Center=�5      .@�5       �� $��Player 1 (You)��6   �� 5!      $��Player 2��6   �� 5"      $��Player 3��6   �� 5#      $��Player 4��6   �� 5$      $��Player 5��6   �� 5%      $��Player 6��6   �� 5&      $��Player 7��6   �� 5'      $��Player 8��6   5(      �� 5)   
   $
��File��6   $
��Left=�6!   $��
208,42,0,0q�6<   $	��Top=�6P   $	��182��6h   $	��392��5*      .��5+      � $��0,0,0,-1q�6   .+�5,      �� -   5        
 ��Name$
 tBox_FileContent��5,      $
��Left=�64   $	��140��6O   $��	10,10,0,0q�6\    �� TextWrapping$
 Wrap=�5-      $	��Top=�6.   $	��358��6F   .��5.   E   ��5/      �� 50       �� EndPoint$
 0,20&�6.    � MappingMode$ Absolute=�6>    �� 
StartPoint$	 0,0&�6U   .��51      �  � Color$ 	#FFABADB3��6+    � Offset$
 0.05Q�6=   � 52      $ 	#FFE2E3EA��6+   $
 0.07Q�6=   � 53      $ 	#FFC5CFD8��6+   $ 1Q�6=   54      .��55   +   56      57      58      �� 59   
   $
��Left=�6   $��46��6+   $��
694,22,0,0q�67   $	��Top=�6K   $��86��6c   $��Resources/vidcap.png��5:      �� 5;   
   $��	Variables��6   $
��Left=�6&   $��208,229,0,0q�6A   $	��Top=�6V   $	��135��5<      $	��392��6    .��5=      � .+�5>      �� $��Nickname - ��6   $
��Left=�6.   $��	21,18,0,0q�6I   $	��Top=�6\   $��73��5?      �� 5@      $��Ladder Rank - ��6   $
��Left=�61   $��	10,43,0,0q�6L   $	��Top=�6_   $��84��5A      �� 5B      $��Hours Played - ��6   $
��Left=�62   $��
199,18,0,0q�6M   $	��Top=�6a   �� 5C      $��Game Level - ��6   $
��Left=�60   $��
209,43,0,0q�6K   $	��Top=�6_   $��81��5D      �� 5E      $��Steam ID - ��6   $
��Left=�6.   $��	28,68,0,0q�6I   $	��Top=�6\   $��66��5F      �� 5G      $��%NICK%��6   $
��Left=�6)   $��	94,18,0,0q�6D   $	��Top=�6W   $
��Bold"�5H      �� 5I      $��%RANK%��6   $
��Left=�6)   $��	94,43,0,0q�6D   $	��Top=�6W   $
��Bold"�5J      �� 5K      $��	%STEAMID%��6   $
��Left=�6,   $��	94,68,0,0q�6G   $	��Top=�6Z   $
��Bold"�5L      �� 5M      $��%HOURS%��6   $
��Left=�6*   $��
290,18,0,0q�6E   $	��Top=�6Y   $
��Bold"�5N      �� 5O      $��%LEVEL%��6   $
��Left=�6*   $��
290,43,0,0q�6E   $	��Top=�6Y   $
��Bold"�5P      5Q      5R      �� 5S   
   -   5         ��Name$ btnCreateAll��5S      $��Save all files��6'   $
��Left=�6@   $��
10,229,0,0q�6[   $	��Top=�5T      $	��189��6)   $��31��65   6A   �� 5U   
   -   5        $ btnCreateSelected��5U      $��Save for selected player��6,   $
��Left=�6O   $��
10,265,0,0q�5V      $	��Top=�6%   $	��189��6=   $��31��6I   6U   �� 5W   
   -   5        $ btnClose��5W      $��Close��6#   $
��Left=�63   $��
10,330,0,0q�6N   $	��Top=�5X      $	��189��6)   $��31��65   6A   �� 5Y   
   -   5        $
 	tBox_path��5Y      $
��Left=�6%   $��24��6@   $��
295,13,0,0q�6L   $	��Top=�6`   $	��227��5Z      {�.  6   �� 5[   
   -   5        $ 	btnBrowse��5[      $��Browse��6$   $
��Left=�65   $��
527,13,0,0q�6P   $	��Top=�5\      $��73��6)    J� RenderTransformOrigin$ 0.479,1&�64   $��24��6T   6`   �� 5]   
   $��Output Folder:��6   $
��Left=�6)   $��
208,12,0,0q�6D   $	��Top=�6X   5_      5`      