# SBOM Generation Script for SIMS Project (PowerShell)
# Generates CycloneDX SBOMs for all .NET projects

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$SbomOutputDir = Join-Path $ScriptDir "outputs"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SIMS SBOM Generation Script" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Create output directory
if (-not (Test-Path $SbomOutputDir)) {
    New-Item -ItemType Directory -Path $SbomOutputDir | Out-Null
}

# Install CycloneDX .NET Tool if not present
Write-Host "Checking CycloneDX .NET Tool..." -ForegroundColor Yellow
$cyclonedxInstalled = dotnet tool list -g | Select-String "cyclonedx"
if (-not $cyclonedxInstalled) {
    Write-Host "Installing CycloneDX .NET Tool..." -ForegroundColor Yellow
    dotnet tool install --global CycloneDX
} else {
    Write-Host "CycloneDX Tool is already installed." -ForegroundColor Green
}

# Define projects
$Projects = @(
    "BackendApi\BackendApi.csproj",
    "sims-api\sims-api.csproj",
    "sims-nosql-api\sims-nosql-api.csproj",
    "sims-web_app\sims-web_app.csproj"
)

# Generate SBOM for each project
foreach ($Project in $Projects) {
    $ProjectPath = Join-Path $ProjectRoot $Project
    
    if (-not (Test-Path $ProjectPath)) {
        Write-Host "Warning: Project not found: $ProjectPath" -ForegroundColor Yellow
        continue
    }
    
    $ProjectName = [System.IO.Path]::GetFileNameWithoutExtension($Project)
    $OutputFile = Join-Path $SbomOutputDir "$ProjectName-sbom.json"
    
    Write-Host ""
    Write-Host "Generating SBOM for: $ProjectName" -ForegroundColor Cyan
    Write-Host "  Project: $Project"
    Write-Host "  Output: $OutputFile"
    
    # Change to project directory
    $ProjectDir = Split-Path -Parent $ProjectPath
    $ProjectFileName = [System.IO.Path]::GetFileName($ProjectPath)
    $OutputDir = Split-Path -Parent $OutputFile
    Push-Location $ProjectDir
    
    try {
        # Generate SBOM with CycloneDX
        # Use -o for output directory, -fn for filename, -F Json for format, -ed to exclude dev dependencies
        # Use only the filename since we're already in the project directory
        dotnet CycloneDX $ProjectFileName `
            -o $OutputDir `
            -fn "$ProjectName-sbom.json" `
            -F Json `
            -ed
        
        $GeneratedFile = Join-Path $OutputDir "$ProjectName-sbom.json"
        if (Test-Path $GeneratedFile) {
            Write-Host "  SBOM successfully generated: $GeneratedFile" -ForegroundColor Green
            
            # Show component count
            try {
                $jsonContent = Get-Content $GeneratedFile -Raw -Encoding UTF8 | ConvertFrom-Json
                $componentCount = $jsonContent.components.Count
                Write-Host "  Components found: $componentCount" -ForegroundColor Cyan
            } catch {
                # Ignore parsing errors
            }
        } else {
            Write-Host "  Error generating SBOM" -ForegroundColor Red
        }
    } finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SBOM Generation completed!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Output directory: $SbomOutputDir"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Review generated SBOM files in: $SbomOutputDir"
Write-Host "2. Upload SBOMs to DependencyTrack (http://localhost:8083)"
Write-Host "3. Or use the upload-sbom.ps1 script for automatic upload"
Write-Host ""
