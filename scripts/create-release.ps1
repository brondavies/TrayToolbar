<#
.SYNOPSIS
    Cuts a TrayToolbar release: version bump, changelog roll, pull request, merge,
    and the signed publish run that creates the tag.

.DESCRIPTION
    Runs the whole release process from a local checkout:

      1. Validates the working tree, the requested version, and gh authentication.
      2. Updates <Version> in TrayToolbar.csproj and assemblyIdentity in app.manifest.
      3. Rolls CHANGELOG.md [Unreleased] into the new version and updates the compare
         links, and retitles the leading section of docs/release-notes.md.
      4. Commits to a release branch, opens a pull request, and merges it to master.
      5. Cancels the master push validation run, which would otherwise ask for a second
         pair of SignPath approvals for the same commit.
      6. Dispatches the workflow with publish set, which signs the portable zips,
         creates tag v<Version>, and publishes the GitHub prerelease.

    The tag is created by the release run rather than pushed from here. SignPath rejects
    signing requests submitted from a tag ref with 400 Bad Request, so the release path
    never builds from a tag. See CONTRIBUTING.md for the trigger table.

    The publish run pauses at each signing step until the request is approved in the
    SignPath portal. Expect two approvals, arm64 then x64.

.PARAMETER Version
    Release version as MAJOR.MINOR.PATCH, without the leading v.

.PARAMETER Force
    Skips the confirmation prompt.

.PARAMETER KeepValidationRun
    Leaves the master push validation run alone. It signs the same commit again, so it
    costs an extra pair of SignPath approvals.

.PARAMETER NoWait
    Returns once the publish run is dispatched instead of waiting for it to finish.

.EXAMPLE
    .\scripts\create-release.ps1 1.8.2

.EXAMPLE
    .\scripts\create-release.ps1 -Version 1.9.0 -Force
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version,

    [switch]$Force,

    [switch]$KeepValidationRun,

    [switch]$NoWait
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepositoryRoot = Split-Path -Parent $PSScriptRoot
$ProjectPath = Join-Path $RepositoryRoot 'src/TrayToolbar/TrayToolbar.csproj'
$ManifestPath = Join-Path $RepositoryRoot 'src/TrayToolbar/app.manifest'
$ChangelogPath = Join-Path $RepositoryRoot 'CHANGELOG.md'
$ReleaseNotesPath = Join-Path $RepositoryRoot 'docs/release-notes.md'
$WorkflowFile = 'dotnet-desktop.yml'
$WorkflowName = 'Portable Release'
$BaseBranch = 'master'

function Write-Step
{
    param([Parameter(Mandatory)][string]$Message)
    Write-Host ''
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Invoke-Native
{
    param(
        [Parameter(Mandatory)][string]$Executable,
        [Parameter(Mandatory)][string[]]$Arguments,
        [string]$ErrorMessage
    )

    $output = & $Executable @Arguments 2>&1
    if ($LASTEXITCODE -ne 0)
    {
        $joined = ($output | Out-String).Trim()
        $message = if ([string]::IsNullOrWhiteSpace($ErrorMessage))
        {
            "$Executable $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
        }
        else
        {
            $ErrorMessage
        }

        throw "$message`n$joined"
    }

    return ($output | Out-String).TrimEnd()
}

function Invoke-Git
{
    param([Parameter(Mandatory, ValueFromRemainingArguments)][string[]]$Arguments)
    return Invoke-Native -Executable 'git' -Arguments (@('-C', $RepositoryRoot) + $Arguments)
}

function Invoke-Gh
{
    param([Parameter(Mandatory, ValueFromRemainingArguments)][string[]]$Arguments)
    return Invoke-Native -Executable 'gh' -Arguments $Arguments
}

function Get-FileText
{
    param([Parameter(Mandatory)][string]$Path)
    return [System.IO.File]::ReadAllText($Path)
}

function Set-FileText
{
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][AllowEmptyString()][string]$Content
    )

    # Preserve whether the existing file carries a UTF-8 byte order mark.
    $preamble = New-Object byte[] 3
    $stream = [System.IO.File]::OpenRead($Path)
    try
    {
        $read = $stream.Read($preamble, 0, 3)
    }
    finally
    {
        $stream.Dispose()
    }

    $hasBom = $read -eq 3 -and $preamble[0] -eq 0xEF -and $preamble[1] -eq 0xBB -and $preamble[2] -eq 0xBF
    $encoding = New-Object System.Text.UTF8Encoding($hasBom)
    [System.IO.File]::WriteAllText($Path, $Content, $encoding)
}

function Assert-Tooling
{
    foreach ($tool in @('git', 'gh'))
    {
        if (-not (Get-Command $tool -ErrorAction SilentlyContinue))
        {
            throw "$tool was not found on PATH."
        }
    }

    try
    {
        Invoke-Gh 'auth' 'status' | Out-Null
    }
    catch
    {
        throw "gh is not authenticated. Run 'gh auth login' and try again.`n$($_.Exception.Message)"
    }
}

function Assert-CleanCheckout
{
    $status = Invoke-Git 'status' '--porcelain' '--untracked-files=no'
    if (-not [string]::IsNullOrWhiteSpace($status))
    {
        throw "The working tree has uncommitted changes:`n$status"
    }

    $branch = Invoke-Git 'rev-parse' '--abbrev-ref' 'HEAD'
    if ($branch -ne $BaseBranch)
    {
        throw "Expected to be on '$BaseBranch' but the current branch is '$branch'."
    }

    Invoke-Git 'fetch' 'origin' '--tags' | Out-Null

    $local = Invoke-Git 'rev-parse' 'HEAD'
    $remote = Invoke-Git 'rev-parse' "origin/$BaseBranch"
    if ($local -ne $remote)
    {
        throw "Local '$BaseBranch' ($($local.Substring(0, 8))) does not match origin/$BaseBranch ($($remote.Substring(0, 8))). Pull or push first."
    }
}

function Assert-VersionAvailable
{
    param([Parameter(Mandatory)][string]$NewVersion)

    $existingTag = Invoke-Git 'tag' '--list' "v$NewVersion"
    if (-not [string]::IsNullOrWhiteSpace($existingTag))
    {
        throw "Tag v$NewVersion already exists. Pick a version that has not been tagged."
    }

    $currentVersion = Get-ProjectVersion
    if ($currentVersion -eq $NewVersion)
    {
        throw "TrayToolbar.csproj already declares version $NewVersion."
    }

    return $currentVersion
}

function Get-ProjectVersion
{
    $text = Get-FileText -Path $ProjectPath
    $match = [regex]::Match($text, '<Version>(?<version>\d+\.\d+\.\d+)</Version>')
    if (-not $match.Success)
    {
        throw "Could not find <Version> in $ProjectPath."
    }

    return $match.Groups['version'].Value
}

function Update-ProjectVersion
{
    param([Parameter(Mandatory)][string]$NewVersion)

    $text = Get-FileText -Path $ProjectPath
    $updated = [regex]::Replace($text, '<Version>\d+\.\d+\.\d+</Version>', "<Version>$NewVersion</Version>", 1)
    Set-FileText -Path $ProjectPath -Content $updated
    Write-Host "  TrayToolbar.csproj  <Version>$NewVersion</Version>"
}

function Update-ManifestVersion
{
    param([Parameter(Mandatory)][string]$NewVersion)

    $text = Get-FileText -Path $ManifestPath
    $pattern = '(?<prefix><assemblyIdentity\s+version=")\d+\.\d+\.\d+\.\d+(?<suffix>")'
    if (-not [regex]::IsMatch($text, $pattern))
    {
        throw "Could not find assemblyIdentity version in $ManifestPath."
    }

    $updated = [regex]::Replace($text, $pattern, "`${prefix}$NewVersion.0`${suffix}", 1)
    Set-FileText -Path $ManifestPath -Content $updated
    Write-Host "  app.manifest        assemblyIdentity version=`"$NewVersion.0`""
}

function Update-Changelog
{
    param([Parameter(Mandatory)][string]$NewVersion)

    $text = Get-FileText -Path $ChangelogPath

    if ($text -match "(?m)^\#\#\s+\[$([regex]::Escape($NewVersion))\]")
    {
        throw "CHANGELOG.md already has a section for $NewVersion."
    }

    # Horizontal whitespace and at most one carriage return. In multiline mode \s would
    # also match the line breaks after the heading, and replacing the match would eat the
    # blank line below it. The \r is needed because $ matches before \n on CRLF files.
    $unreleasedHeading = [regex]::Match($text, '(?m)^\#\#[ \t]+\[Unreleased\][ \t]*\r?$')
    if (-not $unreleasedHeading.Success)
    {
        throw 'Could not find the "## [Unreleased]" heading in CHANGELOG.md.'
    }

    # Warn when nothing has accumulated, so an empty release section is a deliberate choice.
    $afterHeading = $text.Substring($unreleasedHeading.Index + $unreleasedHeading.Length)
    $nextHeading = [regex]::Match($afterHeading, '(?m)^\#\#\s')
    $body = if ($nextHeading.Success) { $afterHeading.Substring(0, $nextHeading.Index) } else { $afterHeading }
    if ([string]::IsNullOrWhiteSpace($body))
    {
        Write-Warning 'The [Unreleased] section is empty, so the new release section will have no entries.'
    }

    # Match the line endings already in the file rather than assuming the platform's.
    $newLine = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }

    $releaseDate = Get-Date -Format 'yyyy-MM-dd'
    $text = $text.Remove($unreleasedHeading.Index, $unreleasedHeading.Length).Insert(
        $unreleasedHeading.Index,
        "## [Unreleased]$newLine$newLine## [$NewVersion] - $releaseDate")

    # The Unreleased compare link names the previous released version.
    $unreleasedLink = [regex]::Match($text, '(?m)^\[Unreleased\]:[ \t]*(?<base>\S+?/compare/)v(?<previous>\d+\.\d+\.\d+)\.\.\.HEAD[ \t]*\r?$')
    if (-not $unreleasedLink.Success)
    {
        throw 'Could not find the [Unreleased] compare link in CHANGELOG.md.'
    }

    $compareBase = $unreleasedLink.Groups['base'].Value
    $previousVersion = $unreleasedLink.Groups['previous'].Value
    $replacement = "[Unreleased]: ${compareBase}v$NewVersion...HEAD$newLine[$NewVersion]: ${compareBase}v$previousVersion...v$NewVersion"
    $text = $text.Remove($unreleasedLink.Index, $unreleasedLink.Length).Insert($unreleasedLink.Index, $replacement)

    Set-FileText -Path $ChangelogPath -Content $text
    Write-Host "  CHANGELOG.md        [$NewVersion] - $releaseDate, compare from v$previousVersion"
}

function Update-ReleaseNotes
{
    param([Parameter(Mandatory)][string]$NewVersion)

    $text = Get-FileText -Path $ReleaseNotesPath
    $heading = [regex]::Match($text, '(?m)^\#\#\s+\d+\.\d+\.\d+.*$')
    if (-not $heading.Success)
    {
        throw "Could not find a version heading in $ReleaseNotesPath."
    }

    $text = $text.Remove($heading.Index, $heading.Length).Insert($heading.Index, "## $NewVersion")
    Set-FileText -Path $ReleaseNotesPath -Content $text
    Write-Host "  release-notes.md    ## $NewVersion"
}

function New-ReleasePullRequest
{
    param(
        [Parameter(Mandatory)][string]$NewVersion,
        [Parameter(Mandatory)][string]$Branch
    )

    Invoke-Git 'checkout' '-b' $Branch | Out-Null

    $subject = "Release $NewVersion"
    $bodyLines = @(
        '',
        'Bumps the project version and rolls the changelog and release notes.',
        '',
        'The tag is created by the publish run, not pushed from the release script.'
    )
    $message = $subject + [Environment]::NewLine + ($bodyLines -join [Environment]::NewLine)

    Invoke-Git 'commit' '--all' '--message' $message | Out-Null
    Invoke-Git 'push' '--set-upstream' 'origin' $Branch | Out-Null

    $prBodyPath = Join-Path ([System.IO.Path]::GetTempPath()) "traytoolbar-release-$NewVersion.md"
    $prBody = @(
        "Release $NewVersion.",
        '',
        '- Bump the version in `TrayToolbar.csproj` and `app.manifest`',
        '- Roll `CHANGELOG.md` and `docs/release-notes.md`',
        '',
        'The publish run signs the portable zips, creates tag `v' + $NewVersion + '`, and publishes the prerelease.'
    ) -join [Environment]::NewLine
    Set-Content -LiteralPath $prBodyPath -Value $prBody -Encoding UTF8

    try
    {
        $url = Invoke-Gh 'pr' 'create' '--base' $BaseBranch '--head' $Branch '--title' $subject '--body-file' $prBodyPath
    }
    finally
    {
        Remove-Item -LiteralPath $prBodyPath -Force -ErrorAction SilentlyContinue
    }

    Write-Host "  $url"
    return $url
}

function Merge-ReleasePullRequest
{
    param([Parameter(Mandatory)][string]$PullRequestUrl)

    try
    {
        Invoke-Gh 'pr' 'merge' $PullRequestUrl '--merge' '--admin' | Out-Null
    }
    catch
    {
        Write-Warning "Merging with --admin failed, retrying without it. $($_.Exception.Message)"
        Invoke-Gh 'pr' 'merge' $PullRequestUrl '--merge' | Out-Null
    }

    Invoke-Git 'checkout' $BaseBranch | Out-Null
    Invoke-Git 'pull' '--ff-only' | Out-Null

    $head = Invoke-Git 'rev-parse' 'HEAD'
    Write-Host "  $BaseBranch is now at $($head.Substring(0, 8))"
    return $head
}

function Get-WorkflowRuns
{
    param(
        [Parameter(Mandatory)][string]$Event,
        [int]$Limit = 10
    )

    $json = Invoke-Gh 'run' 'list' '--workflow' $WorkflowFile '--event' $Event '--limit' "$Limit" '--json' 'databaseId,headSha,status,conclusion,createdAt,name'
    if ([string]::IsNullOrWhiteSpace($json))
    {
        return @()
    }

    return @($json | ConvertFrom-Json | Where-Object { $_.name -eq $WorkflowName })
}

function Stop-ValidationRun
{
    param([Parameter(Mandatory)][string]$HeadSha)

    # The push run signs the same commit the publish run will sign, which means a second
    # pair of SignPath approvals for artifacts that are never published.
    $deadline = (Get-Date).AddSeconds(90)
    while ((Get-Date) -lt $deadline)
    {
        $run = Get-WorkflowRuns -Event 'push' |
            Where-Object { $_.headSha -eq $HeadSha -and $_.status -ne 'completed' } |
            Select-Object -First 1

        if ($run)
        {
            Invoke-Gh 'run' 'cancel' "$($run.databaseId)" | Out-Null
            Write-Host "  cancelled validation run $($run.databaseId)"
            return
        }

        Start-Sleep -Seconds 5
    }

    Write-Warning 'No master push validation run appeared to cancel. If one starts, it will ask for its own SignPath approvals.'
}

function Start-PublishRun
{
    $before = @(Get-WorkflowRuns -Event 'workflow_dispatch' | ForEach-Object { $_.databaseId })
    Invoke-Gh 'workflow' 'run' $WorkflowFile '--ref' $BaseBranch '-f' 'publish=true' | Out-Null

    $deadline = (Get-Date).AddSeconds(120)
    while ((Get-Date) -lt $deadline)
    {
        Start-Sleep -Seconds 5
        $run = Get-WorkflowRuns -Event 'workflow_dispatch' |
            Where-Object { $before -notcontains $_.databaseId } |
            Select-Object -First 1

        if ($run)
        {
            return $run.databaseId
        }
    }

    throw 'The publish run was dispatched but did not appear in the run list.'
}

function Wait-ForRun
{
    param([Parameter(Mandatory)][string]$RunId)

    Write-Host '  Waiting. Approve the arm64 and x64 signing requests in the SignPath portal.'
    $lastStep = ''

    while ($true)
    {
        $json = Invoke-Gh 'api' "repos/{owner}/{repo}/actions/runs/$RunId" '--jq' '{status: .status, conclusion: .conclusion}'
        $run = $json | ConvertFrom-Json
        if ($run.status -eq 'completed')
        {
            return $run.conclusion
        }

        $stepsJson = Invoke-Gh 'api' "repos/{owner}/{repo}/actions/runs/$RunId/jobs" '--jq' '[.jobs[0].steps[] | select(.status == "in_progress") | .name]'
        $steps = @($stepsJson | ConvertFrom-Json)
        if ($steps.Count -gt 0 -and $steps[0] -ne $lastStep)
        {
            $lastStep = $steps[0]
            Write-Host "  $lastStep"
        }

        Start-Sleep -Seconds 20
    }
}

Assert-Tooling
Assert-CleanCheckout
$currentVersion = Assert-VersionAvailable -NewVersion $Version

Write-Host ''
Write-Host "TrayToolbar release $currentVersion -> $Version" -ForegroundColor Green
Write-Host "  repository   $RepositoryRoot"
Write-Host "  publishes    prerelease v$Version with signed portable zips"
Write-Host '  approvals    two SignPath signing requests, arm64 then x64'

if (-not $Force)
{
    $answer = Read-Host 'Continue? [y/N]'
    if ($answer -notmatch '^(y|yes)$')
    {
        Write-Host 'Cancelled.'
        return
    }
}

$branch = "release-$Version"

Write-Step 'Updating version files'
Update-ProjectVersion -NewVersion $Version
Update-ManifestVersion -NewVersion $Version
Update-Changelog -NewVersion $Version
Update-ReleaseNotes -NewVersion $Version

Write-Step "Opening the release pull request from $branch"
try
{
    $pullRequestUrl = New-ReleasePullRequest -NewVersion $Version -Branch $branch
}
catch
{
    Write-Warning "Rolling back to $BaseBranch. The version edits are still on '$branch' if it was created."
    Invoke-Git 'checkout' '--force' $BaseBranch | Out-Null
    throw
}

Write-Step 'Merging to master'
$mergeSha = Merge-ReleasePullRequest -PullRequestUrl $pullRequestUrl

if (-not $KeepValidationRun)
{
    Write-Step 'Cancelling the duplicate validation run'
    Stop-ValidationRun -HeadSha $mergeSha
}

Write-Step 'Dispatching the publish run'
$runId = Start-PublishRun
$runUrl = "https://github.com/$(Invoke-Gh 'repo' 'view' '--json' 'nameWithOwner' '--jq' '.nameWithOwner')/actions/runs/$runId"
Write-Host "  $runUrl"

Invoke-Git 'push' 'origin' '--delete' $branch 2>&1 | Out-Null

if ($NoWait)
{
    Write-Host ''
    Write-Host "Dispatched. Approve the signing requests in SignPath, then watch $runUrl" -ForegroundColor Green
    return
}

Write-Step 'Waiting for the publish run'
$conclusion = Wait-ForRun -RunId $runId

if ($conclusion -ne 'success')
{
    throw "The publish run finished with '$conclusion'. See $runUrl"
}

Write-Step 'Verifying the release'
$releaseJson = Invoke-Gh 'release' 'view' "v$Version" '--json' 'tagName,isDraft,isPrerelease,url,assets'
$release = $releaseJson | ConvertFrom-Json

Write-Host "  tag         $($release.tagName)"
Write-Host "  prerelease  $($release.isPrerelease)"
foreach ($asset in $release.assets)
{
    Write-Host "  asset       $($asset.name)  $($asset.size) bytes"
}

Invoke-Git 'fetch' 'origin' '--tags' | Out-Null
$tagSha = Invoke-Git 'rev-list' '-n' '1' "v$Version"
if ($tagSha -ne $mergeSha)
{
    Write-Warning "Tag v$Version points at $($tagSha.Substring(0, 8)) but the release commit is $($mergeSha.Substring(0, 8))."
}

Write-Host ''
Write-Host "Released $Version" -ForegroundColor Green
Write-Host "  $($release.url)"
