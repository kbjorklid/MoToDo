param(
    [Parameter(Mandatory = $false)]
    [string]$SolutionName,
    
    [Parameter(Mandatory = $true)]
    [string]$DotNetVersion
)

# Get default solution name from current directory
$defaultSolutionName = Split-Path -Leaf (Get-Location)

# Prompt for solution name if not provided
if ([string]::IsNullOrWhiteSpace($SolutionName)) {
    $SolutionName = Read-Host "Enter solution name [$defaultSolutionName]"
    if ([string]::IsNullOrWhiteSpace($SolutionName)) {
        $SolutionName = $defaultSolutionName
    }
}

if ([string]::IsNullOrWhiteSpace($DotNetVersion)) {
    Write-Error "DotNetVersion cannot be empty"
    exit 1
}

# Validate .NET version format
if ($DotNetVersion -notmatch '^net\d+\.\d+$') {
    Write-Error "DotNetVersion must be in format 'netX.X' (e.g., 'net8.0')"
    exit 1
}

Write-Host "Creating Modular Monolith solution: $SolutionName" -ForegroundColor Green
Write-Host "Target .NET version: $DotNetVersion" -ForegroundColor Green

# Create solution file
Write-Host "Creating solution file..." -ForegroundColor Yellow
dotnet new sln -n $SolutionName

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create solution"
    exit 1
}

# Create directory structure
Write-Host "Creating directory structure..." -ForegroundColor Yellow
$directories = @(
    "src",
    "src/BuildingBlocks",
    "src/ApiHost",
    "tests",
    "tests/BuildingBlocks",
    "tests/SystemTests"
)

foreach ($dir in $directories) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    Write-Host "  Created: $dir" -ForegroundColor Gray
}

# Create BuildingBlocks projects
Write-Host "Creating BuildingBlocks projects..." -ForegroundColor Yellow

# Base.Domain
$baseDomainPath = "src/BuildingBlocks/Base.Domain"
dotnet new classlib -n "Base.Domain" -o $baseDomainPath -f $DotNetVersion
if ($LASTEXITCODE -eq 0) {
    dotnet sln add "$baseDomainPath/Base.Domain.csproj"
    Write-Host "  Created: Base.Domain" -ForegroundColor Gray
} else {
    Write-Error "Failed to create Base.Domain project"
    exit 1
}

# Base.Application
$baseApplicationPath = "src/BuildingBlocks/Base.Application"
dotnet new classlib -n "Base.Application" -o $baseApplicationPath -f $DotNetVersion
if ($LASTEXITCODE -eq 0) {
    dotnet sln add "$baseApplicationPath/Base.Application.csproj"
    Write-Host "  Created: Base.Application" -ForegroundColor Gray
} else {
    Write-Error "Failed to create Base.Application project"
    exit 1
}

# Base.Infrastructure
$baseInfrastructurePath = "src/BuildingBlocks/Base.Infrastructure"
dotnet new classlib -n "Base.Infrastructure" -o $baseInfrastructurePath -f $DotNetVersion
if ($LASTEXITCODE -eq 0) {
    dotnet sln add "$baseInfrastructurePath/Base.Infrastructure.csproj"
    Write-Host "  Created: Base.Infrastructure" -ForegroundColor Gray
} else {
    Write-Error "Failed to create Base.Infrastructure project"
    exit 1
}

# Create ApiHost project
Write-Host "Creating ApiHost project..." -ForegroundColor Yellow
$apiHostPath = "src/ApiHost"
dotnet new webapi -n "ApiHost" -o $apiHostPath -f $DotNetVersion
if ($LASTEXITCODE -eq 0) {
    dotnet sln add "$apiHostPath/ApiHost.csproj"
    Write-Host "  Created: ApiHost" -ForegroundColor Gray
} else {
    Write-Error "Failed to create ApiHost project"
    exit 1
}

# Create test projects
Write-Host "Creating test projects..." -ForegroundColor Yellow

# Base.Domain.Tests
$baseDomainTestsPath = "tests/BuildingBlocks/Base.Domain.Tests"
dotnet new xunit -n "Base.Domain.Tests" -o $baseDomainTestsPath -f $DotNetVersion
if ($LASTEXITCODE -eq 0) {
    dotnet sln add "$baseDomainTestsPath/Base.Domain.Tests.csproj"
    # Add NSubstitute package
    dotnet add "$baseDomainTestsPath/Base.Domain.Tests.csproj" package NSubstitute
    Write-Host "  Created: Base.Domain.Tests" -ForegroundColor Gray
} else {
    Write-Error "Failed to create Base.Domain.Tests project"
    exit 1
}

# Base.Application.Tests
$baseApplicationTestsPath = "tests/BuildingBlocks/Base.Application.Tests"
dotnet new xunit -n "Base.Application.Tests" -o $baseApplicationTestsPath -f $DotNetVersion
if ($LASTEXITCODE -eq 0) {
    dotnet sln add "$baseApplicationTestsPath/Base.Application.Tests.csproj"
    # Add NSubstitute package
    dotnet add "$baseApplicationTestsPath/Base.Application.Tests.csproj" package NSubstitute
    Write-Host "  Created: Base.Application.Tests" -ForegroundColor Gray
} else {
    Write-Error "Failed to create Base.Application.Tests project"
    exit 1
}

# SystemTests
$systemTestsPath = "tests/SystemTests"
dotnet new xunit -n "SystemTests" -o $systemTestsPath -f $DotNetVersion
if ($LASTEXITCODE -eq 0) {
    dotnet sln add "$systemTestsPath/SystemTests.csproj"
    # Add NSubstitute and Microsoft.AspNetCore.Mvc.Testing packages
    dotnet add "$systemTestsPath/SystemTests.csproj" package NSubstitute
    dotnet add "$systemTestsPath/SystemTests.csproj" package Microsoft.AspNetCore.Mvc.Testing
    Write-Host "  Created: SystemTests" -ForegroundColor Gray
} else {
    Write-Error "Failed to create SystemTests project"
    exit 1
}

# Add project references according to architecture
Write-Host "Setting up project references..." -ForegroundColor Yellow

# BuildingBlocks dependencies: Base.Application -> Base.Domain
dotnet add "$baseApplicationPath/Base.Application.csproj" reference "$baseDomainPath/Base.Domain.csproj"
Write-Host "  Base.Application -> Base.Domain" -ForegroundColor Gray

# BuildingBlocks dependencies: Base.Infrastructure -> Base.Application
dotnet add "$baseInfrastructurePath/Base.Infrastructure.csproj" reference "$baseApplicationPath/Base.Application.csproj"
Write-Host "  Base.Infrastructure -> Base.Application" -ForegroundColor Gray

# ApiHost dependencies
dotnet add "$apiHostPath/ApiHost.csproj" reference "$baseApplicationPath/Base.Application.csproj"
dotnet add "$apiHostPath/ApiHost.csproj" reference "$baseInfrastructurePath/Base.Infrastructure.csproj"
Write-Host "  ApiHost -> Base.Application, Base.Infrastructure" -ForegroundColor Gray

# Test project dependencies
dotnet add "$baseDomainTestsPath/Base.Domain.Tests.csproj" reference "$baseDomainPath/Base.Domain.csproj"
Write-Host "  Base.Domain.Tests -> Base.Domain" -ForegroundColor Gray

dotnet add "$baseApplicationTestsPath/Base.Application.Tests.csproj" reference "$baseApplicationPath/Base.Application.csproj"
Write-Host "  Base.Application.Tests -> Base.Application" -ForegroundColor Gray

dotnet add "$systemTestsPath/SystemTests.csproj" reference "$apiHostPath/ApiHost.csproj"
Write-Host "  SystemTests -> ApiHost" -ForegroundColor Gray

# Remove default Class1.cs files
Write-Host "Cleaning up default files..." -ForegroundColor Yellow
$defaultFiles = @(
    "$baseDomainPath/Class1.cs",
    "$baseApplicationPath/Class1.cs", 
    "$baseInfrastructurePath/Class1.cs"
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

dotnet new gitignore

Write-Host ""
Write-Host "Modular Monolith solution '$SolutionName' created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Structure created:" -ForegroundColor Cyan
Write-Host "+-- $SolutionName.sln" -ForegroundColor White
Write-Host "+-- src/" -ForegroundColor White
Write-Host "|   +-- BuildingBlocks/" -ForegroundColor White
Write-Host "|   |   +-- Base.Domain/" -ForegroundColor White
Write-Host "|   |   +-- Base.Application/" -ForegroundColor White
Write-Host "|   |   +-- Base.Infrastructure/" -ForegroundColor White
Write-Host "|   +-- ApiHost/" -ForegroundColor White
Write-Host "+-- tests/" -ForegroundColor White
Write-Host "    +-- BuildingBlocks/" -ForegroundColor White
Write-Host "    |   +-- Base.Domain.Tests/" -ForegroundColor White
Write-Host "    |   +-- Base.Application.Tests/" -ForegroundColor White
Write-Host "    +-- SystemTests/" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Use a separate script to create your business modules" -ForegroundColor White
Write-Host "2. Implement your domain logic in the Domain projects" -ForegroundColor White
Write-Host "3. Add your application services in the Application projects" -ForegroundColor White
Write-Host "4. Configure your infrastructure in the Infrastructure projects" -ForegroundColor White
Write-Host "5. Set up your API controllers in the ApiHost project" -ForegroundColor White