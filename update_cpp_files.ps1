# Script to fetch the latest C++ files from GitHub for MaiksMixer
# Usage: .\update_cpp_files.ps1 [repository_url] [branch_name]

param (
    [string]$repoUrl = "https://github.com/Maiks-Dev-Team/MaiksMixerCpp.git",
    [string]$branch = "master"
)

$ErrorActionPreference = "Stop"
$tempDir = Join-Path $env:TEMP "MaiksMixer_cpp_temp"
$cppDir = Join-Path $PSScriptRoot "MaiksMixer.Audio\cpp"

Write-Host "=== MaiksMixer C++ Update Script ===" -ForegroundColor Cyan
Write-Host "Repository: $repoUrl" -ForegroundColor Yellow
Write-Host "Branch: $branch" -ForegroundColor Yellow
Write-Host "Destination: $cppDir" -ForegroundColor Yellow

# Create directories if they don't exist
if (-not (Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir | Out-Null
    Write-Host "Created temporary directory: $tempDir" -ForegroundColor Green
}

if (-not (Test-Path $cppDir)) {
    New-Item -ItemType Directory -Path $cppDir -Force | Out-Null
    Write-Host "Created C++ directory: $cppDir" -ForegroundColor Green
}

# Function to clean up temporary files
function Cleanup {
    if (Test-Path $tempDir) {
        Write-Host "Cleaning up temporary files..." -ForegroundColor Yellow
        Remove-Item -Path $tempDir -Recurse -Force
    }
}

try {
    # Change to temp directory
    Push-Location $tempDir

    # Check if git is installed
    try {
        git --version | Out-Null
    } catch {
        Write-Host "Git is not installed or not in PATH. Please install Git and try again." -ForegroundColor Red
        exit 1
    }

    # Clone or update the repository
    if (Test-Path (Join-Path $tempDir ".git")) {
        Write-Host "Updating existing repository..." -ForegroundColor Yellow
        git fetch origin
        git checkout $branch
        git pull origin $branch
    } else {
        Write-Host "Cloning repository..." -ForegroundColor Yellow
        git clone --branch $branch --depth 1 $repoUrl .
    }

    # Find all C++ files
    Write-Host "Finding C++ files..." -ForegroundColor Yellow
    $cppFiles = Get-ChildItem -Path $tempDir -Recurse -Include "*.cpp", "*.h", "*.hpp", "*.c" | Where-Object {
        # Exclude files in certain directories like .git, build, etc.
        $relativePath = $_.FullName.Substring($tempDir.Length + 1)
        -not ($relativePath -match "^\.git|^build|^bin|^obj|^Debug|^Release")
    }

    # Copy C++ files to destination
    Write-Host "Copying C++ files to $cppDir..." -ForegroundColor Yellow
    
    # Clear destination directory first
    if (Test-Path $cppDir) {
        Get-ChildItem -Path $cppDir -Recurse | Remove-Item -Force -Recurse
    }
    
    foreach ($file in $cppFiles) {
        $relativePath = $file.FullName.Substring($tempDir.Length + 1)
        $destPath = Join-Path $cppDir $relativePath
        $destDir = Split-Path -Parent $destPath
        
        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        }
        
        Copy-Item -Path $file.FullName -Destination $destPath -Force
        Write-Host "  Copied: $relativePath" -ForegroundColor Green
    }

    Write-Host "Successfully updated C++ files!" -ForegroundColor Green
    Write-Host "Total files copied: $($cppFiles.Count)" -ForegroundColor Cyan

} catch {
    Write-Host "Error: $_" -ForegroundColor Red
} finally {
    # Return to original directory
    Pop-Location
    
    # Clean up
    Cleanup
}

Write-Host "`nDone! C++ files have been updated in: $cppDir" -ForegroundColor Cyan
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
