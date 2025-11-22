# SBOM Upload Script für DependencyTrack (PowerShell)
# Lädt alle generierten SBOMs in DependencyTrack hoch

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$SbomOutputDir = Join-Path $ProjectRoot "sbom-output"

# DependencyTrack Konfiguration
$DependencyTrackUrl = if ($env:DEPENDENCYTRACK_URL) { $env:DEPENDENCYTRACK_URL } else { "http://localhost:8082" }
$DependencyTrackApiKey = $env:DEPENDENCYTRACK_API_KEY

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SIMS SBOM Upload zu DependencyTrack" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Prüfe ob SBOM-Verzeichnis existiert
if (-not (Test-Path $SbomOutputDir)) {
    Write-Host "❌ SBOM-Output-Verzeichnis nicht gefunden: $SbomOutputDir" -ForegroundColor Red
    Write-Host "Bitte führe zuerst generate-sbom.ps1 aus." -ForegroundColor Yellow
    exit 1
}

# Prüfe ob API Key gesetzt ist
if ([string]::IsNullOrEmpty($DependencyTrackApiKey)) {
    Write-Host "⚠️  Warnung: DEPENDENCYTRACK_API_KEY nicht gesetzt." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Bitte setze die Umgebungsvariable:"
    Write-Host "  `$env:DEPENDENCYTRACK_API_KEY='dein-api-key'" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Oder erstelle einen API Key in DependencyTrack:"
    Write-Host "  1. Öffne: http://localhost:8083 (Web-UI)"
    Write-Host "  2. Gehe zu: Administration > Access Management > Teams > Automation"
    Write-Host "  3. Erstelle einen neuen API Key"
    Write-Host ""
    $continue = Read-Host "Möchtest du trotzdem fortfahren? (j/n)"
    if ($continue -ne "j" -and $continue -ne "J" -and $continue -ne "y" -and $continue -ne "Y") {
        exit 1
    }
}

# Prüfe ob DependencyTrack erreichbar ist
Write-Host "Prüfe DependencyTrack Verbindung..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$DependencyTrackUrl/api/version" -Method Get -UseBasicParsing -ErrorAction Stop
    Write-Host "✅ DependencyTrack ist erreichbar" -ForegroundColor Green
} catch {
    Write-Host "❌ DependencyTrack ist nicht erreichbar unter: $DependencyTrackUrl" -ForegroundColor Red
    Write-Host "Bitte stelle sicher, dass DependencyTrack läuft:"
    Write-Host "  docker-compose up -d dependencytrack" -ForegroundColor Cyan
    exit 1
}

# Projekte und ihre DependencyTrack Projekt-Namen
$ProjectMapping = @{
    "BackendApi" = "SIMS-BackendApi"
    "sims-api" = "SIMS-UserApi"
    "sims-nosql-api" = "SIMS-NoSqlApi"
    "sims-web_app" = "SIMS-WebApp"
}

# Lade SBOMs hoch
$sbomFiles = Get-ChildItem -Path $SbomOutputDir -Filter "*-sbom.json"

foreach ($SbomFile in $sbomFiles) {
    $Filename = $SbomFile.Name
    $ProjectName = $Filename -replace '-sbom\.json$', ''
    $DtProjectName = if ($ProjectMapping.ContainsKey($ProjectName)) { $ProjectMapping[$ProjectName] } else { $ProjectName }
    
    Write-Host ""
    Write-Host "Lade SBOM hoch: $Filename" -ForegroundColor Cyan
    Write-Host "  Projekt: $ProjectName -> $DtProjectName"
    
    if (-not [string]::IsNullOrEmpty($DependencyTrackApiKey)) {
        # Prüfe ob Projekt existiert
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
            Write-Host "  Erstelle Projekt in DependencyTrack..." -ForegroundColor Yellow
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
                Write-Host "  ⚠️  Fehler beim Erstellen des Projekts: $_" -ForegroundColor Yellow
                continue
            }
        }
        
        # Lade SBOM hoch
        Write-Host "  Lade SBOM hoch..." -ForegroundColor Yellow
        $sbomContent = Get-Content $SbomFile.FullName -Raw
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
                Write-Host "  ✅ SBOM erfolgreich hochgeladen (Token: $($uploadResponse.token))" -ForegroundColor Green
            } else {
                Write-Host "  ⚠️  Upload-Response: $($uploadResponse | ConvertTo-Json)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "  ❌ Fehler beim Hochladen: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "  ⚠️  Überspringe Upload (kein API Key)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SBOM Upload abgeschlossen!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "DependencyTrack URL: $DependencyTrackUrl"
Write-Host ""

