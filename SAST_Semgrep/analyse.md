# Analyse der Semgrep Vulnerability Scan Ergebnisse

## Übersicht

Diese Analyse basiert auf den Ergebnissen eines Semgrep Security Scans, der am **19. November 2025** durchgeführt wurde. Insgesamt wurden **4 Vulnerabilities** gefunden, davon **2 True Positives** (echte Sicherheitsprobleme) und **2 False Positives** (falsche Alarmierungen).

### Zusammenfassung

| ID | Regel | Schweregrad | Status | Konfidenz | Komponente |
|----|-------|-------------|--------|-----------|------------|
| Vul1 | detect-non-literal-regexp | Medium | False Positive | Low | sims-nosql-api |
| Vul2 | ssrf | High | False Positive | Low | sims-web_app |
| Vul3 | missing-user-entrypoint | High | **True Positive** | Medium | BackendApi |
| Vul4 | missing-user-entrypoint | High | **True Positive** | Medium | sims-nosql-api |

---

## Detaillierte Analyse

### Vul1: ReDoS in jQuery Validation Library (False Positive)

**ID:** #300175655  
**Regel:** `detect-non-literal-regexp`  
**Schweregrad:** Medium  
**CWE:** CWE-1333  
**Datei:** `sims-nosql-api/wwwroot/lib/.../jquery.validate.unobtrusive.js:349`  
**Status:** ⚠️ False Positive (laut Semgrep Assistant)  
**Konfidenz:** Low

#### Beschreibung
Die Regel erkennt, dass ein RegExp direkt aus dem `params`-Argument konstruiert wird, welches von HTML-Attributen stammt. Theoretisch könnte ein Angreifer einen bösartigen Regex-Pattern in das `data-val-regex-pattern` HTML-Attribut injizieren, um einen ReDoS-Angriff (Regular Expression Denial-of-Service) auszulösen.

#### Warum False Positive?
- Die betroffene Datei ist eine **Third-Party jQuery Validation Library** (`jquery.validate.unobtrusive.js`) im `wwwroot/lib` Verzeichnis
- Das `params`-Argument stammt von Validierungsattributen, die von Entwicklern definiert werden, nicht von direktem Benutzereingaben
- Dies ist **client-side Validierung**, bei der ReDoS nur den Browser des Benutzers selbst betreffen würde, nicht den Server oder andere Benutzer
- Daher ist das Sicherheitsrisiko als niedrig einzustufen

#### Empfehlung
✅ **Keine Maßnahme erforderlich** - Dies ist ein False Positive. Die Library wird für client-side Validierung verwendet und stellt kein Server-seitiges Sicherheitsrisiko dar.

---

### Vul2: SSRF in BackendApiHandler (False Positive)

**ID:** #300175658  
**Regel:** `ssrf`  
**Schweregrad:** High  
**CWE:** CWE-918  
**Datei:** `sims-web_app/Services/BackendApiHandler.cs:93`  
**Status:** ⚠️ False Positive (laut Semgrep Assistant)  
**Konfidenz:** Low

#### Beschreibung
Die Methode `GetIncidentById(string id)` erstellt einen `RestClient` mit einer URL, die direkt das `id`-Parameter einbettet: `new RestClient(baseUrl + $"/Incidents/{id}")`. Die Regel warnt, dass ein Angreifer dadurch den Server-seitigen Request-Pfad kontrollieren könnte.

#### Warum False Positive?
- Der `RestClient` wird mit einer konstanten `baseUrl` aus einer Umgebungsvariable konstruiert, die mit einem festen Pfad-String konkateniert wird
- Der `id`-Parameter wird **nur innerhalb des URL-Pfad-Teils** verwendet (kontrolliert nicht den Host/Domain)
- Dies ist ein **normales API-Nutzungsmuster** und keine SSRF-Schwachstelle
- Die `baseUrl` ist fest definiert und der `id` kann nur den Pfad innerhalb dieser Base-URL beeinflussen

#### Empfehlung
✅ **Keine Maßnahme erforderlich** - Dies ist ein False Positive. Die Implementierung folgt einem sicheren API-Nutzungsmuster.

---

### Vul3: Container läuft als Root - BackendApi (True Positive)

**ID:** #300175657  
**Regel:** `missing-user-entrypoint`  
**Schweregrad:** **High**  
**CWE:** CWE-269  
**Datei:** `BackendApi/Dockerfile:27`  
**Status:** ✅ **True Positive** (laut Semgrep Assistant)  
**Konfidenz:** Medium

#### Beschreibung
Der Container-Entrypoint läuft ohne vorherige `USER`-Direktive, was bedeutet, dass der dotnet-Prozess als **root-Benutzer** innerhalb des Containers ausgeführt wird.

#### Sicherheitsrisiko
Wenn ein Angreifer Code-Ausführung innerhalb des Containers erlangt (z.B. über eine Schwachstelle in der ASP.NET-Anwendung oder einer Dependency), hat er **root-Berechtigungen** innerhalb des Containers. Dies ermöglicht:
- Zugriff auf sensible Dateien
- Modifikation des Anwendungsverhaltens
- Potenzielle Container-Escape-Versuche
- Angriffe auf andere Container auf demselben Host

#### Betroffener Code
```dockerfile
# Stage 4: Final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8888
EXPOSE 8881
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "BackendApi.dll"]  # ❌ Läuft als root
```

#### Empfohlene Lösung
```dockerfile
# Stage 4: Final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8888
EXPOSE 8881
COPY --from=publish /app/publish ./

# ✅ Non-root User erstellen und verwenden
RUN useradd -m appuser
RUN chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "BackendApi.dll"]
```

#### Empfehlung
🔴 **Sofortige Maßnahme erforderlich** - Dies ist eine echte Sicherheitsschwachstelle. Die Anwendung sollte als non-root User laufen, um die Auswirkungen einer möglichen Kompromittierung zu reduzieren.

---

### Vul4: Container läuft als Root - sims-nosql-api (True Positive)

**ID:** #300175656  
**Regel:** `missing-user-entrypoint`  
**Schweregrad:** **High**  
**CWE:** CWE-269  
**Datei:** `sims-nosql-api/Dockerfile:29`  
**Status:** ✅ **True Positive** (laut Semgrep Assistant)  
**Konfidenz:** Medium

#### Beschreibung
Der Container-Entrypoint führt die .NET-Anwendung aus, ohne zuvor einen non-root `USER` im Dockerfile zu spezifizieren. Der Container führt den `dotnet sims-nosql-api.dll` Prozess standardmäßig als **root** aus.

#### Sicherheitsrisiko
Wenn ein Angreifer eine Schwachstelle in der .NET-Anwendung entdeckt (z.B. Remote Code Execution in einem Web-Endpoint auf Port 8080), erhält er **Shell-Zugriff auf den Container als root-Benutzer**. Mit root-Zugriff kann der Angreifer:
- Alle Dateien im Container ohne Einschränkungen lesen
- Anwendungsdateien und Konfigurationen modifizieren oder löschen
- Auf Secrets oder Umgebungsvariablen zugreifen
- Aus dem Container ausbrechen, um das Host-System oder andere Container anzugreifen

#### Betroffener Code
```dockerfile
# • Finale Laufzeitstufe
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "sims-nosql-api.dll"]  # ❌ Läuft als root
```

#### Empfohlene Lösung
```dockerfile
# • Finale Laufzeitstufe
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# ✅ Non-root User erstellen und verwenden
RUN adduser --disabled-password --no-create-home appuser
USER appuser

ENTRYPOINT ["dotnet", "sims-nosql-api.dll"]
```

**Hinweis:** Falls das Image auf Alpine Linux basiert, verwenden Sie stattdessen: `RUN adduser -D -H appuser`

#### Empfehlung
🔴 **Sofortige Maßnahme erforderlich** - Dies ist eine echte Sicherheitsschwachstelle. Die Anwendung sollte als non-root User laufen, um das Risiko einer kompromittierten Anwendung zu reduzieren.

---

## Zusammenfassung der Handlungsempfehlungen

### Sofortige Maßnahmen (High Priority)

1. **BackendApi/Dockerfile** - Non-root User hinzufügen
   - Erstellen Sie einen non-root User (`appuser`)
   - Setzen Sie die Dateiberechtigungen mit `chown`
   - Verwenden Sie `USER appuser` vor dem `ENTRYPOINT`

2. **sims-nosql-api/Dockerfile** - Non-root User hinzufügen
   - Erstellen Sie einen non-root User (`appuser`)
   - Verwenden Sie `USER appuser` vor dem `ENTRYPOINT`

### Keine Maßnahmen erforderlich

- **Vul1 (ReDoS)**: False Positive - Client-side Library, kein Server-Risiko
- **Vul2 (SSRF)**: False Positive - Normale API-Nutzung mit fester baseUrl

---

## Metriken

- **Gesamtanzahl Findings:** 4
- **True Positives:** 2 (50%)
- **False Positives:** 2 (50%)
- **High Severity:** 3 (davon 2 True Positives)
- **Medium Severity:** 1 (False Positive)
- **Betroffene Komponenten:**
  - BackendApi (1 True Positive)
  - sims-nosql-api (1 True Positive, 1 False Positive)
  - sims-web_app (1 False Positive)

---

## Nächste Schritte

1. ✅ **Dockerfile-Korrekturen implementieren** für BackendApi und sims-nosql-api
2. ✅ **Container neu bauen und testen** nach den Änderungen
3. ✅ **Security Scan erneut durchführen** um zu verifizieren, dass die Issues behoben sind
4. ✅ **Container-Images aktualisieren** in der CI/CD-Pipeline

---

## Scan-Details

- **Scan-Datum:** 19. November 2025
- **Tool:** Semgrep
- **Scan-Typ:** Managed Scan
- **Repository:** is241307/simsfh_ws25_SAST
- **Branch:** main
- **Commit:** 5cef08519

---

*Diese Analyse wurde automatisch unter Verwendung von KI aus den Semgrep Scan-Ergebnissen generiert.*

