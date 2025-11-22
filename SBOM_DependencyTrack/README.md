# SBOM & DependencyTrack Scripts

Dieses Verzeichnis enthält Scripts zur automatischen Generierung von Software Bill of Materials (SBOM) und zum Upload in DependencyTrack.

## 📖 Welche Datei soll ich lesen?

- **QUICKSTART.md** ⭐ - **Start hier!** Schnelle Schritt-für-Schritt Anleitung für den ersten Durchlauf
- **README.md** (diese Datei) - Vollständige Dokumentation und Referenz

## Übersicht

- **generate-sbom.sh / generate-sbom.ps1**: Generiert CycloneDX SBOMs für alle .NET-Projekte
- **upload-sbom.sh**: Lädt generierte SBOMs in DependencyTrack hoch

## Voraussetzungen

### 1. DependencyTrack Container starten

```bash
# Im Projekt-Root-Verzeichnis
docker-compose up -d dependencytrack dependencytrack-frontend dependencytrack-postgres
```

**Zugriff:**
- **Web-UI:** http://localhost:8083 (für Login und Verwaltung)
- **API:** http://localhost:8082 (für API-Zugriff)

**Standard-Credentials:**
- Username: `admin`
- Password: `admin` (wird beim ersten Login geändert)

### 2. API Key erstellen

1. Öffne DependencyTrack Web-UI: **http://localhost:8083**
2. Logge dich ein (Standard: admin/admin)
3. Gehe zu: **Administration > Access Management > Teams > Automation**
4. Erstelle einen neuen API Key
5. Kopiere den API Key

### 3. CycloneDX Tool installieren

Das Script installiert CycloneDX automatisch, falls es nicht vorhanden ist. Alternativ manuell:

```bash
dotnet tool install --global CycloneDX
```

## Verwendung

### Linux/macOS (Bash)

```bash
# 1. SBOMs generieren
cd SBOM_DependencyTrack
chmod +x generate-sbom.sh upload-sbom.sh
./generate-sbom.sh

# 2. SBOMs in DependencyTrack hochladen
export DEPENDENCYTRACK_API_KEY="dein-api-key"
./upload-sbom.sh
```

### Windows (Git Bash oder WSL)

```bash
# 1. SBOMs generieren
cd SBOM_DependencyTrack
chmod +x generate-sbom.sh upload-sbom.sh
./generate-sbom.sh

# 2. SBOMs in DependencyTrack hochladen
./upload-sbom.sh
```

**Alternative: PowerShell für Generation**
```powershell
# 1. SBOMs generieren
cd SBOM_DependencyTrack
.\generate-sbom.ps1

# 2. SBOMs hochladen (Git Bash oder WSL)
# Öffne Git Bash oder WSL und führe aus:
cd SBOM_DependencyTrack
./upload-sbom.sh
```

## Output

Die generierten SBOM-Dateien werden im Verzeichnis `outputs/` gespeichert:

```
SBOM_DependencyTrack/outputs/
├── BackendApi-sbom.json
├── sims-api-sbom.json
├── sims-nosql-api-sbom.json
└── sims-web_app-sbom.json
```

## Projekte in DependencyTrack

Die Scripts erstellen automatisch folgende Projekte in DependencyTrack:

- **SIMS-BackendApi** (Version 1.0.0)
- **SIMS-UserApi** (Version 1.0.0)
- **SIMS-NoSqlApi** (Version 1.0.0)
- **SIMS-WebApp** (Version 1.0.0)

## Automatisierung

### CI/CD Integration

Die Scripts können in CI/CD-Pipelines integriert werden:

```yaml
# Beispiel: GitHub Actions
- name: Generate SBOMs
  run: ./scripts/generate-sbom.sh

- name: Upload SBOMs to DependencyTrack
  env:
    DEPENDENCYTRACK_API_KEY: ${{ secrets.DEPENDENCYTRACK_API_KEY }}
  run: ./scripts/upload-sbom.sh
```

### Regelmäßige Ausführung

Für regelmäßige SBOM-Generierung kann ein Cron-Job (Linux) oder Scheduled Task (Windows) eingerichtet werden.

## Troubleshooting

### DependencyTrack nicht erreichbar

```bash
# Prüfe ob Container läuft
docker ps | grep dependencytrack

# Starte Container falls nötig
docker-compose up -d dependencytrack dependencytrack-frontend dependencytrack-postgres

# Prüfe Logs
docker logs dependencytrack
```

### API Key Fehler

- Stelle sicher, dass der API Key korrekt gesetzt ist
- Prüfe ob der API Key die notwendigen Berechtigungen hat
- Erstelle einen neuen API Key falls nötig

### SBOM Generation Fehler

- Stelle sicher, dass alle .NET-Projekte erfolgreich gebaut werden können
- Prüfe ob CycloneDX Tool korrekt installiert ist: `dotnet tool list -g`
- Führe `dotnet restore` in den Projektverzeichnissen aus

## Weitere Informationen

- [CycloneDX Dokumentation](https://cyclonedx.org/)
- [DependencyTrack Dokumentation](https://docs.dependencytrack.org/)
- [CycloneDX .NET Tool](https://github.com/CycloneDX/cyclonedx-dotnet)

