   M S B A M L   `   `   `  ����  IFEO_Guard_UI    IFEO_Guard_UI.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  IFEO Guard Manager��5   	   $	��600��6$   $	��800��61    � Icon$ /rainbow.ico��6=   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5	      $
��Auto �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   .+�5      �� -   5         ��Name$ btnStart��5      $��启动服务��6'   $��80��66   $��5q�6A   6L   �� 5      -   5        $ btnStop��5      $��停止服务��6&   $��80��65   $��5q�6@   6K   5      �� 5   
   -   5         ��Name$ txtLog��5      $��1��6"   $��5q�6/    �� 
IsReadOnly . " �� VerticalScrollBarVisibility$
 Auto=�5   $    �� TextWrapping$
 Wrap=�5      �� 5   
   $��2��6   $ 
Horizontal=�6"   $��Right=�6;   .+�5      �� -   5        $ btnClearLog��5      $��清空日志��6*   $��80��69   $��5q�6D   6O   �� 5      -   5        $ btnManageWhitelist��5      $��白名单管理��61   $��80��6A   $��5q�6L   6W   5      5      5      