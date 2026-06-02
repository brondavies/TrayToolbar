[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$SignPathEnabled,

    [Parameter(Mandatory)]
    [string]$Arm64Asset,

    [Parameter(Mandatory)]
    [string]$X64Asset,

    [string]$RepositoryRoot = (Get-Location).Path,

    [string]$GitHubOutputPath = $env:GITHUB_OUTPUT
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($GitHubOutputPath))
{
    throw 'GITHUB_OUTPUT is not set. Pass -GitHubOutputPath explicitly when invoking this script outside GitHub Actions.'
}

$resolvedRepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).ProviderPath

function Convert-ToRelativeArtifactPath
{
    param(
        [Parameter(Mandatory)]
        [string]$RelativeDirectory,

        [Parameter(Mandatory)]
        [string]$AbsoluteDirectory,

        [Parameter(Mandatory)]
        [string]$AbsoluteFilePath
    )

    if (-not $AbsoluteFilePath.StartsWith($AbsoluteDirectory, [System.StringComparison]::OrdinalIgnoreCase))
    {
        throw "Cannot resolve relative artifact path because '$AbsoluteFilePath' is not under '$AbsoluteDirectory'."
    }

    $relativeChildPath = $AbsoluteFilePath.Substring($AbsoluteDirectory.Length).TrimStart('\', '/') -replace '\\', '/'
    if ([string]::IsNullOrWhiteSpace($relativeChildPath))
    {
        return $RelativeDirectory
    }

    return "$RelativeDirectory/$relativeChildPath"
}

function Resolve-FinalAssetPath
{
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory)]
        [string]$RelativeDirectory,

        [Parameter(Mandatory)]
        [string]$ExpectedFileName
    )

    $expectedRelativePath = "$RelativeDirectory/$ExpectedFileName"
    $absoluteDirectory = Join-Path $RepositoryRoot $RelativeDirectory
    $expectedAbsolutePath = Join-Path $absoluteDirectory $ExpectedFileName

    if (Test-Path -LiteralPath $expectedAbsolutePath -PathType Leaf)
    {
        return $expectedRelativePath
    }

    if (-not (Test-Path -LiteralPath $absoluteDirectory -PathType Container))
    {
        throw "Expected signed artifact directory not found: $RelativeDirectory"
    }

    $discoveredFiles = @(Get-ChildItem -LiteralPath $absoluteDirectory -File -Recurse)
    $discoveredZipFiles = @($discoveredFiles | Where-Object Extension -eq '.zip')

    if ($discoveredZipFiles.Count -eq 1)
    {
        $resolvedRelativePath = Convert-ToRelativeArtifactPath -RelativeDirectory $RelativeDirectory -AbsoluteDirectory $absoluteDirectory -AbsoluteFilePath $discoveredZipFiles[0].FullName
        Write-Warning "Expected signed asset '$expectedRelativePath' was not found. Using discovered ZIP artifact '$resolvedRelativePath' instead."
        return $resolvedRelativePath
    }

    if ($discoveredFiles.Count -gt 0 -and $discoveredZipFiles.Count -eq 0)
    {
        $temporaryRoot = if (-not [string]::IsNullOrWhiteSpace($env:RUNNER_TEMP))
        {
            $env:RUNNER_TEMP
        }
        else
        {
            [System.IO.Path]::GetTempPath()
        }

        $tempArchivePath = Join-Path $temporaryRoot ("repacked-$ExpectedFileName")
        if (Test-Path -LiteralPath $tempArchivePath)
        {
            Remove-Item -LiteralPath $tempArchivePath -Force
        }

        Compress-Archive -Path (Join-Path $absoluteDirectory '*') -DestinationPath $tempArchivePath -Force
        Move-Item -LiteralPath $tempArchivePath -Destination $expectedAbsolutePath -Force

        Write-Warning "Expected signed ZIP '$expectedRelativePath' was not found. Repacked files from '$RelativeDirectory' into '$expectedRelativePath'."
        return $expectedRelativePath
    }

    $discoveredRelativePaths = if ($discoveredFiles.Count -gt 0)
    {
        $discoveredFiles | ForEach-Object {
            Convert-ToRelativeArtifactPath -RelativeDirectory $RelativeDirectory -AbsoluteDirectory $absoluteDirectory -AbsoluteFilePath $_.FullName
        }
    }
    else
    {
        @('<none>')
    }

    throw "Expected final asset not found: $expectedRelativePath. Discovered files under ${RelativeDirectory}: $($discoveredRelativePaths -join ', ')"
}

$arm64RelativePath = if ($SignPathEnabled -eq 'true')
{
    Resolve-FinalAssetPath -RepositoryRoot $resolvedRepositoryRoot -RelativeDirectory 'signpath/arm64' -ExpectedFileName $Arm64Asset
}
else
{
    $Arm64Asset
}

$x64RelativePath = if ($SignPathEnabled -eq 'true')
{
    Resolve-FinalAssetPath -RepositoryRoot $resolvedRepositoryRoot -RelativeDirectory 'signpath/x64' -ExpectedFileName $X64Asset
}
else
{
    $X64Asset
}

foreach ($relativePath in @($arm64RelativePath, $x64RelativePath))
{
    if (-not (Test-Path -LiteralPath (Join-Path $resolvedRepositoryRoot $relativePath) -PathType Leaf))
    {
        throw "Expected final asset not found: $relativePath"
    }
}

Add-Content -LiteralPath $GitHubOutputPath -Value "arm64_path=$arm64RelativePath"
Add-Content -LiteralPath $GitHubOutputPath -Value "x64_path=$x64RelativePath"

Write-Host "Resolved arm64 asset path: $arm64RelativePath"
Write-Host "Resolved x64 asset path: $x64RelativePath"
