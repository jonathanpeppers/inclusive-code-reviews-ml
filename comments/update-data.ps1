# break on errors
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$PSDefaultParameterValues['*:ErrorAction'] = 'Stop'

function Write-ErrorAndExit() {
    Param(
      [Parameter(Mandatory=$true, Position=0)]
      $Message,
      [Parameter(Mandatory=$true, Position=1)]
      $ExitCode,
      $AdditionalInformation
    )

    Write-Host "[ERROR] $Message" -ForegroundColor Red;
    if ($AdditionalInformation) {
        Write-Host $AdditionalInformation -ForegroundColor DarkGray;
    }
    Exit $ExitCode;
}

function Pull-RepoData {
    Param(
        [string] $Repository,
        [string] $OutputDirectory
    )

    dotnet run --project ghdump/ghdump.csproj --no-build -- --include-pull-requests --output $OutputDirectory --repository $Repository
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorAndExit -Message "❌ Failed to pull from $Repository" -ExitCode -20;
    }
}

function Pull-GitHubData {
    Param(
        [string] $OutputDirectory
    )

    try {

        git clone https://github.com/davidfowl/feedbackflow.git
        #git clone https://github.com/RussKie/feedbackflow.git
        Push-Location feedbackflow
        git pull origin main
        git checkout main

        $cleanUp = $false

        dotnet build

        $repos = @(
            "dotnet/runtime",
            "dotnet/aspnetcore",
            "dotnet/winforms",
            "dotnet/wpf",
            "dotnet/maui",
            "dotnet/efcore",
            "dotnet/roslyn"
            );
        foreach ($repo in $repos) {
            Pull-RepoData -Repository $repo -OutputDirectory $OutputDirectory
        }

        Write-Host "✔️ Data downloaded successfully" -ForegroundColor Green
        $cleanUp = $true
    }
    catch {
        Write-Host "[ERROR]" -Foreground "Red"
        Write-Host $_.Exception.Message -Foreground "Red"
        Write-Host $_.ScriptStackTrace -Foreground "DarkGray"

        Exit -1
    }
    finally {
        Pop-Location

        if ($cleanUp) {
            Remove-Item -Path 'feedbackflow' -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

function Scrub-RepoData {
    Param(
        [string] $DataFile,
        [string] $OutputDirectory
    )

    dotnet run --project GitHubDataScrubber.csproj --no-build -- --input $DataFile --output $OutputDirectory
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorAndExit -Message "❌ Failed to scrub $DataFile" -ExitCode -30;
    }
}

function Scrub-GitHubData {
    param (
        [string] $OutputDirectory
    )

    try {
        git clone https://github.com/RussKie/GitHubDataScrubber.git
        Push-Location GitHubDataScrubber
        git pull origin master
        git checkout master

        $cleanUp = $false

        dotnet build

        Get-ChildItem -Path $OutputDirectory -Filter "*.json" | `
            ForEach-Object {
                Scrub-RepoData -DataFile $_.FullName -OutputDirectory $OutputDirectory
            }

        Write-Host "✔️ Data scrubbed successfully" -ForegroundColor Green
        $cleanUp = $true
    }
    catch {
        Write-Host "[ERROR]" -Foreground "Red"
        Write-Host $_.Exception.Message -Foreground "Red"
        Write-Host $_.ScriptStackTrace -Foreground "DarkGray"

        Exit -2
    }
    finally {
        Pop-Location

        if ($cleanUp) {
            Remove-Item -Path 'GitHubDataScrubber' -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

function Merge-CsvFiles {
    param (
        [string] $InputDirectory
    )

    $csvFiles = Get-ChildItem -Path $InputDirectory -Filter "*.csv"
    $outputContent = @()

    foreach ($file in $csvFiles) {
        $content = Get-Content -Path $file.FullName
        $outputContent += $content
    }

    $OutputFile = "$InputDirectory/merged.csv"
    if (Test-Path $OutputFile) {
        Remove-Item -Path $OutputFile -Force
    }

    $outputContent | Sort-Object -Unique | Out-File -FilePath $OutputFile -Encoding utf8
}

try {
    Push-Location $PSScriptRoot
    
    if (-not (Test-Path ghdata)) {
        mkdir ghdata | Out-Null
    }

    $datagDir = "$PSScriptRoot/ghdata"
    
    Pull-GitHubData -OutputDirectory $datagDir
    Scrub-GitHubData -OutputDirectory $datagDir
    Merge-CsvFiles -InputDirectory $datagDir
}
finally {
    Pop-Location
}