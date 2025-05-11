# PowerShell script to copy the MaiksMixer project to a path without special characters
param (
    [string]$DestinationPath = "C:\MaiksMixer"
)

Write-Host "Copying MaiksMixer project to a clean path: $DestinationPath" -ForegroundColor Cyan

# Create the destination directory if it doesn't exist
if (-not (Test-Path $DestinationPath)) {
    Write-Host "Creating destination directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null
}

# Get the source directory (current project location)
$sourceDir = $PSScriptRoot

# Copy the project files to the new location
Write-Host "Copying project files..." -ForegroundColor Yellow
Copy-Item -Path "$sourceDir\*" -Destination $DestinationPath -Recurse -Force -Exclude @(".git", "bin", "obj", "packages", "TestResults", ".vs")

# Create a .gitignore file in the new location to exclude build artifacts
$gitignoreContent = @"
# Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
bld/
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/

# Visual Studio files
.vs/
*.user
*.userosscache
*.suo
*.userprefs
*.sln.docstates

# Build artifacts
*.dll
*.exe
*.pdb
*.ilk
*.obj
*.lib
*.exp
*.log

# CMake build directory
build/
CMakeFiles/
CMakeCache.txt
cmake_install.cmake
"@

Set-Content -Path "$DestinationPath\.gitignore" -Value $gitignoreContent

# Create a README file with instructions
$readmeContent = @"
# MaiksMixer (Clean Path Version)

This is a copy of the MaiksMixer project with a clean path (no special characters) to avoid CMake build issues.

## Building the C++ Code

1. Open a command prompt or PowerShell window in this directory
2. Run the build script: `.\build-cpp.ps1`

## Original Project Location

The original project is located at: $sourceDir

## Notes

- Any changes made in this copy will need to be manually synchronized with the original project
- This is a temporary solution until the C++ team resolves the CMake build issues
"@

Set-Content -Path "$DestinationPath\README.md" -Value $readmeContent

Write-Host "Project copied successfully!" -ForegroundColor Green
Write-Host "New project location: $DestinationPath" -ForegroundColor Green
Write-Host "You can now build the C++ code from the new location." -ForegroundColor Green