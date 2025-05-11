# PowerShell script to build the C++ submodule with minimal configuration
param (
    [string]$Configuration = "Release"
)

Write-Host "Building C++ project in $Configuration configuration..." -ForegroundColor Cyan

# Navigate to the C++ project directory
Push-Location "MaiksMixer.Audio/cpp"

try {
    # Clean any existing build files
    if (Test-Path "build") {
        Write-Host "Cleaning existing build directory..." -ForegroundColor Yellow
        Remove-Item -Path "build" -Recurse -Force
    }
    
    # Create build directory
    Write-Host "Creating build directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "build" -Force | Out-Null
    
    # Create a minimal CMakeLists.txt file
    $cmakeContent = @"
cmake_minimum_required(VERSION 3.14)
project(MaiksMixer VERSION 0.1.0 LANGUAGES CXX)

# Set C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Define source directory
set(SRC_DIR "\${CMAKE_CURRENT_SOURCE_DIR}/emp/src")

# Include directories
include_directories(
    "C:/Program Files/JACK2/include"
    \${SRC_DIR}
)

# Add the Audio Engine library
add_library(MaiksMixerAudioEngine
    \${SRC_DIR}/AudioEngine/AudioOptimizer.cpp
    \${SRC_DIR}/AudioEngine/AudioProcessor.cpp
    \${SRC_DIR}/AudioEngine/JackAudioEngine.cpp
    \${SRC_DIR}/AudioEngine/JackAudioStream.cpp
)

# Set output directories
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY \${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY \${CMAKE_BINARY_DIR}/lib)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY \${CMAKE_BINARY_DIR}/lib)

# Add a simple test application
add_executable(MaiksMixerTest
    \${SRC_DIR}/Test/main.cpp
)

# Link against the libraries
target_link_libraries(MaiksMixerTest MaiksMixerAudioEngine)
"@
    
    Set-Content -Path "CMakeLists.txt" -Value $cmakeContent -Force
    
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
    
    # Create lib directory if it doesn't exist
    if (-not (Test-Path "../../MaiksMixer.Core/lib")) {
        Write-Host "Creating lib directory..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path "../../MaiksMixer.Core/lib" -Force | Out-Null
    }
    
    # Copy the output files
    Write-Host "Copying output files..." -ForegroundColor Yellow
    
    # Determine the output directory
    $outputDir = ""
    if (Test-Path "build/$Configuration") {
        $outputDir = "build/$Configuration"
    } else {
        $outputDir = "build"
    }
    
    # Copy libraries
    Get-ChildItem -Path $outputDir -Filter "*.lib" -Recurse | ForEach-Object {
        Write-Host "Copying $($_.Name)..." -ForegroundColor Green
        Copy-Item $_.FullName -Destination "../../MaiksMixer.Core/lib/" -Force
    }
    
    Get-ChildItem -Path $outputDir -Filter "*.dll" -Recurse | ForEach-Object {
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