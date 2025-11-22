#!/usr/bin/env python3
"""
SBOM Upload Script for DependencyTrack (Python)
Uploads all generated SBOMs to DependencyTrack
"""

import os
import sys
import json
import requests
from pathlib import Path

# Configuration
SCRIPT_DIR = Path(__file__).parent.absolute()
SBOM_OUTPUT_DIR = SCRIPT_DIR / "outputs"
DEPENDENCYTRACK_URL = os.environ.get("DEPENDENCYTRACK_URL", "http://localhost:8082")
DEPENDENCYTRACK_API_KEY = os.environ.get("DEPENDENCYTRACK_API_KEY", "odt_31eGzZ3i_nMTGh2qK3evWtTXZET6zNX2vysEcnYAb")

# Project mapping
PROJECT_MAPPING = {
    "BackendApi": "SIMS-BackendApi",
    "sims-api": "SIMS-UserApi",
    "sims-nosql-api": "SIMS-NoSqlApi",
    "sims-web_app": "SIMS-WebApp"
}

def main():
    print("=" * 42)
    print("SIMS SBOM Upload to DependencyTrack")
    print("=" * 42)
    print()
    
    # Check if SBOM output directory exists
    if not SBOM_OUTPUT_DIR.exists():
        print(f"❌ SBOM output directory not found: {SBOM_OUTPUT_DIR}")
        print("Please run generate-sbom.ps1 or generate-sbom.sh first.")
        sys.exit(1)
    
    # Check if API Key is set
    if not DEPENDENCYTRACK_API_KEY:
        print("⚠️  Warning: DEPENDENCYTRACK_API_KEY not set.")
        print()
        print("Please set the environment variable:")
        print("  export DEPENDENCYTRACK_API_KEY='your-api-key'")
        print()
        print("Or create an API Key in DependencyTrack:")
        print("  1. Open: http://localhost:8083 (Web-UI)")
        print("  2. Go to: Administration > Access Management > Teams > Automation")
        print("  3. Create a new API Key")
        print()
        response = input("Do you want to continue anyway? (y/n): ")
        if response.lower() not in ['y', 'yes']:
            sys.exit(1)
    
    # Check if DependencyTrack is reachable
    print("Checking DependencyTrack connection...")
    try:
        response = requests.get(f"{DEPENDENCYTRACK_URL}/api/version", timeout=5)
        response.raise_for_status()
        print("✅ DependencyTrack is reachable")
    except Exception as e:
        print(f"❌ DependencyTrack is not reachable at: {DEPENDENCYTRACK_URL}")
        print(f"Error: {e}")
        print("Please ensure DependencyTrack is running:")
        print("  docker-compose up -d dependencytrack dependencytrack-frontend dependencytrack-postgres")
        sys.exit(1)
    
    # Upload SBOMs
    sbom_files = list(SBOM_OUTPUT_DIR.glob("*-sbom.json"))
    
    if not sbom_files:
        print(f"❌ No SBOM files found in: {SBOM_OUTPUT_DIR}")
        sys.exit(1)
    
    for sbom_file in sbom_files:
        filename = sbom_file.name
        project_name = filename.replace("-sbom.json", "")
        dt_project_name = PROJECT_MAPPING.get(project_name, project_name)
        
        print()
        print(f"Uploading SBOM: {filename}")
        print(f"  Project: {project_name} -> {dt_project_name}")
        
        try:
            # Read SBOM file
            with open(sbom_file, 'rb') as f:
                sbom_data = f.read()
            
            # Prepare multipart/form-data
            files = {
                'bom': (filename, sbom_data, 'application/json')
            }
            
            data = {
                'projectName': dt_project_name,
                'projectVersion': '1.0.0',
                'autoCreate': 'true'
            }
            
            headers = {
                'X-Api-Key': DEPENDENCYTRACK_API_KEY
            }
            
            # Upload SBOM
            print("  Uploading SBOM...")
            # Try POST first (some DependencyTrack versions use POST instead of PUT)
            response = requests.post(
                f"{DEPENDENCYTRACK_URL}/api/v1/bom",
                headers=headers,
                data=data,
                files=files,
                timeout=120
            )
            
            response.raise_for_status()
            
            # Parse response
            if response.text:
                try:
                    result = response.json()
                    if 'token' in result:
                        print(f"  ✅ SBOM successfully uploaded (Token: {result['token']})")
                    else:
                        print(f"  ✅ SBOM upload initiated: {response.text}")
                except json.JSONDecodeError:
                    print(f"  ✅ SBOM upload initiated: {response.text}")
            else:
                print("  ✅ SBOM successfully uploaded")
                
        except requests.exceptions.HTTPError as e:
            print(f"  ❌ Error uploading: HTTP {e.response.status_code}")
            try:
                error_detail = e.response.json()
                print(f"  Response: {json.dumps(error_detail, indent=2)}")
            except:
                print(f"  Response: {e.response.text}")
        except Exception as e:
            print(f"  ❌ Error uploading: {e}")
    
    print()
    print("=" * 42)
    print("SBOM Upload completed!")
    print("=" * 42)
    print(f"DependencyTrack API URL: {DEPENDENCYTRACK_URL}")
    print(f"DependencyTrack Web UI: http://localhost:8083")
    print()

if __name__ == "__main__":
    main()

