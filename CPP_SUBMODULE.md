# Working with the C++ Submodule

This project uses Git submodules to manage the C++ audio engine code. This approach keeps the C++ codebase separate while still integrating it with the C# project.

## Initial Setup

When you first clone the repository, you need to initialize the submodule:

```bash
git clone https://github.com/your-org/MaiksMixer.git
cd MaiksMixer
git submodule init
git submodule update
```

Alternatively, you can clone with submodules in one step:

```bash
git clone --recurse-submodules https://github.com/your-org/MaiksMixer.git
```

## Building the C++ Project

We've provided build scripts to simplify the process of building the C++ project and copying the output files to the C# project:

### Windows

```powershell
# PowerShell
.\build-cpp.ps1 [Debug|Release]
```

```batch
# Command Prompt
build-cpp.bat [Debug|Release]
```

If you don't specify a configuration, it defaults to `Release`.

### Prerequisites

To build the C++ project, you'll need:

- CMake (version 3.15 or higher)
- A C++ compiler (Visual Studio 2019/2022, GCC, or Clang)
- JACK Audio Connection Kit development files

## Updating the C++ Submodule

To update the C++ submodule to the latest version:

```bash
git submodule update --remote
```

Then build the updated code using the build scripts.

## Making Changes to the C++ Code

If you need to make changes to the C++ code:

1. Navigate to the submodule directory: `cd MaiksMixer.Audio/cpp`
2. Create a branch: `git checkout -b my-feature`
3. Make your changes
4. Commit and push to the C++ repository
5. Update the submodule reference in the main project:

   ```bash
   cd ../..  # Back to the main project
   git add MaiksMixer.Audio/cpp
   git commit -m "Update C++ submodule reference"
   git push
   ```

## Troubleshooting

### Missing DLLs

If you get "DLL not found" errors when running the C# application:

1. Make sure you've built the C++ project using the build scripts
2. Check that the DLLs exist in `MaiksMixer.Core/lib/`
3. Ensure the C# project is configured to copy these DLLs to the output directory

### Build Errors

If you encounter build errors:

1. Make sure you have all the prerequisites installed
2. Check the CMake output for specific error messages
3. Try cleaning the build directory: `rm -rf MaiksMixer.Audio/cpp/build` and rebuilding

## Integration with Visual Studio

You can integrate the C++ build process with Visual Studio:

1. Right-click on your C# project in Solution Explorer
2. Select "Properties"
3. Go to "Build Events" > "Pre-build event"
4. Add the command: `powershell -ExecutionPolicy Bypass -File "$(SolutionDir)build-cpp.ps1" $(ConfigurationName)`

This will automatically build the C++ project before building the C# project.
