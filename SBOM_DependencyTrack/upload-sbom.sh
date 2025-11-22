#!/bin/bash

# SBOM Upload Script for DependencyTrack
# Uploads all generated SBOMs to DependencyTrack

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SBOM_OUTPUT_DIR="$SCRIPT_DIR/outputs"

# DependencyTrack Configuration
DEPENDENCYTRACK_URL="${DEPENDENCYTRACK_URL:-http://localhost:8082}"
DEPENDENCYTRACK_WEB_URL="${DEPENDENCYTRACK_WEB_URL:-http://localhost:8083}"
# API Key - can be overridden by environment variable
DEPENDENCYTRACK_API_KEY="${DEPENDENCYTRACK_API_KEY:-odt_31eGzZ3i_nMTGh2qK3evWtTXZET6zNX2vysEcnYAb}"

echo "=========================================="
echo "SIMS SBOM Upload to DependencyTrack"
echo "=========================================="
echo ""

# Check if SBOM output directory exists
if [ ! -d "$SBOM_OUTPUT_DIR" ]; then
    echo "❌ SBOM output directory not found: $SBOM_OUTPUT_DIR"
    echo "Please run generate-sbom.sh first."
    exit 1
fi

# API Key is already set in the script, but can be overridden by environment variable
# No need to check if it's empty since we have a default value

# Check if DependencyTrack is reachable
echo "Checking DependencyTrack connection..."
if ! curl -s -f "$DEPENDENCYTRACK_URL/api/version" > /dev/null 2>&1; then
    echo "❌ DependencyTrack is not reachable at: $DEPENDENCYTRACK_URL"
    echo "Please ensure DependencyTrack is running:"
    echo "  docker-compose up -d dependencytrack dependencytrack-frontend dependencytrack-postgres"
    exit 1
fi
echo "✅ DependencyTrack is reachable"

# Projects and their DependencyTrack project names
declare -A PROJECT_MAPPING=(
    ["BackendApi"]="SIMS-BackendApi"
    ["sims-api"]="SIMS-UserApi"
    ["sims-nosql-api"]="SIMS-NoSqlApi"
    ["sims-web_app"]="SIMS-WebApp"
)

# Upload SBOMs
for SBOM_FILE in "$SBOM_OUTPUT_DIR"/*-sbom.json; do
    if [ ! -f "$SBOM_FILE" ]; then
        continue
    fi
    
    FILENAME=$(basename "$SBOM_FILE")
    PROJECT_NAME=$(echo "$FILENAME" | sed 's/-sbom\.json$//')
    DT_PROJECT_NAME="${PROJECT_MAPPING[$PROJECT_NAME]:-$PROJECT_NAME}"
    
    echo ""
    echo "Uploading SBOM: $FILENAME"
    echo "  Project: $PROJECT_NAME -> $DT_PROJECT_NAME"
    
    # Upload SBOM (autoCreate=true creates project automatically)
    echo "  Uploading SBOM..."
    # Use multipart/form-data with curl
    # Try POST first (some DependencyTrack versions use POST instead of PUT)
    HTTP_CODE=$(curl -s -o /tmp/upload_response.json -w "%{http_code}" -X POST \
        "$DEPENDENCYTRACK_URL/api/v1/bom" \
        -H "X-Api-Key: $DEPENDENCYTRACK_API_KEY" \
        -F "projectName=$DT_PROJECT_NAME" \
        -F "projectVersion=1.0.0" \
        -F "autoCreate=true" \
        -F "bom=@$SBOM_FILE")
    
    UPLOAD_RESULT=$(cat /tmp/upload_response.json 2>/dev/null || echo "")
    
    if [ "$HTTP_CODE" -ge 200 ] && [ "$HTTP_CODE" -lt 300 ]; then
        if [ -n "$UPLOAD_RESULT" ] && echo "$UPLOAD_RESULT" | jq -e '.token' > /dev/null 2>&1; then
            TOKEN=$(echo "$UPLOAD_RESULT" | jq -r '.token')
            echo "  ✅ SBOM successfully uploaded (Token: $TOKEN)"
        elif [ -n "$UPLOAD_RESULT" ]; then
            echo "  ✅ SBOM upload initiated"
            echo "  Response: $UPLOAD_RESULT"
        else
            echo "  ✅ SBOM successfully uploaded (HTTP $HTTP_CODE)"
        fi
    else
        echo "  ❌ Error uploading (HTTP $HTTP_CODE)"
        if [ -n "$UPLOAD_RESULT" ]; then
            echo "  Response: $UPLOAD_RESULT"
        fi
    fi
done

echo ""
echo "=========================================="
echo "SBOM Upload completed!"
echo "=========================================="
echo "DependencyTrack API URL: $DEPENDENCYTRACK_URL"
echo "DependencyTrack Web UI: $DEPENDENCYTRACK_WEB_URL"
echo ""

