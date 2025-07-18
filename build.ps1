$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
#set "path=%path%;%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin"
$csproj = "$root\src\TrayToolbar\TrayToolbar.csproj"
$version = ([xml](Get-Content $csproj)).Project.PropertyGroup.Version

msbuild /t:restore $csproj

msbuild /t:Publish -p:RuntimeIdentifier=win-arm64 $csproj /p:PublishUrl=${root}\publish /p:Configuration=Release -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:SelfContained=false -p:PublishProtocol=FileSystem
Compress-Archive "$root\publish\*.exe" "$root\TrayToolbar-win-arm64-portable-$version.zip" -Force

msbuild /t:Publish -p:RuntimeIdentifier=win-x64 $csproj /p:PublishUrl=${root}\publish /p:Configuration=Release -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:SelfContained=false -p:PublishProtocol=FileSystem
Compress-Archive "$root\publish\*.exe" "$root\TrayToolbar-win-x64-portable-$version.zip" -Force
