   M S B A M L   `   `   `  ����   CELO"    CELO_Enhanced.VersionChanger   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     �Name$  VersionWindow��$	��300��$	��615�� � WindowStartupLocation$ CenterOwner=� � SizeToContent$ WidthAndHeight=� � 
ResizeMode$ CanMinimize=�$	��300��&   System.Windows.Media.TextOptions   TextFormattingMode$ Display=� � Title$# CELO - Replay Version Changer��$	��615�� � Icon$ icon-modified.ico��.��� .+��� ���	#FFD5DFE5$��1q�$
��Left=�$	��181��$��	10,73,0,0q�$	��Top=�$	��302��._��� ���	#FFC1CBD1$��1q�$
��Left=�$��53��$��148,120,0,0q�$	��Top=�$	��147��._��� -    ��Name$ txt_lastVersion��$��Last Version:��$��Center=�$��	-1,6,-1,0q�$	��147��$��Center=��� $��        Warning��$
��Left=�$��
322,10,0,0q�$	��Top=�$	��186��$	��266��.��� $��0,0,0,2q�.+��� $
��Left=�$��8,18,0,0q�$
��Wrap=�$����Replays that are very old will not work due to the fact that the unit values are different and the game cannot sync the replay with the actual updated values of the units.��$	��Top=�$��67��$	��236���� $
��Left=�$��8,96,0,0q�$
��Wrap=�${��uChanging versions is mostly recommended when the game is patched/hotfixed recently and the replay is 1-2 patches old.��$	��Top=�$	��236��$��59���� $
��Left=�$��32��$��	325,3,0,0q� �� BitmapScalingMode$ NearestNeighbor=�	 �� EdgeMode$	 Aliased=�$
��None=�$	��Top=�$��32��$��Resources/warning.png���� -   $ txt_original_name��$��Name:��$
��Left=�$��	10,18,0,0q�$	��Top=�$	��302���� -   $ txt_original_version��$��Version:��$
��Left=�$��	10,43,0,0q�$	��Top=�$	��302��	� -   
 	�Name$
 rBtn_overwrite��$��Overwrite replay��$
��Left=�$��	24,89,0,0q�$	��Top=� d� 	IsChecked$
 TrueY�$��1��	� -   $
 rBtn_backup��$��Create backup��$
��Left=�$��
210,89,0,0q�$	��Top=�$��2���� -    ��Name$ 	tBox_name��$
��Left=�$��23��$��107,121,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��195��{�.  $��3���� $
��Left=�$��
24,124,0,0q�$
��Wrap=�$��Backup name:��$	��Top=�$��83���� -   $ tBox_version��$
��Left=�$��23��$��138,164,0,0q�$
 Wrap=�$	��Top=�$	��164�� �� MaxLines$ 1�� �� 	MaxLength$ 5��$��4���� $
��Left=�$��
24,168,0,0q�$
��Wrap=�$��New version:     4.0.0.��$	��Top=��� -	    ��Name$ btn_lastVersion��$��Set last version��$
��Left=�$��
24,210,0,0q�$	��Top=�$	��122��$��29��$��5���� -
   $ 	btn_apply��$��Apply��$
��Left=�$��335,207,0,0q�$	��Top=�$	��134��$��44��{�.  $��6���� -   $ 
btn_cancel��$��Cancel��$
��Left=�$��497,207,0,0q�$	��Top=�$��91��$��44��$��7��