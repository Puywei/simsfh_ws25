#!/bin/bash

# SBOM Generation Script für SIMS-Projekt
# Generiert CycloneDX SBOMs für alle .NET-Projekte

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SBOM_OUTPUT_DIR="$PROJECT_ROOT/sbom-output"
CYCLONEDX_VERSION="3.0.0"

echo "=========================================="
echo "SIMS SBOM Generation Script"
echo "=========================================="
echo ""

# Erstelle Output-Verzeichnis
mkdir -p "$SBOM_OUTPUT_DIR"

# Installiere CycloneDX .NET Tool falls nicht vorhanden
echo "Prüfe CycloneDX .NET Tool..."
if ! dotnet tool list -g | grep -q "cyclonedx"; then
    echo "Installiere CycloneDX .NET Tool..."
    dotnet tool install --global CycloneDX
else
    echo "CycloneDX Tool ist bereits installiert."
fi

# Projekte definieren
PROJECTS=(
    "BackendApi/BackendApi.csproj"
    "sims-api/sims-api.csproj"
    "sims-nosql-api/sims-nosql-api.csproj"
    "sims-web_app/sims-web_app.csproj"
)

# Generiere SBOM für jedes Projekt
for PROJECT in "${PROJECTS[@]}"; do
    PROJECT_PATH="$PROJECT_ROOT/$PROJECT"
    
    if [ ! -f "$PROJECT_PATH" ]; then
        echo "⚠️  Warnung: Projekt nicht gefunden: $PROJECT_PATH"
        continue
    fi
    
    PROJECT_NAME=$(basename "$PROJECT" .csproj)
    OUTPUT_FILE="$SBOM_OUTPUT_DIR/${PROJECT_NAME}-sbom.json"
    
    echo ""
    echo "Generiere SBOM für: $PROJECT_NAME"
    echo "  Projekt: $PROJECT"
    echo "  Output: $OUTPUT_FILE"
    
    # Wechsle ins Projektverzeichnis
    cd "$(dirname "$PROJECT_PATH")"
    
    # Generiere SBOM mit CycloneDX
    dotnet CycloneDX "$PROJECT" \
        --json \
        --out "$OUTPUT_FILE" \
        --exclude-dev \
        --exclude-test
    
    if [ -f "$OUTPUT_FILE" ]; then
        echo "  ✅ SBOM erfolgreich generiert: $OUTPUT_FILE"
        
        # Zeige Anzahl der Komponenten
        COMPONENT_COUNT=$(jq '.components | length' "$OUTPUT_FILE" 2>/dev/null || echo "0")
        echo "  📦 Komponenten gefunden: $COMPONENT_COUNT"
    else
        echo "  ❌ Fehler beim Generieren der SBOM"
    fi
    
    cd "$PROJECT_ROOT"
done

echo ""
echo "=========================================="
echo "SBOM Generation abgeschlossen!"
echo "=========================================="
echo "Output-Verzeichnis: $SBOM_OUTPUT_DIR"
echo ""
echo "Nächste Schritte:"
echo "1. Überprüfe die generierten SBOM-Dateien in: $SBOM_OUTPUT_DIR"
echo "2. Lade die SBOMs in DependencyTrack hoch (http://localhost:8082)"
echo "3. Oder verwende das upload-sbom.sh Script für automatischen Upload"
echo ""


