   M S B A M L   `   `   `  ����Q  MInteractiveNoticeboard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:InteractiveNoticeboardInteractiveNoticeboard  5   	   '    !InteractiveNoticeboard.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   3IN$clr-namespace:InteractiveNoticeboard     5   	   -   5          � Title$*  $Interactive Noticeboard | CSTE, NSTU��5   	   $	��800��5   	   $
��1200��6    � 
ResizeMode$ NoResize=�6#    � WindowStartupLocation$ CenterScreen=�5   	    � WindowState$ 	Maximized=�6.    � WindowStyle$
 None=�6F    � Topmost .  � 	Resources 5         WindowButtonStyle �� 
TargetType �� Property	 �� Value
 �� Triggers P� Property P� Value�� 5   
   %.  &	        �� 5      " ��6.   .�5      ��  � ��	 �	#B60000006C   �� 5       �@�Cursor$
	 Hand��6+   6:   �� 5       �@�ToolTip$	 Close��6,   6<   
 5      P� 5       �@J�IsMouseOver . . �5      ��  � ��	 �	#D60000006K   5      5      5      .��5      � -   5        $��
LayoutGrid��5      .+�5   
   r� -   5        $��MediaPlayer��5      $y�	Collapsed=�6*   $	��120��6A   $��
0,-120,0,0q�6N    r� LoadedBehavior$ Manual=�6b   $��Center=�6z   $	��Top=�6�   �� 5   
   ���	#FF262626._�5      � .+�5       �� $��Center=�6   $��Center=�69    �� TextAlignment$ Center=�6T   �� 5!      ���	#FF464646$��50(�61   $!��Glacial Indifference, Arial)�6?   $$�Interactive Noticeboard System��6h   6�   �� 5"      6"   �� 5#      ���	#FF363636$	��250(�61   $��Rockwell, Arial)�6@   $
��Bold"�6]   $
�CSTE��6o   6}   5$      �� 5&      $��Center=�6   $��Bottom=�69   $ Center=�6T   $��0,0,0,10q�6k   �� 5'      ���	#FF464646$��14(�61   $��Consolas, Arial)�6?   $2�,By the CSTE • Of the CSTE • For the CSTE��5(      6L   5)      5*      5+      � 5,   
   -   5        $��ContainerPanel��5,      54      � 56   
   -   5        $��NotificationPanel��56      6*   � 58   
   -   5        $��WindowButtonPanel��58      $��30��6(   $	��100��64   $	��Top=�6@   $��Right=�6X   
������ .+�59      ��  �� Orientation$ 
Horizontal=�6   $��Right=�62   $��Center=�6N   $��0,0,10,0q�6i   y�6{   �� *IsMouseOver+ �� ElementName$ WindowButtonPanel��  B2V �� 	Converter# [ .+�5:      �� -   5        $��btnMinimize��5:      #��[  6,   $��20��6W   $��20��6b    �� CornerRadius$ 3��6n    � ToolTip$ Mimimize��6   ._�5;      F� ����     z �@T�  z 	
�������$��2��6:    �� StrokeStartLineCap$ Round=�6N    �� StrokeEndLineCap$ Round=�6i    �� StrokeLineJoin$ Round=�6�   $��10��6�   $��10��6�   $��Center=�6�   6�   5<      �� 5>      -   5        $��btnClose��5>      #��[  6)   $��20��6T   $��20��6_   $��5,0,0,0q�6k   $ 3��6|   $ Close��6�   ._�5?      F� 4����        р�� ��� �@KL @KL р��     �    ��� 	
�������$��2��6K   $ Round=�6_   $ Round=�6z   $ Round=�6�    �� Stretch$
 Fill=�6�   $��10��6�   $��10��6�   $��Center=�6�   6�   5@      5A      5B      5D      5E      