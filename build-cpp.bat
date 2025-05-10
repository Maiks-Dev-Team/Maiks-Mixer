@echo off
setlocal enabledelayedexpansion

:: Default to Release configuration if not specified
set CONFIG=Release
if not "%1"=="" set CONFIG=%1

echo Building C++ project in %CONFIG% configuration...

:: Check if the submodule directory exists
if not exist "MaiksMixer.Audio\cpp\.git" (
    echo Initializing Git submodule...
    git submodule init
    git submodule update
) else (
    echo Updating Git submodule...
    git submodule update --remote
)

:: Create lib directory if it doesn't exist
if not exist "MaiksMixer.Core\lib" (
    echo Creating lib directory...
    mkdir "MaiksMixer.Core\lib"
)

:: Navigate to the C++ project directory
pushd "MaiksMixer.Audio\cpp"

:: Create build directory if it doesn't exist
if not exist "build" (
    echo Creating build directory...
    mkdir "build"
)

:: Configure with CMake
echo Configuring with CMake...
cmake -B build -S . -DCMAKE_BUILD_TYPE=%CONFIG%

if %ERRORLEVEL% neq 0 (
    echo CMake configuration failed with exit code %ERRORLEVEL%
    goto :error
)

:: Build the project
echo Building with CMake...
cmake --build build --config %CONFIG%

if %ERRORLEVEL% neq 0 (
    echo CMake build failed with exit code %ERRORLEVEL%
    goto :error
)

:: Determine the output directory based on the build system
set OUTPUT_DIR=build
if exist "build\%CONFIG%" (
    :: Visual Studio / Multi-configuration generator
    set OUTPUT_DIR=build\%CONFIG%
)

:: Copy the output files to the C# project
echo Copying output files to C# project...

:: Copy DLLs
for %%F in ("%OUTPUT_DIR%\*.dll") do (
    echo Copying %%~nxF...
    copy "%%F" "..\..\MaiksMixer.Core\lib\" /Y
)

:: Copy shared libraries for Linux/macOS if they exist
for %%F in ("%OUTPUT_DIR%\*.so") do (
    echo Copying %%~nxF...
    copy "%%F" "..\..\MaiksMixer.Core\lib\" /Y
)

for %%F in ("%OUTPUT_DIR%\*.dylib") do (
    echo Copying %%~nxF...
    copy "%%F" "..\..\MaiksMixer.Core\lib\" /Y
)

echo Build completed successfully!
goto :end

:error
echo Error occurred during build process
exit /b 1

:end
:: Return to the original directory
popd
exit /b 0
