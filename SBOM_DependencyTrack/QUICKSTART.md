# SBOM & DependencyTrack - Quick Start Guide ⭐

**Diese Datei ist die Hauptanleitung für den ersten Durchlauf!**

Für detaillierte Informationen siehe [README.md](README.md).

## Schnellstart

### 1. DependencyTrack starten

```bash
# Im Projekt-Root-Verzeichnis
docker-compose up -d dependencytrack-postgres dependencytrack dependencytrack-frontend 
```

Warte ca. 30-60 Sekunden, bis DependencyTrack vollständig gestartet ist.

### 2. DependencyTrack initialisieren

1. Öffne **http://localhost:8083** (Web-UI)
2. Logge dich ein mit:
   - Username: `admin`
   - Password: `admin`
3. **WICHTIG:** Ändere das Passwort beim ersten Login
4. Gehe zu: **Administration > Access Management > Teams > Automation**
5. Klicke auf **Create API Key**
6. Kopiere den API Key (wird nur einmal angezeigt!)

**Hinweis:** 
- Web-UI: http://localhost:8083
- API: http://localhost:8082

### 3. SBOMs generieren

**Linux/macOS:**
```bash
cd SBOM_DependencyTrack
chmod +x generate-sbom.sh
./generate-sbom.sh
```

**Windows:**
```powershell
cd SBOM_DependencyTrack
.\generate-sbom.ps1
```

### 4. SBOMs hochladen

**Linux/macOS/WSL/Git Bash:**
```bash
cd SBOM_DependencyTrack
chmod +x upload-sbom.sh
./upload-sbom.sh
```

**Hinweis:** Das Script hat den API-Key bereits eingebaut. Falls Sie einen anderen verwenden möchten:
```bash
export DEPENDENCYTRACK_API_KEY="dein-api-key"
./upload-sbom.sh
```

### 5. Ergebnisse ansehen

Öffne DependencyTrack Web-UI: **http://localhost:8083**

Du solltest jetzt folgende Projekte sehen:
- SIMS-BackendApi
- SIMS-UserApi
- SIMS-NoSqlApi
- SIMS-WebApp

Klicke auf ein Projekt, um:
- Alle Abhängigkeiten zu sehen
- Sicherheitslücken zu überprüfen
- Risiko-Scores anzuzeigen

## Troubleshooting

### DependencyTrack startet nicht

```bash
# Prüfe Logs
docker logs dependencytrack

# Prüfe ob PostgreSQL läuft
docker ps | grep dependencytrack-postgres

# Starte neu
docker-compose restart dependencytrack dependencytrack-frontend
```

### API Key funktioniert nicht

- Stelle sicher, dass der API Key korrekt kopiert wurde (keine Leerzeichen)
- Prüfe ob der API Key die Berechtigung "BOM_UPLOAD" hat
- Erstelle einen neuen API Key falls nötig

### SBOM Generation schlägt fehl

```bash
# Installiere CycloneDX manuell
dotnet tool install --global CycloneDX

# Prüfe ob Projekte gebaut werden können
cd BackendApi
dotnet restore
dotnet build
```

## Nächste Schritte

- **Regelmäßige Scans:** Richte einen Cron-Job/Scheduled Task ein, um SBOMs regelmäßig zu generieren
- **CI/CD Integration:** Integriere die Scripts in deine CI/CD-Pipeline
- **Alerts:** Konfiguriere E-Mail-Benachrichtigungen in DependencyTrack für kritische Vulnerabilities

