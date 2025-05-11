# PowerShell script to update C++ files from GitHub
# Usage: .\update-cpp-files.ps1 [GitHub URL]

param (
    [string]$GitHubUrl = "https://github.com/MaiksMixer/MaiksMixerAudio.git"
)

Write-Host "Updating C++ files from GitHub..." -ForegroundColor Cyan

# Create a temporary directory for cloning
$tempDir = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.Guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

try {
    # Clone the repository to the temporary directory
    Write-Host "Cloning repository from $GitHubUrl..." -ForegroundColor Yellow
    git clone $GitHubUrl $tempDir

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to clone repository from $GitHubUrl"
    }

    # Create the target directories if they don't exist
    $targetDirs = @(
        "MaiksMixer.Audio\cpp\emp\src\AudioEngine",
        "MaiksMixer.Audio\cpp\emp\src\Bridge",
        "MaiksMixer.Audio\cpp\emp\src\JackClient",
        "MaiksMixer.Audio\cpp\emp\src\Test"
    )

    foreach ($dir in $targetDirs) {
        $fullPath = Join-Path -Path $PSScriptRoot -ChildPath $dir
        if (-not (Test-Path $fullPath)) {
            Write-Host "Creating directory: $fullPath" -ForegroundColor Yellow
            New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        }
    }

    # Copy the C++ files from the temporary directory to the project
    Write-Host "Copying C++ files to project..." -ForegroundColor Yellow
    
    # Assuming the repository has a similar structure to what we need
    $sourceDirs = @(
        @{ Source = "src\AudioEngine"; Target = "MaiksMixer.Audio\cpp\emp\src\AudioEngine" },
        @{ Source = "src\Bridge"; Target = "MaiksMixer.Audio\cpp\emp\src\Bridge" },
        @{ Source = "src\JackClient"; Target = "MaiksMixer.Audio\cpp\emp\src\JackClient" },
        @{ Source = "src\Test"; Target = "MaiksMixer.Audio\cpp\emp\src\Test" }
    )

    foreach ($dirPair in $sourceDirs) {
        $sourceDir = Join-Path -Path $tempDir -ChildPath $dirPair.Source
        $targetDir = Join-Path -Path $PSScriptRoot -ChildPath $dirPair.Target
        
        if (Test-Path $sourceDir) {
            Write-Host "Copying files from $($dirPair.Source) to $($dirPair.Target)..." -ForegroundColor Green
            Copy-Item -Path "$sourceDir\*" -Destination $targetDir -Recurse -Force
        } else {
            Write-Host "Source directory $($dirPair.Source) not found in the repository" -ForegroundColor Yellow
        }
    }

    # Copy CMakeLists.txt if it exists
    $sourceCMake = Join-Path -Path $tempDir -ChildPath "CMakeLists.txt"
    $targetCMake = Join-Path -Path $PSScriptRoot -ChildPath "MaiksMixer.Audio\cpp\CMakeLists.txt"
    
    if (Test-Path $sourceCMake) {
        Write-Host "Copying CMakeLists.txt..." -ForegroundColor Green
        Copy-Item -Path $sourceCMake -Destination $targetCMake -Force
    }

    Write-Host "C++ files updated successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
finally {
    # Clean up the temporary directory
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Recurse -Force
    }
}
