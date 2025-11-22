# SBOM Upload Script for DependencyTrack (PowerShell)
# Uploads all generated SBOMs to DependencyTrack

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$SbomOutputDir = Join-Path $ScriptDir "outputs"

# DependencyTrack Configuration
$DependencyTrackUrl = if ($env:DEPENDENCYTRACK_URL) { $env:DEPENDENCYTRACK_URL } else { "http://localhost:8082" }
$DependencyTrackWebUrl = if ($env:DEPENDENCYTRACK_WEB_URL) { $env:DEPENDENCYTRACK_WEB_URL } else { "http://localhost:8083" }

# API Key - can be overridden by environment variable
$DependencyTrackApiKey = if ($env:DEPENDENCYTRACK_API_KEY) { 
    $env:DEPENDENCYTRACK_API_KEY 
} else { 
    "odt_31eGzZ3i_nMTGh2qK3evWtTXZET6zNX2vysEcnYAb"
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SIMS SBOM Upload to DependencyTrack" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if SBOM directory exists
if (-not (Test-Path $SbomOutputDir)) {
    Write-Host "Error: SBOM output directory not found: $SbomOutputDir" -ForegroundColor Red
    Write-Host "Please run generate-sbom.ps1 first." -ForegroundColor Yellow
    exit 1
}

# Check if DependencyTrack is reachable
Write-Host "Checking DependencyTrack connection..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$DependencyTrackUrl/api/version" -Method Get -UseBasicParsing -ErrorAction Stop
    Write-Host "DependencyTrack is reachable" -ForegroundColor Green
} catch {
    Write-Host "Error: DependencyTrack is not reachable at: $DependencyTrackUrl" -ForegroundColor Red
    Write-Host "Please make sure DependencyTrack is running:"
    Write-Host "  docker-compose up -d dependencytrack dependencytrack-frontend dependencytrack-postgres" -ForegroundColor Cyan
    exit 1
}

# Project mapping
$ProjectMapping = @{
    "BackendApi" = "SIMS-BackendApi"
    "sims-api" = "SIMS-UserApi"
    "sims-nosql-api" = "SIMS-NoSqlApi"
    "sims-web_app" = "SIMS-WebApp"
}

# Upload SBOMs
$sbomFiles = Get-ChildItem -Path $SbomOutputDir -Filter "*-sbom.json"

if ($sbomFiles.Count -eq 0) {
    Write-Host "No SBOM files found in: $SbomOutputDir" -ForegroundColor Yellow
    Write-Host "Please run generate-sbom.ps1 first." -ForegroundColor Yellow
    exit 1
}

foreach ($SbomFile in $sbomFiles) {
    $Filename = $SbomFile.Name
    $ProjectName = $Filename -replace '-sbom\.json$', ''
    $DtProjectName = if ($ProjectMapping.ContainsKey($ProjectName)) { $ProjectMapping[$ProjectName] } else { $ProjectName }
    
    Write-Host ""
    Write-Host "Uploading SBOM: $Filename" -ForegroundColor Cyan
    Write-Host "  Project: $ProjectName -> $DtProjectName"
    
    if (-not [string]::IsNullOrEmpty($DependencyTrackApiKey)) {
        # Check if project exists
        try {
            $projectLookupUrl = "$DependencyTrackUrl/api/v1/project/lookup?name=$DtProjectName&version=1.0.0"
            $projectResponse = Invoke-RestMethod -Uri $projectLookupUrl `
                -Method Get `
                -Headers @{ "X-Api-Key" = $DependencyTrackApiKey } `
                -ErrorAction SilentlyContinue
            
            $projectUuid = $projectResponse.uuid
        } catch {
            $projectUuid = $null
        }
        
        if ([string]::IsNullOrEmpty($projectUuid)) {
            Write-Host "  Creating project in DependencyTrack..." -ForegroundColor Yellow
            $projectBody = @{
                name = $DtProjectName
                version = "1.0.0"
                description = "SIMS $ProjectName Component"
                purl = "pkg:nuget/$ProjectName@1.0.0"
            } | ConvertTo-Json
            
            try {
                $createResponse = Invoke-RestMethod -Uri "$DependencyTrackUrl/api/v1/project" `
                    -Method Put `
                    -Headers @{ 
                        "X-Api-Key" = $DependencyTrackApiKey
                        "Content-Type" = "application/json"
                    } `
                    -Body $projectBody
                $projectUuid = $createResponse.uuid
            } catch {
                Write-Host "  Warning: Error creating project: $_" -ForegroundColor Yellow
                continue
            }
        }
        
        # Upload SBOM
        Write-Host "  Uploading SBOM..." -ForegroundColor Yellow
        $sbomContent = Get-Content $SbomFile.FullName -Raw -Encoding UTF8
        $uploadBody = @{
            project = $projectUuid
            bom = ($sbomContent | ConvertFrom-Json)
        } | ConvertTo-Json -Depth 100
        
        try {
            $uploadResponse = Invoke-RestMethod -Uri "$DependencyTrackUrl/api/v1/bom" `
                -Method Put `
                -Headers @{ 
                    "X-Api-Key" = $DependencyTrackApiKey
                    "Content-Type" = "application/json"
                } `
                -Body $uploadBody
            
            if ($uploadResponse.token) {
                Write-Host "  SBOM successfully uploaded (Token: $($uploadResponse.token))" -ForegroundColor Green
            } else {
                Write-Host "  Warning: Upload response: $($uploadResponse | ConvertTo-Json)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "  Error uploading: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "  Skipping upload (no API key)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SBOM Upload completed!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "DependencyTrack API URL: $DependencyTrackUrl"
Write-Host "DependencyTrack Web UI: $DependencyTrackWebUrl"
Write-Host ""
