# SIMS - Service Incident Management System

Ein umfassendes Service Incident Management System (SIMS) zur Verwaltung von Incidents, Kunden und Benutzern mit einer modernen Microservices-Architektur.

## 📋 Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [Systemarchitektur](#systemarchitektur)
- [Projektstruktur](#projektstruktur)
- [Technologien](#technologien)
- [API-Endpunkte](#api-endpunkte)
- [Datenbanken](#datenbanken)
- [Installation & Setup](#installation--setup)
- [Docker Deployment](#docker-deployment)
- [Features](#features)

## 🎯 Übersicht

SIMS ist ein vollständiges Incident Management System, das aus mehreren Microservices besteht:

- **Frontend**: Blazor Web App mit MudBlazor UI Framework
- **Backend APIs**: 
  - Incident API für Incident-Verwaltung
  - User API für Benutzerverwaltung mit Keycloak Integration
  - NoSQL API für Logging mit Redis
- **Datenbanken**:
  - PostgreSQL für Incidents, Kunden, Benutzer und Rollen
  - SQL Server für Incident-Daten (aktuell verwendet)
  - Redis für Logging-Daten

## 🏗️ Systemarchitektur

Die Systemarchitektur ist in der Datei `Flipcharts/Schema.drawio` bzw. `Flipcharts/Schema.png` dokumentiert.

### Architektur-Übersicht

```
┌─────────────┐
│  Frontend   │ (Blazor Web App - MudBlazor)
│  (Port 80)  │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────────────┐
│           Backend Services              │
├─────────────────────────────────────────┤
│  • IncidentApi (SQL Server)             │
│  • sims-api (Keycloak JWT Auth)         │
│  • sims-nosql-api (Redis Logging)       │
└──────┬──────────────────────────────────┘
       │
       ├──► PostgreSQL (Keycloak DB)
       ├──► SQL Server (Incidents DB)
       └──► Redis (Logging DB)
```

### Frontend-Routen

- `/` - Login-Seite und Dashboard
- `/incidents` - Incident-Übersicht und Liste
- `/incidents/[incId]` - Incident-Detailansicht
- `/settings` - Benutzereinstellungen (nur Admins)
- `/customers` - Kundenverwaltung (nur Admins)

## 📁 Projektstruktur

```
simsfh_ws25/
├── IncidentApi/                    # Incident Management API
│   ├── Data/
│   │   ├── Database/
│   │   │   └── ApiDbContext.cs     # Entity Framework Context
│   │   ├── Model/
│   │   │   ├── Enum/               # IncidentStatus, Severity, Type
│   │   │   └── Incident/           # Incident & IncidentComment Models
│   │   └── Services/
│   │       └── IncidentService.cs  # Business Logic
│   ├── Endpoints/
│   │   └── MapApiEndpoints.cs      # API Endpoint Definitions
│   └── Program.cs
│
├── sims-api/                       # User Management API mit Keycloak
│   ├── Controllers/
│   │   └── UserController.cs       # User Endpoints
│   ├── docker-compose.yaml         # Keycloak + PostgreSQL Setup
│   └── Program.cs
│
├── sims-nosql-api/                 # Redis Logging API
│   ├── Controller/
│   │   └── RedisController.cs      # Redis Endpoints
│   ├── Database/
│   │   └── RedisConnection.cs      # Redis Connection Management
│   ├── Services/
│   │   └── RedisService.cs         # Redis Business Logic
│   ├── docker-compose.yaml         # Redis Setup
│   └── Program.cs
│
├── sims-web_app/                   # Blazor Frontend
│   ├── Components/
│   │   ├── Account/                # Authentication Components
│   │   ├── Incident/               # Incident Components
│   │   ├── Layout/                 # Layout Components
│   │   └── Pages/                  # Page Components
│   ├── Data/
│   │   ├── Models/                 # Frontend Models
│   │   └── ApplicationDbContext.cs # Identity DB Context
│   └── Program.cs
│
├── IncidentApi.Tests/              # Unit Tests
│   └── IncidentEndpointsTest.cs
│
├── Flipcharts/
│   ├── Schema.drawio               # Systemarchitektur Diagramm
│   └── Schema.png                  # Systemarchitektur Bild
│
└── sims.sln                        # Visual Studio Solution
```

## 🛠️ Technologien

### Frontend
- **.NET 9.0** - Blazor Server
- **MudBlazor 8.x** - UI Component Library
- **ASP.NET Core Identity** - Benutzerauthentifizierung

### Backend
- **.NET 9.0** - IncidentApi
- **.NET 8.0** - sims-api, sims-nosql-api
- **Entity Framework Core 9.0** - ORM für SQL Server
- **Swagger/OpenAPI** - API Dokumentation

### Datenbanken
- **SQL Server** - Incident-Daten (Entity Framework)
- **PostgreSQL 16** - Keycloak Datenbank (Users & Roles)
- **Redis** - NoSQL Logging-Datenbank

### Authentication & Authorization
- **Keycloak 26.4.0** - Identity and Access Management
- **JWT Bearer Tokens** - Token-basierte Authentifizierung

### Containerisierung
- **Docker** - Container-Orchestrierung
- **Docker Compose** - Multi-Container Setup

## 🔌 API-Endpunkte

### Incident API (`/api/incidents`)

| Methode | Endpunkt | Beschreibung |
|---------|----------|--------------|
| GET | `/api/incidents` | Alle Incidents abrufen |
| GET | `/api/incidents/{id}` | Incident-Details abrufen |
| POST | `/api/incidents` | Neues Incident erstellen |
| PUT | `/api/incidents/{id}` | Incident aktualisieren |
| DELETE | `/api/incidents/{id}` | Incident löschen |

**Beispiel-Request (POST):**
```json
{
  "summary": "System outage detected",
  "description": "Unexpected system failure",
  "severity": 2,
  "incidentType": 0,
  "assignedPerson": "Max Mustermann",
  "author": "Alice"
}
```

### User API (`/api/user`)

| Methode | Endpunkt | Beschreibung | Authentifizierung |
|---------|----------|--------------|-------------------|
| GET | `/api/user/public` | Öffentlicher Endpunkt | ❌ |
| GET | `/api/user/secure` | Geschützter Endpunkt | ✅ JWT Token |

### Redis Logging API (`/api/redis`)

| Methode | Endpunkt | Beschreibung |
|---------|----------|--------------|
| POST | `/api/redis/set?key={key}&value={value}` | Wert in Redis speichern |
| GET | `/api/redis/get?key={key}` | Wert aus Redis abrufen |
| GET | `/api/redis/all` | Alle Werte abrufen |

## 💾 Datenbanken

### SQL Server - Incidents Schema

#### Tabelle: `Incidents`
| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| `IncidentId` | string (PK) | Eindeutige Incident-ID (Format: INC-0001) |
| `IncidentUUid` | Guid | Globally Unique Identifier |
| `Summary` | string | Kurzbeschreibung |
| `Description` | string | Detaillierte Beschreibung |
| `CreateDate` | DateTime | Erstellungsdatum |
| `UpdateDate` | DateTime? | Letztes Update |
| `ClosedDate` | DateTime? | Schließungsdatum |
| `Status` | IncidentStatus | Status (Open, onProgress, onHold, Aborted, Solved) |
| `IncidentType` | IncidentType | Typ (SecurityIncident) |
| `Severity` | IncidentSeverity | Priorität (Low, Medium, High, Critical) |
| `AssignedPerson` | string | Zugewiesene Person |
| `Author` | string | Ersteller |

#### Tabelle: `IncidentComments`
| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| `CommentId` | Guid (PK) | Eindeutige Kommentar-ID |
| `IncidentId` | string (FK) | Referenz zum Incident |
| `UserId` | Guid | Benutzer-ID |
| `UserName` | string | Benutzername |
| `Comment` | string | Kommentar-Text |
| `CreateDate` | DateTime | Erstellungsdatum |
| `CommentOrder` | int | Reihenfolge |

### PostgreSQL - Keycloak Schema

#### Tabelle: `Users`
| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| `uid` | int (PK) | Primärschlüssel |
| `Firstname` | string | Vorname |
| `Lastname` | string | Nachname |
| `Email` | string | E-Mail-Adresse |
| `RoleId` | int (FK) | Referenz zur Rolle |
| `PasswordHash` | string | Passwort-Hash |

#### Tabelle: `Roles`
| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| `RoleId` | int (PK) | Primärschlüssel |
| `RoleName` | string | Rollenname |
| `Description` | string | Beschreibung |

### Redis - Logging Schema

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `logId` | int | Log-ID |
| `message` | string | Log-Nachricht |

## 🚀 Installation & Setup

### Voraussetzungen

- .NET 9.0 SDK
- .NET 8.0 SDK
- Docker Desktop
- SQL Server (oder Docker Container)
- Visual Studio 2022 oder VS Code

### Lokale Entwicklung

1. **Repository klonen**
   ```bash
   git clone <repository-url>
   cd simsfh_ws25
   ```

2. **Datenbankverbindungen konfigurieren**

   **IncidentApi/appsettings.json:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=SIMS_Incidents;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Keycloak & PostgreSQL starten**
   ```bash
   cd sims-api
   docker-compose up -d
   ```

4. **Redis starten**
   ```bash
   cd sims-nosql-api
   docker-compose up -d
   ```

5. **Datenbank-Migrationen ausführen**
   ```bash
   cd IncidentApi
   dotnet ef database update
   ```

6. **Anwendungen starten**

   **Terminal 1 - IncidentApi:**
   ```bash
   cd IncidentApi
   dotnet run
   ```
   API verfügbar unter: `https://localhost:5001` oder `http://localhost:5000`

   **Terminal 2 - sims-api:**
   ```bash
   cd sims-api
   dotnet run
   ```

   **Terminal 3 - sims-nosql-api:**
   ```bash
   cd sims-nosql-api
   dotnet run
   ```

   **Terminal 4 - Frontend:**
   ```bash
   cd sims-web_app
   dotnet run
   ```
   Frontend verfügbar unter: `https://localhost:5001` oder `http://localhost:5000`

## 🐳 Docker Deployment

### IncidentApi

Die IncidentApi kann über Docker deployt werden. Stelle sicher, dass eine SQL Server Datenbank verfügbar ist.

### sims-api mit Keycloak

```bash
cd sims-api
docker-compose up -d
```

Dies startet:
- PostgreSQL Container (Port 5432)
- Keycloak Container (Port 8080)
- User API Container (Port 5000)

**Keycloak Admin-Zugang:**
- URL: http://localhost:8080
- Username: `admin`
- Password: `admin`
- Realm: `IncidentSystem`

### sims-nosql-api mit Redis

```bash
cd sims-nosql-api
docker-compose up -d
```

Dies startet:
- Redis Container (Port 6379)
- NoSQL API Container (Port 8081)

## ✨ Features

### Incident Management
- ✅ CRUD-Operationen für Incidents
- ✅ Incident-Status-Verwaltung (Open, onProgress, onHold, Aborted, Solved)
- ✅ Prioritätsverwaltung (Low, Medium, High, Critical)
- ✅ Automatische Incident-ID-Generierung (INC-0001, INC-0002, ...)
- ✅ Kommentar-System für Incidents
- ✅ Zuweisung von Incidents an Personen
- ✅ Zeitstempel (Erstellung, Update, Schließung)

### Benutzerverwaltung
- ✅ Keycloak Integration für Identity Management
- ✅ JWT-basierte Authentifizierung
- ✅ Rollenbasierte Zugriffskontrolle
- ✅ Benutzer-Registrierung und -Verwaltung

### Logging
- ✅ Redis-basierte Logging-Infrastruktur
- ✅ Key-Value Speicherung von Log-Nachrichten
- ✅ RESTful API für Log-Zugriff

### Frontend
- ✅ Moderne Blazor Web App mit MudBlazor
- ✅ Responsive Design
- ✅ Incident-Übersicht mit interaktiver Tabelle
- ✅ Authentifizierung und Autorisierung
- ✅ Benutzerfreundliche Navigation

### API-Dokumentation
- ✅ Swagger/OpenAPI Integration
- ✅ Automatische API-Dokumentation
- ✅ Interaktive API-Tests über Swagger UI

## 🧪 Testing

Unit Tests sind im Projekt `IncidentApi.Tests` enthalten:

```bash
cd IncidentApi.Tests
dotnet test
```

## 👥 Autoren

Dieses Projekt wurde im Rahmen des SW-AC (Software Architecture) Kurses an der **University of Applied Sciences St. Pölten** entwickelt.

**Projektteam:**
- Grabner Gabriel
- Macala Konatsu
- Mühlparzer Philipp
- Puschnig Margarethe
- Radler Maximilian


## 📝 Weitere Informationen

- **Architektur-Diagramm**: Siehe `Flipcharts/Schema.drawio` oder `Flipcharts/Schema.png`
- **API-Dokumentation**: Swagger UI unter `/swagger` (Development-Modus)
- **Projekt-Spezifikation**: Siehe `SIMS.pdf` - Die Systemarchitektur und Anforderungen basieren auf den Angaben in dieser Datei

## 📄 Lizenz

[Lizenz-Informationen hier einfügen]

---

**Hinweis**: Dieses System ist für Entwicklungs- und Lernzwecke konzipiert. Für Produktionsumgebungen sollten zusätzliche Sicherheitsmaßnahmen implementiert werden.
