# PowerShell script to build the C++ code directly using Visual C++ compiler
param (
    [string]$Configuration = "Release"
)

Write-Host "Building C++ project in $Configuration configuration..." -ForegroundColor Cyan

# Find Visual Studio installation
function Find-VisualStudio {
    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vsWhere) {
        $vsInstallPath = & $vsWhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
        if ($vsInstallPath) {
            return $vsInstallPath
        }
    }
    # Fallback to common paths
    $commonPaths = @(
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Community",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Professional",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Enterprise",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Professional",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise"
    )
    foreach ($path in $commonPaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    return $null
}

# Initialize Visual Studio environment
function Initialize-VsEnvironment {
    $vsPath = Find-VisualStudio
    if (-not $vsPath) {
        throw "Visual Studio installation not found"
    }
    
    Write-Host "Using Visual Studio at: $vsPath" -ForegroundColor Yellow
    
    # Find the developer command prompt batch file
    $vcvarsallPath = "$vsPath\VC\Auxiliary\Build\vcvarsall.bat"
    if (-not (Test-Path $vcvarsallPath)) {
        throw "Visual C++ environment setup script not found at: $vcvarsallPath"
    }
    
    # Create a temporary batch file to capture environment variables
    $tempBatchFile = [System.IO.Path]::GetTempFileName() + ".bat"
    $tempEnvFile = [System.IO.Path]::GetTempFileName()
    
    try {
        # Create batch file that calls vcvarsall.bat and outputs environment variables
        @"
@echo off
call "$vcvarsallPath" x64
set > "$tempEnvFile"
"@ | Out-File -FilePath $tempBatchFile -Encoding ASCII
        
        # Execute the batch file
        cmd.exe /c $tempBatchFile
        
        # Read the environment variables and set them in the current PowerShell session
        Get-Content $tempEnvFile | ForEach-Object {
            if ($_ -match '(.+?)=(.*)') {
                $name = $matches[1]
                $value = $matches[2]
                [System.Environment]::SetEnvironmentVariable($name, $value, [System.EnvironmentVariableTarget]::Process)
            }
        }
        
        Write-Host "Visual Studio environment initialized" -ForegroundColor Green
    }
    finally {
        # Clean up temporary files
        if (Test-Path $tempBatchFile) { Remove-Item $tempBatchFile -Force }
        if (Test-Path $tempEnvFile) { Remove-Item $tempEnvFile -Force }
    }
}

# Create output directories
function Create-OutputDirectories {
    $directories = @(
        "build",
        "build/obj",
        "build/bin",
        "build/lib",
        "../MaiksMixer.Core/lib"
    )
    
    foreach ($dir in $directories) {
        if (-not (Test-Path $dir)) {
            Write-Host "Creating directory: $dir" -ForegroundColor Yellow
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
        }
    }
}

# Compile C++ files
function Compile-CppFiles {
    param (
        [string]$Configuration
    )
    
    $compilerFlags = "/nologo /EHsc /std:c++17 /W3"
    if ($Configuration -eq "Debug") {
        $compilerFlags += " /Od /Zi /MDd /D_DEBUG"
    } else {
        $compilerFlags += " /O2 /MD /DNDEBUG"
    }
    
    # Include paths
    $includePaths = @(
        "/I`"C:/Program Files/JACK2/include`"",
        "/I`"./emp/src`""
    )
    
    # Source files
    $sourceFiles = @(
        "./emp/src/AudioEngine/AudioOptimizer.cpp",
        "./emp/src/AudioEngine/AudioProcessor.cpp",
        "./emp/src/AudioEngine/JackAudioEngine.cpp",
        "./emp/src/AudioEngine/JackAudioStream.cpp",
        "./emp/src/JackClient/JackDynamicLoader.cpp",
        "./emp/src/JackClient/JackMixerClient.cpp",
        "./emp/src/JackClient/JackMixerClient_impl.cpp",
        "./emp/src/Test/main.cpp"
    )
    
    # Compile each source file
    $objFiles = @()
    foreach ($sourceFile in $sourceFiles) {
        if (Test-Path $sourceFile) {
            $objFile = "build/obj/" + [System.IO.Path]::GetFileNameWithoutExtension($sourceFile) + ".obj"
            $objFiles += $objFile
            
            $compileCommand = "cl.exe $compilerFlags $($includePaths -join ' ') /c $sourceFile /Fo$objFile"
            Write-Host "Compiling: $sourceFile" -ForegroundColor Yellow
            Write-Host $compileCommand
            
            Invoke-Expression $compileCommand
            if ($LASTEXITCODE -ne 0) {
                throw "Compilation failed for $sourceFile with exit code $LASTEXITCODE"
            }
        } else {
            Write-Host "Warning: Source file not found: $sourceFile" -ForegroundColor Yellow
        }
    }
    
    # Create a static library
    $libFile = "build/lib/MaiksMixerAudio.lib"
    $libCommand = "lib.exe /nologo /out:$libFile $($objFiles -join ' ')"
    Write-Host "Creating library: $libFile" -ForegroundColor Yellow
    Write-Host $libCommand
    
    Invoke-Expression $libCommand
    if ($LASTEXITCODE -ne 0) {
        throw "Library creation failed with exit code $LASTEXITCODE"
    }
    
    # Copy the library to the output directory
    Copy-Item $libFile -Destination "../MaiksMixer.Core/lib/" -Force
    Write-Host "Copied library to ../MaiksMixer.Core/lib/" -ForegroundColor Green
    
    return $libFile
}

# Main script execution
try {
    Push-Location "MaiksMixer.Audio/cpp"
    
    # Initialize Visual Studio environment
    Initialize-VsEnvironment
    
    # Create output directories
    Create-OutputDirectories
    
    # Compile C++ files
    $libFile = Compile-CppFiles -Configuration $Configuration
    
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Output library: $libFile" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}