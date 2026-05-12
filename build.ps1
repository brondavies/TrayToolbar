$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$csproj = "$root\src\TrayToolbar\TrayToolbar.csproj"
$version = ([xml](Get-Content $csproj)).Project.PropertyGroup.Version
$publishDir = Join-Path $root "publish"

$msbuildCommand = Get-Command msbuild -ErrorAction SilentlyContinue
$dotnetCommand = Get-Command dotnet -ErrorAction Stop

if ($msbuildCommand)
{
	$buildTool = $msbuildCommand.Source
	$buildToolPrefix = @()
}
else
{
	$buildTool = $dotnetCommand.Source
	$buildToolPrefix = @("msbuild")
}

function Invoke-BuildTool
{
	param(
		[Parameter(ValueFromRemainingArguments = $true)]
		[string[]] $Arguments
	)

	& $buildTool @buildToolPrefix @Arguments
}

function Reset-PublishDirectory
{
	if (Test-Path $publishDir)
	{
		Remove-Item "$publishDir\*" -Recurse -Force -ErrorAction SilentlyContinue
	}
	else
	{
		New-Item -ItemType Directory -Path $publishDir | Out-Null
	}
}

Invoke-BuildTool @('-t:restore', $csproj)

Reset-PublishDirectory
Invoke-BuildTool @(
	'-t:Publish',
	'-p:RuntimeIdentifier=win-arm64',
	$csproj,
	"-p:PublishDir=$publishDir",
	'-p:Configuration=Release',
	'-p:PublishSingleFile=true',
	'-p:PublishReadyToRun=false',
	'-p:SelfContained=false',
	'-p:PublishProtocol=FileSystem'
)
Compress-Archive "$root\publish\*.exe" "$root\TrayToolbar-win-arm64-portable-$version.zip" -Force

Reset-PublishDirectory
Invoke-BuildTool @(
	'-t:Publish',
	'-p:RuntimeIdentifier=win-x64',
	$csproj,
	"-p:PublishDir=$publishDir",
	'-p:Configuration=Release',
	'-p:PublishSingleFile=true',
	'-p:PublishReadyToRun=false',
	'-p:SelfContained=false',
	'-p:PublishProtocol=FileSystem'
)
Compress-Archive "$root\publish\*.exe" "$root\TrayToolbar-win-x64-portable-$version.zip" -Force
