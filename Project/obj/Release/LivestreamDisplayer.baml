   M S B A M L   `   `   `  ����   CELO'    !CELO_Enhanced.LivestreamDisplayer   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     �Name$  DisplayerWindow�� � Title$! CELO - Livestream Displayer��$	��403��$	��849�� � 
ResizeMode$ CanMinimize=�$��Disabled=� � SizeToContent$ WidthAndHeight=�&   System.Windows.Media.TextOptions   TextFormattingMode$ Display=� � WindowStartupLocation$ CenterScreen=� � Icon$ icon-modified.ico��$	��857��$	��413��.��� .+��� ���	#FFB2C8D4$��1q�$
��Left=�$	��279��$��
612,82,0,0q�$	��Top=�$	��219��._�� .+��� $
��Left=�$
��Wrap=�$����This way you can put players information into your Livestream. In order to add them using OBS (Open Broadcaster Software), you have a add a TEXT Source in your Scene, and chose the player files.��$	��Top=�$	��148��$	��200��$��
10,125,0,0q� �� TextAlignment$ Center=�$��SemiBold"�$��13(��� $
��Left=�$��	10,10,0,0q�$
��Wrap=�$����The Livestream Displayer creates a file for each player with the specified variables in any order or layout when a game is about to start.��$	��Top=�$��13(�$ Center=�$	��104���� ���	#FFB2C8D4$��1q�$
��Left=�$��64��$��
612,13,0,0q�$	��Top=�$	��219���� -    ��Name$ playerListBox��$
��Left=�$	��212��$��8,12,0,0q�$	��Top=�$	��191��$��Center=�$��15(�	 �� SelectedIndex$	 0��$��Disabled=�$��Disabled=�$��SemiBold"�$��Center=�.@��� $��Player 1 (You)���� $��Player 2���� $��Player 3���� $��Player 4���� $��Player 5���� $��Player 6���� $��Player 7���� $��Player 8���� $
��File��$
��Left=�$��
208,42,0,0q�$	��Top=�$	��171��$	��392��.��� $��0,0,0,-1q�.+��� -   
 ��Name$
 tBox_FileContent��$
��Left=�$	��130��$��	10,10,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��358��.������  �� EndPoint$
 0,20&� � MappingMode$ Absolute=� �� 
StartPoint$	 0,0&�.���  � Color$ 	#FFABADB3�� � Offset$
 0.05Q�� $ 	#FFE2E3EA��$
 0.07Q�� $ 	#FFC5CFD8��$ 1Q�.���� $
��Left=�$��46��$��
694,22,0,0q�$	��Top=�$��86��$��Resources/vidcap.png���� $��	Variables��$
��Left=�$��208,218,0,0q�$	��Top=�$	��146��$	��392��.��� .+��� $��Nickname - ��$
��Left=�$��21,6,0,0q�$	��Top=�$��73���� $��Ladder Rank - ��$
��Left=�$��	10,31,0,0q�$	��Top=�$��84���� $��Hours Played - ��$
��Left=�$��	190,6,0,0q�$	��Top=��� $��Game Level - ��$
��Left=�$��
200,31,0,0q�$	��Top=�$��81���� $��Steam ID - ��$
��Left=�$��	28,56,0,0q�$	��Top=�$��66���� $��%NICK%��$
��Left=�$��94,6,0,0q�$	��Top=�$
��Bold"��� $��%RANK%��$
��Left=�$��	94,31,0,0q�$	��Top=�$
��Bold"��� $��	%STEAMID%��$
��Left=�$��	94,56,0,0q�$	��Top=�$
��Bold"��� $��%HOURS%��$
��Left=�$��	281,6,0,0q�$	��Top=�$
��Bold"��� $��%LEVEL%��$
��Left=�$��
281,31,0,0q�$	��Top=�$
��Bold"��� $��
Faction - ��$
��Left=�$��
223,56,0,0q�$	��Top=�$��58���� $��	%FACTION%��$
��Left=�$��
281,56,0,0q�$	��Top=�$
��Bold"�$��80���� $��Team - ��$
��Left=�$��	47,81,0,0q�$	��Top=�$��47���� $��%TEAM%��$
��Left=�$��	94,82,0,0q�$	��Top=�$
��Bold"��� -    ��Name$ btnCreateAll��$��Save all files��$
��Left=�$��
10,229,0,0q�$	��Top=�$	��189��$��31���� -   $ btnCreateSelected��$��Save for selected player��$
��Left=�$��
10,265,0,0q�$	��Top=�$	��189��$��31���� -   $ btnClose��$��Close��$
��Left=�$��
10,330,0,0q�$	��Top=�$	��189��$��31���� -   $
 	tBox_path��$
��Left=�$��24��$��
295,13,0,0q�$	��Top=�$	��227��{�.  �� -   $ 	btnBrowse��$��Browse��$
��Left=�$��
527,13,0,0q�$	��Top=�$��73�� J� RenderTransformOrigin$ 0.479,1&�$��24���� $��Output Folder:��$
��Left=�$��
208,12,0,0q�$	��Top=�