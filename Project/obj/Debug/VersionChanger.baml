   M S B A M L   `   `   `  ����  CELO"    CELO_Enhanced.VersionChanger   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          �Name$  VersionWindow��5   	   $	��300��5   @   $	��615��6M    � WindowStartupLocation$ CenterOwner=�5   	    � SizeToContent$ WidthAndHeight=�6-    � 
ResizeMode$ CanMinimize=�6L   $	��300��6e   &   System.Windows.Media.TextOptions   TextFormattingMode$ Display=�5   	    � Title$# CELO - Replay Version Changer��62   $	��615��5   	    � Icon$ icon-modified.ico��6   .��5      � .+�5   
   �� ���	#FFD5DFE5$��1q�6)   $
��Left=�6=   $	��181��6X   $��	10,73,0,0q�6e   $	��Top=�5	      $	��302��6)   ._�5
      �� ���	#FFC1CBD1$��1q�6-   $
��Left=�6A   $��53��6\   $��148,120,0,0q�5      $	��Top=�6*   $	��147��6B   ._�5      �� -   5         ��Name$ txt_lastVersion��5      $��Last Version:��61   $��Center=�6I   $��	-1,6,-1,0q�6f   $	��147��5      $��Center=�6$   5      5      �� 5   
   $��        Warning��6   $
��Left=�6,   $��
322,10,0,0q�6G   $	��Top=�6[   $	��186��5      $	��266��6    .��5      � $��0,0,0,2q�6   .+�5      �� $
��Left=�6   $��8,18,0,0q�67   $
��Wrap=�6I   $����Replays that are very old will not work due to the fact that the unit values are different and the game cannot sync the replay with the actual updated values of the units.��5      $	��Top=�5      $��67��64   $	��236��6@   �� 5      $
��Left=�6   $��8,96,0,0q�67   $
��Wrap=�6I   ${��uChanging versions is mostly recommended when the game is patched/hotfixed recently and the replay is 1-2 patches old.��5      $	��Top=�5      $	��236��64   $��59��6@   5      5      �� 5   
   $
��Left=�6   $��32��6+   $��	325,3,0,0q�67    �� BitmapScalingMode$ NearestNeighbor=�5      	 �� EdgeMode$	 Aliased=�6B   $
��None=�6c   $	��Top=�5      $��32��6(   $��Resources/warning.png��63   �� 5    
   -   5        $ txt_original_name��5       $��Name:��6+   $
��Left=�6;   $��	10,18,0,0q�6V   $	��Top=�5!      $	��302��6(   �� 5"   
   -   5        $ txt_original_version��5"      $��Version:��6.   $
��Left=�6A   $��	10,43,0,0q�6\   $	��Top=�5#      $	��302��6(   	� 5$   
   -   5        
 	�Name$
 rBtn_overwrite��5$      $��Overwrite replay��6.   $
��Left=�6I   $��	24,89,0,0q�6d   $	��Top=�5%       d� 	IsChecked$
 TrueY�6.   $��1��6`   	� 5&   
   -   5        $
 rBtn_backup��5&      $��Create backup��6+   $
��Left=�6C   $��
210,89,0,0q�6^   $	��Top=�5'      $��2��6L   �� 5(   
   -   5         ��Name$ 	tBox_name��5(      $
��Left=�6%   $��23��6@   $��107,121,0,0q�6L    �� TextWrapping$
 Wrap=�6a   $	��Top=�5)      $	��195��6*   {�.  $��3��6l   �� 5*   
   $
��Left=�6   $��
24,124,0,0q�6/   $
��Wrap=�6C   $��Backup name:��6W   $	��Top=�5+      $��83��6,   �� 5,   
   -   5        $ tBox_version��5,      $
��Left=�6(   $��23��6C   $��138,164,0,0q�6O   $
 Wrap=�6d   $	��Top=�5-      $	��164��6*    �� MaxLines$ 1��6]    �� 	MaxLength$ 5��6j   $��4��5.   =   �� 5/   
   $
��Left=�6   $��
24,168,0,0q�6/   $
��Wrap=�6C   $��New version:     3.0.0.��6W   $	��Top=�50      �� 51   
   -	   5         ��Name$ btn_lastVersion��51      $��Set last version��6*   $
��Left=�6E   $��
24,210,0,0q�6`   $	��Top=�52      $	��122��6)   $��29��65   $��5��6_   �� 53   
   -
   5        $ 	btn_apply��53      $��Apply��6$   $
��Left=�64   $��335,207,0,0q�6O   $	��Top=�54      $	��134��6)   $��44��65   {�.  $��6��6S   6`   �� 55   
   -   5        $ 
btn_cancel��55      $��Cancel��6%   $
��Left=�66   $��497,207,0,0q�6Q   $	��Top=�56      $��91��6)   $��44��64   $��7��6Y   58      59      