   M S B A M L   `   `   `  ����Q  MInteractiveNoticeboard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:NRCNRC  5      D @TLABS.WPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullN5clr-namespace:TLABS.WPF.Converters;assembly=TLABS.WPFTLABS.WPF.Converters 5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   RowDetailsToggleButtonStyle  �� 
TargetType �� Property �� Value �� 
TargetType ��Name � ToolTip � Cursor �� StrokeStartLineCap �� StrokeEndLineCap	 �� StrokeLineJoin
 �� Stretch �� RelativeSource �� AncestorType)   #System.Windows.Controls.DataGridRow �� Path �� 	Converter9  3TLABS.WPF.Converters.DataConverter_boolToVisibility   FalseToVisibility �� Triggers P� Property P� Value   DataGridRowDetailsToggleCell*  $System.Windows.Controls.DataGridCell d� 	IsChecked �� Mode9  3TLABS.WPF.Converters.DataConverter_VisibilityToBool   FalseToVisibility�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      NRCclr-namespace:NRC  5      NTLABS_WPF_Converters5clr-namespace:TLABS.WPF.Converters;assembly=TLABS.WPF  5      %
  &	        &	 �    7   �� 5      "  d�60   .�5   
   ��  �@�FocusVisualStyle 6-   Z� �� 5	   
    � �� 6'   Z� �� 5
   
    � �� 6(   Z� �� 5   
    � �� 6'   Z� �� 5   
    � ��$ Center=�67   �� 5   
    � ��$ Center=�65   �� 5   
    � �� 5      �� 5      " d�6"   .R�5      � .+�5      �� $ PlusLogo��6!   $ Show details��63   
������ $
 Hand��6d   ._�5      �� $��13��6%   $��13��60   $��Center=�6<   .+�5   "   F� ����    ��[ � � ��[ 	$��3��6�   $ Round=�6�   $ Round=�6�   $	 Round=�6�   $��13��6�   $��13��6  $
 Uniform=�6  ��68   �� *
Foreground+ �� "   6#  F� 5   "   ���Ѐ�[     р�[  � 	$��3��6�   $ Round=�6�   $ Round=�6�   $	 Round=�6�   $��13��6�   $��13��6  $
 Uniform=�6  ��68   �� *
Foreground+ �� "   6#  5      y�5      �� 5   "   $ 	IsChecked�6*    6;   �� *TemplatedParent+ 5   &     5   *   $ Visible=�6^   5   #   5       �� 5"      $ 	MinusLogo��6!   $ Hide details��64   
������ $
 Hand��6d   ._�5$      �� $��13��6%   $��13��60   $��Center=�6<   .+�5%   "   F� ����    ��[ � � ��[ 	$��3��6�   $ Round=�6�   $ Round=�6�   $	 Round=�6�   $��13��6�   $��13��6  $
 Uniform=�6  ��68   �� *
Foreground+ �� "   6#  5&      y�5(      �� 5)   "   $ 	IsChecked�6*    6;   �� *TemplatedParent+ 5*   &     5+   *   $ Hidden=�6^   5-   #   5/      50       52      P� 54       � {� .  . �55      ��  6;   Z�  � ��56      58      5:      5;      �� 5=      "   61   .�5>   
   ��  � ��$ 0q�6$   60   �� 5?   
    � �� 5@      �� 5A      .R�5B      d� 8��  $��12��6X   $��12��6c    5C      �� 5D      $ DetailsVisibility�6j   $ TwoWay=�6�    6&   �� "    5E   "     5F   &   $ 	Collapsed=�6Z   5H      5J      5K      5M      5N      5P      