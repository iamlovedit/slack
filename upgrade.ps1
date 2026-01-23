# Slack - Windows upgrade script (PowerShell)
# Usage: irm https://raw.githubusercontent.com/iamlovedit/slack/master/upgrade.ps1 | iex

#Requires -Version 5.1

$ErrorActionPreference = "Stop"

# Configuration
$Repo = "iamlovedit/slack"
$BinaryName = "slack.exe"
$DefaultInstallDir = "$env:LOCALAPPDATA\Programs\slack"

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] " -ForegroundColor Blue -NoNewline
    Write-Host $Message
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] " -ForegroundColor Green -NoNewline
    Write-Host $Message
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[WARN] " -ForegroundColor Yellow -NoNewline
    Write-Host $Message
}

function Write-Error-Exit {
    param([string]$Message)
    Write-Host "[ERROR] " -ForegroundColor Red -NoNewline
    Write-Host $Message
    exit 1
}

function Get-Architecture {
    $arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    switch ($arch) {
        "X64" { return "x64" }
        "Arm64" { return "arm64" }
        default { Write-Error-Exit "Unsupported architecture: $arch" }
    }
}

function Get-LatestVersion {
    try {
        $response = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases/latest" -UseBasicParsing
        return $response.tag_name
    }
    catch {
        Write-Error-Exit "Failed to get latest version. Please check your internet connection. Error: $_"
    }
}

function Get-CurrentVersion {
    $installDir = $DefaultInstallDir
    if ($env:SLACK_INSTALL_DIR) {
        $installDir = $env:SLACK_INSTALL_DIR
    }
    
    $binaryPath = Join-Path $installDir $BinaryName
    
    # Check if installed in custom directory
    if (-not (Test-Path $binaryPath)) {
        # Try to find in PATH
        $slackCommand = Get-Command "slack" -ErrorAction SilentlyContinue
        if ($slackCommand) {
            $binaryPath = $slackCommand.Source
        } else {
            return $null
        }
    }
    
    if (Test-Path $binaryPath) {
        try {
            $versionOutput = & $binaryPath version 2>&1
            if ($versionOutput -match '(\d+\.\d+\.\d+)') {
                return "v$($Matches[1])"
            }
        }
        catch {
            return $null
        }
    }
    
    return $null
}

function Compare-Versions {
    param(
        [string]$Current,
        [string]$Latest
    )
    
    # Remove 'v' prefix
    $currentClean = $Current -replace '^v', ''
    $latestClean = $Latest -replace '^v', ''
    
    try {
        $currentVersion = [Version]$currentClean
        $latestVersion = [Version]$latestClean
        
        return $currentVersion -lt $latestVersion
    }
    catch {
        # Fallback to string comparison
        return $currentClean -ne $latestClean
    }
}

function Add-ToPath {
    param([string]$Directory)
    
    $currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if ($currentPath -notlike "*$Directory*") {
        Write-Info "Adding $Directory to PATH..."
        [Environment]::SetEnvironmentVariable("Path", "$currentPath;$Directory", "User")
        $env:Path = "$env:Path;$Directory"
        Write-Success "Added to PATH. Please restart your terminal for changes to take effect."
    }
}

function Upgrade-Slack {
    Write-Host ""
    Write-Host "🐟 Slack Upgrader for Windows" -ForegroundColor Cyan
    Write-Host "=============================" -ForegroundColor Cyan
    Write-Host ""

    # Check current installation
    Write-Info "Checking current installation..."
    $currentVersion = Get-CurrentVersion
    
    if (-not $currentVersion) {
        Write-Warn "Slack is not installed. Running installation instead..."
        Write-Host ""
        # Download and run install script
        Invoke-Expression (Invoke-RestMethod -Uri "https://raw.githubusercontent.com/$Repo/master/install.ps1")
        return
    }
    
    Write-Info "Current version: $currentVersion"

    # Detect architecture
    Write-Info "Detecting architecture..."
    $arch = Get-Architecture
    $platform = "win-$arch"
    Write-Info "Platform: $platform"

    # Get latest version
    Write-Info "Getting latest version..."
    $latestVersion = Get-LatestVersion
    Write-Info "Latest version: $latestVersion"

    # Compare versions
    $needsUpgrade = Compare-Versions -Current $currentVersion -Latest $latestVersion
    
    if (-not $needsUpgrade) {
        Write-Success "You already have the latest version ($currentVersion). No upgrade needed."
        return
    }

    Write-Info "Upgrading from $currentVersion to $latestVersion..."

    # Set install directory
    $installDir = $DefaultInstallDir
    if ($env:SLACK_INSTALL_DIR) {
        $installDir = $env:SLACK_INSTALL_DIR
    }

    # Construct download URL
    $archiveName = "slack-$platform.zip"
    $downloadUrl = "https://github.com/$Repo/releases/download/$latestVersion/$archiveName"
    
    Write-Info "Downloading from: $downloadUrl"

    # Create temp directory
    $tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString())
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

    try {
        # Download archive
        $archivePath = Join-Path $tempDir $archiveName
        try {
            Invoke-WebRequest -Uri $downloadUrl -OutFile $archivePath -UseBasicParsing
        }
        catch {
            Write-Error-Exit "Failed to download $archiveName. Please check if the release exists for your platform. Error: $_"
        }

        Write-Info "Extracting..."
        Expand-Archive -Path $archivePath -DestinationPath $tempDir -Force

        # Find binary
        $binaryPath = Join-Path $tempDir $BinaryName
        if (-not (Test-Path $binaryPath)) {
            Write-Error-Exit "Binary not found in archive"
        }

        # Create install directory
        Write-Info "Installing to $installDir..."
        if (-not (Test-Path $installDir)) {
            New-Item -ItemType Directory -Path $installDir -Force | Out-Null
        }

        # Copy binary
        Copy-Item -Path $binaryPath -Destination (Join-Path $installDir $BinaryName) -Force

        # Add to PATH
        Add-ToPath -Directory $installDir

        Write-Success "Slack has been upgraded successfully from $currentVersion to $latestVersion!"
        Write-Host ""
        Write-Host "Installation directory: $installDir" -ForegroundColor Gray
        Write-Host ""

        # Verify installation
        $installedBinary = Join-Path $installDir $BinaryName
        if (Test-Path $installedBinary) {
            Write-Info "New version:"
            & $installedBinary version
        }
    }
    finally {
        # Cleanup temp directory
        if (Test-Path $tempDir) {
            Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

# Run upgrade
Upgrade-Slack
