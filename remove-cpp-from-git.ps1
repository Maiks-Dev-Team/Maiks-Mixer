# PowerShell script to remove C++ project files from Git index
# This script doesn't delete any files from your filesystem

Write-Host "Removing C++ project files from Git index..." -ForegroundColor Cyan

# Check if the C++ project directory exists
if (Test-Path "MaiksMixer.Audio/cpp") {
    # Remove the directory from Git's index
    Write-Host "Removing MaiksMixer.Audio/cpp from Git index..." -ForegroundColor Yellow
    git rm -r --cached "MaiksMixer.Audio/cpp"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error removing files from Git index. Exit code: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Successfully removed C++ project files from Git index." -ForegroundColor Green
    Write-Host "The files still exist on your filesystem but are no longer tracked by Git." -ForegroundColor Green
    Write-Host "You should now commit this change:" -ForegroundColor Cyan
    Write-Host "git commit -m 'Remove C++ project files from Git tracking'" -ForegroundColor Yellow
} else {
    Write-Host "C++ project directory not found at MaiksMixer.Audio/cpp" -ForegroundColor Yellow
}

# Remind about setting up the submodule
Write-Host "`nAfter committing, you should set up the C++ project as a submodule:" -ForegroundColor Cyan
Write-Host "git submodule add https://github.com/your-org/cpp-audio-engine.git MaiksMixer.Audio/cpp" -ForegroundColor Yellow
Write-Host "git commit -m 'Add C++ project as submodule'" -ForegroundColor Yellow
