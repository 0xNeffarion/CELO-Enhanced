   M S B A M L   `   `   `  ����  CELO    CELO_Enhanced.InputBox   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          �Name$  InputWindow��5   	    � Title$ %TITLE%��5   	   $	��162��6   $	��506��6&   &   System.Windows.Media.TextOptions   TextFormattingMode$ Display=�62    � ShowInTaskbar .  $��Disabled=�5   	    � WindowStartupLocation$ CenterOwner=�6=    � UseLayoutRounding .  � 
ResizeMode$ NoResize=�5   "    � SizeToContent$ WidthAndHeight=�68   $	��511��6W   $	��170��6f    � Icon$ icon-modified.ico��5   	   .��5      � .+�5	   
   �� -   5        	 ��Name$	 lbl_Caption��5	      $��	%CAPTION%��6%   $
��Left=�69   $��	10,10,0,0q�6T   $	��Top=�5
      $��54��6(   $	��473��64   $ Display=�6@    .  $��0��5   *   �� 5   
   -   5        
 ��Name$
 	tBox_Text��5      $
��Left=�6%   $��23��6@   $��	10,69,0,0q�6L   $��%TEXT%��6_   $	��Top=�5      $	��473��6*   $ Display=�66   $��1��6_   5      �� 5   
   -   5         ��Name$ btn_OK��5      $��279,100,0,10q�6!   $
��Left=�67   $	��113��6R    �� 	IsDefault . $��2��5      .��5      ]� $��Center=�6   $��Center=�63   .+�5      �� $��Resources/accept.png��6   $
��None=�66   $��16��6E    �� BitmapScalingMode$ NearestNeighbor=�5       �� EdgeMode$ Aliased=�6J    . 5      �� 5      $	�� OK��6   $��Center=�6'   $ Display=�6B   5      5      �� 5   
   -   5        $ 
btn_Cancel��5      $��406,100,0,10q�6%   $
��Left=�6;   $��79��6V    �� IsCancel . $��3��5      .��5      ]� $��Center=�6   $��Center=�63   .+�5      �� $��Resources/cancel.png��6   $
��None=�66   $��16��6E   $ NearestNeighbor=�5      $ Aliased=�6J    . 5       �� 5!      $�� Cancel��6   $��Center=�6+   $ Display=�6F   5"      5$      5&      5'      