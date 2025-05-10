# PowerShell script to build the C++ submodule and copy outputs to the C# project
# Usage: .\build-cpp.ps1 [Debug|Release]

param (
    [string]$Configuration = "Release"
)

Write-Host "Building C++ project in $Configuration configuration..." -ForegroundColor Cyan

# Check if the submodule directory exists
if (-not (Test-Path "MaiksMixer.Audio/cpp/.git")) {
    Write-Host "Initializing Git submodule..." -ForegroundColor Yellow
    git submodule init
    git submodule update
}
else {
    Write-Host "Updating Git submodule..." -ForegroundColor Yellow
    git submodule update --remote
}

# Create lib directory if it doesn't exist
if (-not (Test-Path "MaiksMixer.Core/lib")) {
    Write-Host "Creating lib directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "MaiksMixer.Core/lib" -Force | Out-Null
}

# Navigate to the C++ project directory
Push-Location "MaiksMixer.Audio/cpp"

try {
    # Create build directory if it doesn't exist
    if (-not (Test-Path "build")) {
        Write-Host "Creating build directory..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path "build" -Force | Out-Null
    }

    # Configure with CMake
    Write-Host "Configuring with CMake..." -ForegroundColor Yellow
    cmake -B build -S . -DCMAKE_BUILD_TYPE=$Configuration
    
    if ($LASTEXITCODE -ne 0) {
        throw "CMake configuration failed with exit code $LASTEXITCODE"
    }

    # Build the project
    Write-Host "Building with CMake..." -ForegroundColor Yellow
    cmake --build build --config $Configuration
    
    if ($LASTEXITCODE -ne 0) {
        throw "CMake build failed with exit code $LASTEXITCODE"
    }

    # Determine the output directory based on the build system
    $outputDir = ""
    if (Test-Path "build/$Configuration") {
        # Visual Studio / Multi-configuration generator
        $outputDir = "build/$Configuration"
    } else {
        # Single-configuration generator (e.g., Makefile, Ninja)
        $outputDir = "build"
    }

    # Copy the output files to the C# project
    Write-Host "Copying output files to C# project..." -ForegroundColor Yellow
    
    # Copy DLLs
    Get-ChildItem -Path "$outputDir" -Filter "*.dll" | ForEach-Object {
        Write-Host "Copying $($_.Name)..." -ForegroundColor Green
        Copy-Item $_.FullName -Destination "../../MaiksMixer.Core/lib/" -Force
    }
    
    # Copy shared libraries for Linux/macOS if they exist
    Get-ChildItem -Path "$outputDir" -Filter "*.so" -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "Copying $($_.Name)..." -ForegroundColor Green
        Copy-Item $_.FullName -Destination "../../MaiksMixer.Core/lib/" -Force
    }
    
    Get-ChildItem -Path "$outputDir" -Filter "*.dylib" -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "Copying $($_.Name)..." -ForegroundColor Green
        Copy-Item $_.FullName -Destination "../../MaiksMixer.Core/lib/" -Force
    }

    Write-Host "Build completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
finally {
    # Return to the original directory
    Pop-Location
}
