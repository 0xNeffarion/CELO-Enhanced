   M S B A M L   `   `   `  ����  CELO#    CELO_Enhanced.Replay_Uploader   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          �Name$  UploadWindow��5   	    � Title$ CELO - Replay Uploader��5   	   $	��471��6(   $	��588��65   &   System.Windows.Media.TextOptions   TextFormattingMode$ Display=�6A    � 
ResizeMode$ NoResize=�5   	   $��Disabled=�6    � Icon$ icon-modified.ico��6S    � SizeToContent$ WidthAndHeight=�5   &    � WindowStartupLocation$ CenterOwner=�6E   $	��588��6i   $	��471��5   	    J� IsManipulationEnabled .  � UseLayoutRounding . .��5      � .+�5	   
   �� -   5        	 ��Name$	 	groupBox1��5	      $!��COH2.ORG Login credentials:��6&   $
��Left=�6K   $��	15,10,0,0q�6f   $	��Top=�5
      $	��111��6+   $	��549��68   .��5      � .+�5      �� -   5        
 ��Name$
 tBox_username��5      $
��Left=�61   $��19��6L   $��	75,16,0,0q�6X    �� TextWrapping$
 Wrap=�5      $	��Top=�6.   $	��452��6F    �� 	MaxLength$ 50��6R   5      �� 5      $
��Left=�6   $��	11,17,0,0q�67   $
��Wrap=�6J   $��	Username:��6^   $	��Top=�5      �� 5      $
��Left=�6   $��	13,57,0,0q�67   $
��Wrap=�6J   $��	Password:��6^   $	��Top=�5      G� 5      -   5         G�Name$ tBox_password��5      $
��Left=�65   $��19��6P   $��
75,53,0,17q�6\   $	��318��5       G� 	MaxLength$ 50��6*   69   �� 5      -   5         ��Name$ btnLogin��5      $��Login��6+   $
��Left=�6;   $��
407,46,0,0q�6V   $	��Top=�5      $	��120��61   $��33��6=    �� 	IsDefault . {�.  5      5      5      � 5   
   -   5         �Name$ pgBar��5      $
��Left=�6%   $��20��6@   $��
75,131,0,0q�6L   $	��Top=�6`   $	��485��5       . ���	#FFA0A0A0���	#FFDDE6E2��5      �� 5       �� EndPoint$	 1,0&�6&    � SpreadMethod$ Reflect=�65   .��5      �  � Color$ 	#FF2DC861��6#    � Offset$	 0.8Q�65   � 5       $ 	#FF24BFA5��6#   $	 0.2Q�65   5!      5#      �� 5$   
   -   5        $	 	groupBox2��5$      $��Replay Title��6&   $
��Left=�6<   $��
15,164,0,0q�6W   $	��Top=�5%      $��72��6+   $	��549��67   {�.  .��5'      � .+�5(      �� -   5        $
 
tBox_title��5(      $
��Left=�6.   $��23��6I   $��	10,13,0,0q�6U   $
 Wrap=�5)      $	��Top=�6.   $	��517��6F   $	 200��6R   5*      5+      �� 5,   
   -	   5        $	 	groupBox3��5,      $��Comment��6&   $
��Left=�67   $��
15,241,0,0q�6R   $	��Top=�5-      $	��137��6+   $	��549��68   {�.  .��5.      � .+�5/      �� -
   5        $
 tBox_comment��5/      $
��Left=�60   $��95��6K   $��	10,10,0,0q�6W   $
 Wrap=�50      $	��Top=�6.   $	��517��6F    �� AcceptsReturn .  �� MaxLines$ 15��6g   $
 8000��51      52      53      �� 54   
   -   5        $ 	btnUpload��54      $��Upload��6$   $
��Left=�65   $��320,385,0,0q�6P   $	��Top=�55      $	��240��6)   $��32��65   6A   �� 56   
   -   5        $ btnClose��56      $��Close��6#   $
��Left=�63   $��
15,385,0,0q�6N   $	��Top=�57      $	��114��6)   $��32��65   �� 58   
   $
��Left=�6   $��
15,132,0,0q�6/   $
��Wrap=�6C   $��	Progress:��6W   $	��Top=�59      7  1System.Windows.Forms.Integration.WindowsFormsHost  5;   
   -   5          Name$ winHost��5;      $
��Left=�6,   $��17��6G   $��159,400,0,0q�6S   $	��Top=�5<      $��21��63   $y�Hidden=�6>   5?      5@      