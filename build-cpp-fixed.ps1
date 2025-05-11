# PowerShell script to build the C++ submodule with workarounds for path issues
param (
    [string]$Configuration = "Release"
)

Write-Host "Building C++ project in $Configuration configuration..." -ForegroundColor Cyan

# Create a temporary build directory without special characters in the path
$tempBuildDir = "C:/temp/maiks_mixer_build"
if (-not (Test-Path $tempBuildDir)) {
    Write-Host "Creating temporary build directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $tempBuildDir -Force | Out-Null
}

# Copy the C++ source files to the temporary directory
$sourceDir = "$PSScriptRoot/MaiksMixer.Audio/cpp"
$tempSourceDir = "$tempBuildDir/cpp"

Write-Host "Copying source files to temporary directory..." -ForegroundColor Yellow
if (Test-Path $tempSourceDir) {
    Remove-Item -Path $tempSourceDir -Recurse -Force
}
Copy-Item -Path $sourceDir -Destination $tempBuildDir -Recurse -Force

# Navigate to the temporary C++ project directory
Push-Location $tempSourceDir

try {
    # Create build directory
    $buildDir = "$tempSourceDir/build"
    if (-not (Test-Path $buildDir)) {
        Write-Host "Creating build directory..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
    }

    # Configure with CMake
    Write-Host "Configuring with CMake..." -ForegroundColor Yellow
    $cmakeOutput = & cmake -B $buildDir -S . -DCMAKE_BUILD_TYPE=$Configuration 2>&1
    
    # Check if CMake succeeded
    if ($LASTEXITCODE -ne 0) {
        Write-Host "CMake configuration output:" -ForegroundColor Red
        $cmakeOutput | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        throw "CMake configuration failed with exit code $LASTEXITCODE"
    }

    # Build the project
    Write-Host "Building with CMake..." -ForegroundColor Yellow
    $buildOutput = & cmake --build $buildDir --config $Configuration 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "CMake build output:" -ForegroundColor Red
        $buildOutput | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        throw "CMake build failed with exit code $LASTEXITCODE"
    }

    # Determine the output directory based on the build system
    $outputDir = ""
    if (Test-Path "$buildDir/$Configuration") {
        # Visual Studio / Multi-configuration generator
        $outputDir = "$buildDir/$Configuration"
    } else {
        # Single-configuration generator (e.g., Makefile, Ninja)
        $outputDir = $buildDir
    }

    # Create lib directory in the original project if it doesn't exist
    $libDir = "$PSScriptRoot/MaiksMixer.Core/lib"
    if (-not (Test-Path $libDir)) {
        Write-Host "Creating lib directory..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $libDir -Force | Out-Null
    }

    # Copy the output files to the original project
    Write-Host "Copying output files to original project..." -ForegroundColor Yellow
    
    # Copy DLLs
    Get-ChildItem -Path $outputDir -Filter "*.dll" -Recurse | ForEach-Object {
        Write-Host "Copying $($_.Name)..." -ForegroundColor Green
        Copy-Item $_.FullName -Destination $libDir -Force
    }
    
    # Copy static libraries
    Get-ChildItem -Path $outputDir -Filter "*.lib" -Recurse | ForEach-Object {
        Write-Host "Copying $($_.Name)..." -ForegroundColor Green
        Copy-Item $_.FullName -Destination $libDir -Force
    }
    
    # Copy shared libraries for Linux/macOS if they exist
    Get-ChildItem -Path $outputDir -Filter "*.so" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "Copying $($_.Name)..." -ForegroundColor Green
        Copy-Item $_.FullName -Destination $libDir -Force
    }
    
    Get-ChildItem -Path $outputDir -Filter "*.dylib" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "Copying $($_.Name)..." -ForegroundColor Green
        Copy-Item $_.FullName -Destination $libDir -Force
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