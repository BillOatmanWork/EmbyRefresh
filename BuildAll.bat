dotnet publish -r win-x64 -c Release -p:PublishReadyToRun=true -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-arm -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-arm64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained

cd C:\repos\EmbyRefresh\Build

copy /Y "C:\repos\EmbyRefresh\bin\Release\net8.0\win-x64\publish\EmbyRefresh.exe" .
"C:\Program Files\7-Zip\7z" a -tzip EmbyRefresh-WIN.zip EmbyRefresh.exe

copy /Y "C:\repos\EmbyRefresh\bin\Release\net8.0\osx-x64\publish\EmbyRefresh" .
"C:\Program Files\7-Zip\7z" a -t7z EmbyRefresh-OSX.7z EmbyRefresh

copy /Y "C:\repos\EmbyRefresh\bin\Release\net8.0\linux-x64\publish\EmbyRefresh" .
"C:\Program Files\7-Zip\7z" a -t7z EmbyRefresh-LIN64.7z EmbyRefresh

copy /Y "C:\repos\EmbyRefresh\bin\Release\net8.0\linux-arm\publish\EmbyRefresh" .
"C:\Program Files\7-Zip\7z" a -t7z EmbyRefresh-RasPi.7z EmbyRefresh

copy /Y "C:\repos\EmbyRefresh\bin\Release\net8.0\linux-arm64\publish\EmbyRefresh" .
"C:\Program Files\7-Zip\7z" a -t7z EmbyRefresh-RasPi64.7z EmbyRefresh

