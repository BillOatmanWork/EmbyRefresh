cd C:\Stuff\Emby Stuff\EmbyRefresh\EmbyRefresh\Test

copy /Y "C:\Stuff\Repos\EmbyRefresh\bin\Release\net6.0\win-x64\publish\EmbyRefresh.exe" .
"C:\Program Files\7-Zip\7z" a -tzip EmbyRefresh-WIN.zip EmbyRefresh.exe

copy /Y "C:\Stuff\Repos\EmbyRefresh\bin\Release\net6.0\osx.10.14-x64\publish\EmbyRefresh" .
"C:\Program Files\7-Zip\7z" a -t7z EmbyRefresh-OSX.7z EmbyRefresh

copy /Y "C:\Stuff\Repos\EmbyRefresh\bin\Release\net6.0\ubuntu.18.04-x64\publish\EmbyRefresh" .
"C:\Program Files\7-Zip\7z" a -t7z EmbyRefresh-UBU.7z EmbyRefresh

copy /Y "C:\Stuff\Repos\EmbyRefresh\bin\Release\net6.0\linux-x64\publish\EmbyRefresh" .
"C:\Program Files\7-Zip\7z" a -t7z EmbyRefresh-LIN64.7z EmbyRefresh

copy /Y "C:\Stuff\Repos\EmbyRefresh\bin\Release\net6.0\debian.12-x64\publish\EmbyRefresh" .
"C:\Program Files\7-Zip\7z" a -t7z EmbyRefresh-DEB12.7z EmbyRefresh