   M S B A M L   `   `   `  ����  	SrcChess2    SrcChess2.frmPgnFilter   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Select Filter Criterias��5   	   $	��615��6)   $	��954��66    � WindowStartupLocation$ CenterOwner=�6B   .��5      � .+�5   
   ]� -   5        $��
dockPanel1��5   =   $��Stretch=�6   $��0q�62   $��Stretch=�6O   .+�5      �� -   5        $��	labelDesc��5   6   $��Description��6   $��28��6*   $	��Top=�6G   $	��Top=�6_   $��Center=�6t   �� 5      -   5        $��stackPanel1��5      $��Bottom=�6,   $��Bottom=�6D    �� Orientation$ 
Horizontal=�6_   $��Center=�6x   .+�5	      �� -   5        $��butOk��5	   2   $��Ok��6   $��23��6&   $��75��6?   $��10q�6J    �� 	IsDefault . 6g   �� 5
      -   5        $��butTest��5
   4   $
��Test��6   $��23��6(   $��75��6C   $��10q�6N   6Z   �� 5      -   5        $��	butCancel��5   6   $��Cancel��6   $��23��6*   $��75��6G   $��10q�6R    �� IsCancel . 6^   5      A� 5       A� Rows$ 1��6    A� Columns$ 3��6#   .+�5      �� -   5        $��	groupBox1��5   /   $��
ELO Ranges��6   .��5      ]� .+�5      �� $	��Top=�6%   $ 
Horizontal=�6:   .+�5      �� -   5        $��checkBoxAllRanges��5   H   $��
All Ranges��6'   $��16��6<   $��10q�6a   6�   �� 5      -	   5        $��checkBoxIncludeUnrated��5   N   $��Includes Unrated��6'   $��16��6B   $��10q�6l   5      �� 5      $��Bottom=�6%   .+�5      �� -
   5        $��butSelectAllRange��5   F   $��
Select All��6%   $��23��6:   $
��10,5q�6_   6m   �� 5      -   5        $��butClearAllRange��5   E   $��	Clear All��6%   $��23��69   $
��10,5q�6]   6k   5      �� 5      -   5        $��listBoxRange��5   "   5      5      �� 5      -   5        $��	groupBox2��5   ,   $��Players��6   .��5      ]� .+�5      �� $	��Top=�6%   $ 
Horizontal=�6:   .+�5      �� -   5        $��checkBoxAllPlayer��5   I   $��All Players��6'   $��16��6=   $��10q�6b   6�   5      �� 5       $��Bottom=�6%   .+�5!      �� -   5        $��butSelectAllPlayers��5!   F   $��
Select All��6%   $��23��6:   $
��10,5q�6a   6o   �� 5"      -   5        $��butClearAllPlayers��5"   E   $��	Clear All��6%   $��23��69   $
��10,5q�6_   6m   5#      �� 5$      -   5        $��listBoxPlayer��5$   "   5%      5&      �� 5'      -   5        $��	groupBox3��5'   +   $��Ending��6   .��5(      ]� .+�5)      �� $	��Top=�6%   $ 
Horizontal=�6:   .+�5*      �� -   5        $��checkBoxAllEndGame��5*   K   $��All End Games��6'   $��16��6?   $��10q�6e   6�   5+      �� 5,      $��Bottom=�6%   .+�5-      �� -   5        $��butSelectAllEndGame��5-   F   $��
Select All��6%   $��23��6:   $
��10,5q�6a   6o   �� 5.      -   5        $��butClearAllEndGame��5.   E   $��	Clear All��6%   $��23��69   $
��10,5q�6_   6m   5/      �� 50      -   5        $��listBoxEnding��50   "   51      52      53      54      55      56      