#!/bin/bash

# SBOM Upload Script für DependencyTrack
# Lädt alle generierten SBOMs in DependencyTrack hoch

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SBOM_OUTPUT_DIR="$SCRIPT_DIR/outputs"

# DependencyTrack Konfiguration
DEPENDENCYTRACK_URL="${DEPENDENCYTRACK_URL:-http://localhost:8082}"
DEPENDENCYTRACK_WEB_URL="${DEPENDENCYTRACK_WEB_URL:-http://localhost:8083}"
DEPENDENCYTRACK_API_KEY="${DEPENDENCYTRACK_API_KEY:-}"

echo "=========================================="
echo "SIMS SBOM Upload zu DependencyTrack"
echo "=========================================="
echo ""

# Prüfe ob SBOM-Verzeichnis existiert
if [ ! -d "$SBOM_OUTPUT_DIR" ]; then
    echo "❌ SBOM-Output-Verzeichnis nicht gefunden: $SBOM_OUTPUT_DIR"
    echo "Bitte führe zuerst generate-sbom.sh aus."
    exit 1
fi

# Prüfe ob API Key gesetzt ist
if [ -z "$DEPENDENCYTRACK_API_KEY" ]; then
    echo "⚠️  Warnung: DEPENDENCYTRACK_API_KEY nicht gesetzt."
    echo ""
    echo "Bitte setze die Umgebungsvariable:"
    echo "  export DEPENDENCYTRACK_API_KEY='dein-api-key'"
    echo ""
    echo "Oder erstelle einen API Key in DependencyTrack:"
    echo "  1. Öffne: $DEPENDENCYTRACK_WEB_URL (Web-UI)"
    echo "  2. Gehe zu: Administration > Access Management > Teams > Automation"
    echo "  3. Erstelle einen neuen API Key"
    echo ""
    read -p "Möchtest du trotzdem fortfahren? (j/n): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[JjYy]$ ]]; then
        exit 1
    fi
fi

# Prüfe ob DependencyTrack erreichbar ist
echo "Prüfe DependencyTrack Verbindung..."
if ! curl -s -f "$DEPENDENCYTRACK_URL/api/version" > /dev/null 2>&1; then
    echo "❌ DependencyTrack ist nicht erreichbar unter: $DEPENDENCYTRACK_URL"
    echo "Bitte stelle sicher, dass DependencyTrack läuft:"
    echo "  docker-compose up -d dependencytrack"
    exit 1
fi
echo "✅ DependencyTrack ist erreichbar"

# Projekte und ihre DependencyTrack Projekt-Namen
declare -A PROJECT_MAPPING=(
    ["BackendApi"]="SIMS-BackendApi"
    ["sims-api"]="SIMS-UserApi"
    ["sims-nosql-api"]="SIMS-NoSqlApi"
    ["sims-web_app"]="SIMS-WebApp"
)

# Lade SBOMs hoch
for SBOM_FILE in "$SBOM_OUTPUT_DIR"/*-sbom.json; do
    if [ ! -f "$SBOM_FILE" ]; then
        continue
    fi
    
    FILENAME=$(basename "$SBOM_FILE")
    PROJECT_NAME=$(echo "$FILENAME" | sed 's/-sbom\.json$//')
    DT_PROJECT_NAME="${PROJECT_MAPPING[$PROJECT_NAME]:-$PROJECT_NAME}"
    
    echo ""
    echo "Lade SBOM hoch: $FILENAME"
    echo "  Projekt: $PROJECT_NAME -> $DT_PROJECT_NAME"
    
    # Erstelle Projekt in DependencyTrack falls nicht vorhanden
    if [ -n "$DEPENDENCYTRACK_API_KEY" ]; then
        # Prüfe ob Projekt existiert
        PROJECT_UUID=$(curl -s -X GET \
            "$DEPENDENCYTRACK_URL/api/v1/project/lookup?name=$DT_PROJECT_NAME&version=1.0.0" \
            -H "X-Api-Key: $DEPENDENCYTRACK_API_KEY" \
            -H "Content-Type: application/json" | jq -r '.uuid' 2>/dev/null || echo "")
        
        if [ -z "$PROJECT_UUID" ] || [ "$PROJECT_UUID" = "null" ]; then
            echo "  Erstelle Projekt in DependencyTrack..."
            PROJECT_UUID=$(curl -s -X PUT \
                "$DEPENDENCYTRACK_URL/api/v1/project" \
                -H "X-Api-Key: $DEPENDENCYTRACK_API_KEY" \
                -H "Content-Type: application/json" \
                -d "{
                    \"name\": \"$DT_PROJECT_NAME\",
                    \"version\": \"1.0.0\",
                    \"description\": \"SIMS $PROJECT_NAME Component\",
                    \"purl\": \"pkg:nuget/$PROJECT_NAME@1.0.0\"
                }" | jq -r '.uuid' 2>/dev/null || echo "")
        fi
        
        # Lade SBOM hoch
        echo "  Lade SBOM hoch..."
        UPLOAD_RESULT=$(curl -s -X PUT \
            "$DEPENDENCYTRACK_URL/api/v1/bom" \
            -H "X-Api-Key: $DEPENDENCYTRACK_API_KEY" \
            -H "Content-Type: application/json" \
            -d @- <<EOF
{
    "project": "$PROJECT_UUID",
    "bom": $(cat "$SBOM_FILE")
}
EOF
        )
        
        if echo "$UPLOAD_RESULT" | jq -e '.token' > /dev/null 2>&1; then
            TOKEN=$(echo "$UPLOAD_RESULT" | jq -r '.token')
            echo "  ✅ SBOM erfolgreich hochgeladen (Token: $TOKEN)"
        else
            echo "  ⚠️  Upload-Response: $UPLOAD_RESULT"
        fi
    else
        echo "  ⚠️  Überspringe Upload (kein API Key)"
    fi
done

echo ""
echo "=========================================="
echo "SBOM Upload abgeschlossen!"
echo "=========================================="
echo "DependencyTrack URL: $DEPENDENCYTRACK_URL"
echo ""

