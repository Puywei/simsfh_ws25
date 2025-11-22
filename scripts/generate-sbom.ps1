# SBOM Generation Script für SIMS-Projekt (PowerShell)
# Generiert CycloneDX SBOMs für alle .NET-Projekte

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$SbomOutputDir = Join-Path $ProjectRoot "sbom-output"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SIMS SBOM Generation Script" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Erstelle Output-Verzeichnis
if (-not (Test-Path $SbomOutputDir)) {
    New-Item -ItemType Directory -Path $SbomOutputDir | Out-Null
}

# Installiere CycloneDX .NET Tool falls nicht vorhanden
Write-Host "Prüfe CycloneDX .NET Tool..." -ForegroundColor Yellow
$cyclonedxInstalled = dotnet tool list -g | Select-String "cyclonedx"
if (-not $cyclonedxInstalled) {
    Write-Host "Installiere CycloneDX .NET Tool..." -ForegroundColor Yellow
    dotnet tool install --global CycloneDX
} else {
    Write-Host "CycloneDX Tool ist bereits installiert." -ForegroundColor Green
}

# Projekte definieren
$Projects = @(
    "BackendApi\BackendApi.csproj",
    "sims-api\sims-api.csproj",
    "sims-nosql-api\sims-nosql-api.csproj",
    "sims-web_app\sims-web_app.csproj"
)

# Generiere SBOM für jedes Projekt
foreach ($Project in $Projects) {
    $ProjectPath = Join-Path $ProjectRoot $Project
    
    if (-not (Test-Path $ProjectPath)) {
        Write-Host "⚠️  Warnung: Projekt nicht gefunden: $ProjectPath" -ForegroundColor Yellow
        continue
    }
    
    $ProjectName = [System.IO.Path]::GetFileNameWithoutExtension($Project)
    $OutputFile = Join-Path $SbomOutputDir "$ProjectName-sbom.json"
    
    Write-Host ""
    Write-Host "Generiere SBOM für: $ProjectName" -ForegroundColor Cyan
    Write-Host "  Projekt: $Project"
    Write-Host "  Output: $OutputFile"
    
    # Wechsle ins Projektverzeichnis
    $ProjectDir = Split-Path -Parent $ProjectPath
    Push-Location $ProjectDir
    
    try {
        # Generiere SBOM mit CycloneDX
        dotnet CycloneDX $Project `
            --json `
            --out $OutputFile `
            --exclude-dev `
            --exclude-test
        
        if (Test-Path $OutputFile) {
            Write-Host "  ✅ SBOM erfolgreich generiert: $OutputFile" -ForegroundColor Green
            
            # Zeige Anzahl der Komponenten (falls jq verfügbar ist)
            try {
                $jsonContent = Get-Content $OutputFile -Raw | ConvertFrom-Json
                $componentCount = $jsonContent.components.Count
                Write-Host "  📦 Komponenten gefunden: $componentCount" -ForegroundColor Cyan
            } catch {
                # Ignoriere Fehler beim Parsen
            }
        } else {
            Write-Host "  ❌ Fehler beim Generieren der SBOM" -ForegroundColor Red
        }
    } finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SBOM Generation abgeschlossen!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Output-Verzeichnis: $SbomOutputDir"
Write-Host ""
Write-Host "Nächste Schritte:"
Write-Host "1. Überprüfe die generierten SBOM-Dateien in: $SbomOutputDir"
Write-Host "2. Lade die SBOMs in DependencyTrack hoch (http://localhost:8082)"
Write-Host "3. Oder verwende das upload-sbom.ps1 Script für automatischen Upload"
Write-Host ""

