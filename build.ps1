$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
#set "path=%path%;%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin"
$csproj = "$root\src\TrayToolbar\TrayToolbar.csproj"
$version = ([xml](Get-Content $csproj)).Project.PropertyGroup.Version

dotnet publish -a arm64 -p:RuntimeIdentifier=win-arm64 $csproj -o ${root}\publish -c Release -f net8.0-windows -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:SelfContained=false -p:PublishProtocol=FileSystem
Compress-Archive "$root\publish\*.exe" "$root\TrayToolbar-win-arm64-portable-$version.zip" -Force

dotnet publish -a x64 -p:RuntimeIdentifier=win-x64 $csproj -o ${root}\publish -c Release -f net8.0-windows -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:SelfContained=false -p:PublishProtocol=FileSystem
Compress-Archive "$root\publish\*.exe" "$root\TrayToolbar-win-x64-portable-$version.zip" -Force
