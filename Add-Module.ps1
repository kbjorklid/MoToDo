param(
    [Parameter(Mandatory = $true)]
    [string]$ModuleName,
    
    [Parameter(Mandatory = $false)]
    [string]$SolutionDirectory = "."
)

# Validate parameters
if ([string]::IsNullOrWhiteSpace($ModuleName)) {
    Write-Error "ModuleName cannot be empty"
    exit 1
}

# Validate module name format (valid C# identifier)
if ($ModuleName -notmatch '^[A-Za-z][A-Za-z0-9]*$') {
    Write-Error "ModuleName must be a valid C# identifier (letters and numbers, starting with a letter)"
    exit 1
}

# Convert to absolute path and validate solution directory
$SolutionDirectory = Resolve-Path $SolutionDirectory -ErrorAction SilentlyContinue
if (-not $SolutionDirectory -or -not (Test-Path $SolutionDirectory)) {
    Write-Error "Solution directory does not exist: $SolutionDirectory"
    exit 1
}

# Find solution file
$solutionFiles = Get-ChildItem -Path $SolutionDirectory -Filter "*.sln"
if ($solutionFiles.Count -eq 0) {
    Write-Error "No solution file found in directory: $SolutionDirectory"
    exit 1
}

if ($solutionFiles.Count -gt 1) {
    Write-Error "Multiple solution files found in directory: $SolutionDirectory. Please specify a directory with only one solution file."
    exit 1
}

$solutionFile = $solutionFiles[0].FullName
Write-Host "Using solution file: $solutionFile" -ForegroundColor Gray

# Change to solution directory
Push-Location $SolutionDirectory

try {
    # Check if module already exists
    $moduleDir = "src/$ModuleName"
    if (Test-Path $moduleDir) {
        Write-Error "Module '$ModuleName' already exists at: $moduleDir"
        exit 1
    }

    # Detect .NET version from existing projects
    Write-Host "Detecting .NET version from existing projects..." -ForegroundColor Yellow
    $existingProjects = Get-ChildItem -Path "src" -Filter "*.csproj" -Recurse
    if ($existingProjects.Count -eq 0) {
        Write-Error "No existing projects found to detect .NET version. Please run Create-ModularMonolith.ps1 first."
        exit 1
    }

    # Read first project file to get target framework
    $projectContent = Get-Content $existingProjects[0].FullName -Raw
    $targetFrameworkMatch = [regex]::Match($projectContent, '<TargetFramework>(net\d+\.\d+)</TargetFramework>')
    if (-not $targetFrameworkMatch.Success) {
        Write-Error "Could not detect .NET version from existing projects"
        exit 1
    }
    $dotNetVersion = $targetFrameworkMatch.Groups[1].Value
    Write-Host "Detected .NET version: $dotNetVersion" -ForegroundColor Gray

    Write-Host "Creating module: $ModuleName" -ForegroundColor Green

    # Create directory structure
    Write-Host "Creating directory structure..." -ForegroundColor Yellow
    $directories = @(
        "src/$ModuleName",
        "tests/$ModuleName"
    )

    foreach ($dir in $directories) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "  Created: $dir" -ForegroundColor Gray
    }

    # Create module projects
    Write-Host "Creating module projects..." -ForegroundColor Yellow

    # ModuleName.Contracts
    $contractsPath = "src/$ModuleName/$ModuleName.Contracts"
    dotnet new classlib -n "$ModuleName.Contracts" -o $contractsPath -f $dotNetVersion
    if ($LASTEXITCODE -eq 0) {
        dotnet sln add "$contractsPath/$ModuleName.Contracts.csproj"
        Write-Host "  Created: $ModuleName.Contracts" -ForegroundColor Gray
    } else {
        Write-Error "Failed to create $ModuleName.Contracts project"
        exit 1
    }

    # ModuleName.Domain
    $domainPath = "src/$ModuleName/$ModuleName.Domain"
    dotnet new classlib -n "$ModuleName.Domain" -o $domainPath -f $dotNetVersion
    if ($LASTEXITCODE -eq 0) {
        dotnet sln add "$domainPath/$ModuleName.Domain.csproj"
        Write-Host "  Created: $ModuleName.Domain" -ForegroundColor Gray
    } else {
        Write-Error "Failed to create $ModuleName.Domain project"
        exit 1
    }

    # ModuleName.Application
    $applicationPath = "src/$ModuleName/$ModuleName.Application"
    dotnet new classlib -n "$ModuleName.Application" -o $applicationPath -f $dotNetVersion
    if ($LASTEXITCODE -eq 0) {
        dotnet sln add "$applicationPath/$ModuleName.Application.csproj"
        Write-Host "  Created: $ModuleName.Application" -ForegroundColor Gray
    } else {
        Write-Error "Failed to create $ModuleName.Application project"
        exit 1
    }

    # ModuleName.Infrastructure
    $infrastructurePath = "src/$ModuleName/$ModuleName.Infrastructure"
    dotnet new classlib -n "$ModuleName.Infrastructure" -o $infrastructurePath -f $dotNetVersion
    if ($LASTEXITCODE -eq 0) {
        dotnet sln add "$infrastructurePath/$ModuleName.Infrastructure.csproj"
        Write-Host "  Created: $ModuleName.Infrastructure" -ForegroundColor Gray
    } else {
        Write-Error "Failed to create $ModuleName.Infrastructure project"
        exit 1
    }

    # Create test projects
    Write-Host "Creating test projects..." -ForegroundColor Yellow

    # ModuleName.Domain.Tests
    $domainTestsPath = "tests/$ModuleName/$ModuleName.Domain.Tests"
    dotnet new xunit -n "$ModuleName.Domain.Tests" -o $domainTestsPath -f $dotNetVersion
    if ($LASTEXITCODE -eq 0) {
        dotnet sln add "$domainTestsPath/$ModuleName.Domain.Tests.csproj"
        # Add NSubstitute package
        dotnet add "$domainTestsPath/$ModuleName.Domain.Tests.csproj" package NSubstitute
        Write-Host "  Created: $ModuleName.Domain.Tests" -ForegroundColor Gray
    } else {
        Write-Error "Failed to create $ModuleName.Domain.Tests project"
        exit 1
    }

    # ModuleName.Application.Tests
    $applicationTestsPath = "tests/$ModuleName/$ModuleName.Application.Tests"
    dotnet new xunit -n "$ModuleName.Application.Tests" -o $applicationTestsPath -f $dotNetVersion
    if ($LASTEXITCODE -eq 0) {
        dotnet sln add "$applicationTestsPath/$ModuleName.Application.Tests.csproj"
        # Add NSubstitute package
        dotnet add "$applicationTestsPath/$ModuleName.Application.Tests.csproj" package NSubstitute
        Write-Host "  Created: $ModuleName.Application.Tests" -ForegroundColor Gray
    } else {
        Write-Error "Failed to create $ModuleName.Application.Tests project"
        exit 1
    }

    # Set up project references according to architecture
    Write-Host "Setting up project references..." -ForegroundColor Yellow

    # Module internal dependencies
    dotnet add "$domainPath/$ModuleName.Domain.csproj" reference "src/BuildingBlocks/Base.Domain/Base.Domain.csproj"
    Write-Host "  $ModuleName.Domain -> Base.Domain" -ForegroundColor Gray

    dotnet add "$applicationPath/$ModuleName.Application.csproj" reference "$domainPath/$ModuleName.Domain.csproj"
    dotnet add "$applicationPath/$ModuleName.Application.csproj" reference "$contractsPath/$ModuleName.Contracts.csproj"
    dotnet add "$applicationPath/$ModuleName.Application.csproj" reference "src/BuildingBlocks/Base.Application/Base.Application.csproj"
    Write-Host "  $ModuleName.Application -> $ModuleName.Domain, $ModuleName.Contracts, Base.Application" -ForegroundColor Gray

    dotnet add "$infrastructurePath/$ModuleName.Infrastructure.csproj" reference "$applicationPath/$ModuleName.Application.csproj"
    dotnet add "$infrastructurePath/$ModuleName.Infrastructure.csproj" reference "src/BuildingBlocks/Base.Infrastructure/Base.Infrastructure.csproj"
    Write-Host "  $ModuleName.Infrastructure -> $ModuleName.Application, Base.Infrastructure" -ForegroundColor Gray

    # Test project dependencies
    dotnet add "$domainTestsPath/$ModuleName.Domain.Tests.csproj" reference "$domainPath/$ModuleName.Domain.csproj"
    Write-Host "  $ModuleName.Domain.Tests -> $ModuleName.Domain" -ForegroundColor Gray

    dotnet add "$applicationTestsPath/$ModuleName.Application.Tests.csproj" reference "$applicationPath/$ModuleName.Application.csproj"
    Write-Host "  $ModuleName.Application.Tests -> $ModuleName.Application" -ForegroundColor Gray

    # ApiHost dependencies (add references to the new module)
    dotnet add "src/ApiHost/ApiHost.csproj" reference "$applicationPath/$ModuleName.Application.csproj"
    dotnet add "src/ApiHost/ApiHost.csproj" reference "$contractsPath/$ModuleName.Contracts.csproj"
    Write-Host "  ApiHost -> $ModuleName.Application, $ModuleName.Contracts" -ForegroundColor Gray

    # SystemTests dependencies (add reference to the new module for integration testing)
    dotnet add "tests/SystemTests/SystemTests.csproj" reference "$contractsPath/$ModuleName.Contracts.csproj"
    Write-Host "  SystemTests -> $ModuleName.Contracts" -ForegroundColor Gray

    # Remove default Class1.cs files
    Write-Host "Cleaning up default files..." -ForegroundColor Yellow
    $defaultFiles = @(
        "$contractsPath/Class1.cs",
        "$domainPath/Class1.cs",
        "$applicationPath/Class1.cs",
        "$infrastructurePath/Class1.cs"
    )

    foreach ($file in $defaultFiles) {
        if (Test-Path $file) {
            Remove-Item $file -Force
            Write-Host "  Removed: $file" -ForegroundColor Gray
        }
    }

    # Build solution to verify everything is set up correctly
    Write-Host "Building solution to verify setup..." -ForegroundColor Yellow
    dotnet build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Solution built successfully!" -ForegroundColor Green
    } else {
        Write-Warning "Solution build failed. Please check the output above for errors."
    }

    Write-Host ""
    Write-Host "Module '$ModuleName' created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Structure created:" -ForegroundColor Cyan
    Write-Host "+-- src/" -ForegroundColor White
    Write-Host "|   +-- $ModuleName/" -ForegroundColor White
    Write-Host "|       +-- $ModuleName.Contracts/" -ForegroundColor White
    Write-Host "|       +-- $ModuleName.Domain/" -ForegroundColor White
    Write-Host "|       +-- $ModuleName.Application/" -ForegroundColor White
    Write-Host "|       +-- $ModuleName.Infrastructure/" -ForegroundColor White
    Write-Host "+-- tests/" -ForegroundColor White
    Write-Host "    +-- $ModuleName/" -ForegroundColor White
    Write-Host "        +-- $ModuleName.Domain.Tests/" -ForegroundColor White
    Write-Host "        +-- $ModuleName.Application.Tests/" -ForegroundColor White
    Write-Host ""
    Write-Host "Dependencies configured:" -ForegroundColor Cyan
    Write-Host "• $ModuleName.Domain -> Base.Domain" -ForegroundColor White
    Write-Host "• $ModuleName.Application -> $ModuleName.Domain, $ModuleName.Contracts, Base.Application" -ForegroundColor White
    Write-Host "• $ModuleName.Infrastructure -> $ModuleName.Application, Base.Infrastructure" -ForegroundColor White
    Write-Host "• ApiHost -> $ModuleName.Application, $ModuleName.Contracts" -ForegroundColor White
    Write-Host "• Test projects -> corresponding source projects" -ForegroundColor White
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Define your public API in $ModuleName.Contracts" -ForegroundColor White
    Write-Host "2. Implement domain logic in $ModuleName.Domain" -ForegroundColor White
    Write-Host "3. Create application services in $ModuleName.Application" -ForegroundColor White
    Write-Host "4. Add infrastructure implementations in $ModuleName.Infrastructure" -ForegroundColor White
    Write-Host "5. Write tests for your domain and application logic" -ForegroundColor White

} finally {
    Pop-Location
}