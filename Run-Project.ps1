#!/usr/bin/env pwsh

param(
    [switch]$Clean,
    [switch]$SkipBuild,
    [switch]$Help
)

if ($Help) {
    Write-Host "Run-Project.ps1 - Compile and run the MoToDo project" -ForegroundColor Green
    Write-Host ""
    Write-Host "Parameters:" -ForegroundColor Yellow
    Write-Host "  -Clean      Clean the solution before building"
    Write-Host "  -SkipBuild  Skip the build step and run directly"
    Write-Host "  -Help       Show this help message"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\Run-Project.ps1                # Build and run"
    Write-Host "  .\Run-Project.ps1 -Clean         # Clean, build, and run"
    Write-Host "  .\Run-Project.ps1 -SkipBuild     # Run without building"
    exit 0
}

$ErrorActionPreference = "Stop"

Write-Host "MoToDo Project Runner" -ForegroundColor Green
Write-Host "===================="

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning solution..." -ForegroundColor Yellow
    dotnet clean
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Clean failed"
        exit 1
    }
    Write-Host "Clean completed successfully" -ForegroundColor Green
}

# Build unless skipped
if (-not $SkipBuild) {
    Write-Host "Building solution..." -ForegroundColor Yellow
    dotnet build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed"
        exit 1
    }
    Write-Host "Build completed successfully" -ForegroundColor Green
}

# Run the API
Write-Host "Starting MoToDo API..." -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Cyan
Write-Host ""

dotnet run --project src/ApiHost/ApiHost.csproj