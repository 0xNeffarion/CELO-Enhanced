   M S B A M L   `   `   `  ����   CELO#    CELO_Enhanced.Replay_Uploader   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     �Name$  UploadWindow�� � Title$ CELO - Replay Uploader��$	��471��$	��588��&   System.Windows.Media.TextOptions   TextFormattingMode$ Display=� � 
ResizeMode$ NoResize=�$��Disabled=� � Icon$ icon-modified.ico�� � SizeToContent$ WidthAndHeight=� � WindowStartupLocation$ CenterOwner=�$	��588��$	��471�� J� IsManipulationEnabled .  � UseLayoutRounding . .��� .+��� -   	 ��Name$	 	groupBox1��$!��COH2.ORG Login credentials:��$
��Left=�$��	15,10,0,0q�$	��Top=�$	��111��$	��549��.��� .+��� -   
 ��Name$
 tBox_username��$
��Left=�$��19��$��	75,16,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��452�� �� 	MaxLength$ 50���� $
��Left=�$��	11,17,0,0q�$
��Wrap=�$��	Username:��$	��Top=��� $
��Left=�$��	13,57,0,0q�$
��Wrap=�$��	Password:��$	��Top=�G� -    G�Name$ tBox_password��$
��Left=�$��19��$��
75,53,0,17q�$	��318�� G� 	MaxLength$ 50���� -    ��Name$ btnLogin��$��Login��$
��Left=�$��
407,46,0,0q�$	��Top=�$	��120��$��33�� �� 	IsDefault . {�.  � -    �Name$ pgBar��$
��Left=�$��20��$��
75,131,0,0q�$	��Top=�$	��485�� . ���	#FFA0A0A0���	#FFDDE6E2����  �� EndPoint$	 1,0&� � SpreadMethod$ Reflect=�.���  � Color$ 	#FF2DC861�� � Offset$	 0.8Q�� $ 	#FF24BFA5��$	 0.2Q��� -   $	 	groupBox2��$��Replay Title��$
��Left=�$��
15,164,0,0q�$	��Top=�$��72��$	��549��{�.  .��� .+��� -   $
 
tBox_title��$
��Left=�$��23��$��	10,13,0,0q�$
 Wrap=�$	��Top=�$	��517��$	 200���� -	   $	 	groupBox3��$��Comment��$
��Left=�$��
15,241,0,0q�$	��Top=�$	��137��$	��549��{�.  .��� .+��� -
   $
 tBox_comment��$
��Left=�$��95��$��	10,10,0,0q�$
 Wrap=�$	��Top=�$	��517�� �� AcceptsReturn .  �� MaxLines$ 15��$
 8000���� -   $ 	btnUpload��$��Upload��$
��Left=�$��320,385,0,0q�$	��Top=�$	��240��$��32���� -   $ btnClose��$��Close��$
��Left=�$��
15,385,0,0q�$	��Top=�$	��114��$��32���� $
��Left=�$��
15,132,0,0q�$
��Wrap=�$��	Progress:��$	��Top=�7  1System.Windows.Forms.Integration.WindowsFormsHost  -     Name$ winHost��$
��Left=�$��17��$��159,400,0,0q�$	��Top=�$��21��$y�Hidden=�