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
- [Security Features](#security-features)
- [Aktuelle ToDos](#aktuelle-todos)

## 🎯 Übersicht

SIMS ist ein vollständiges Incident Management System, das aus mehreren Microservices besteht:

- **Frontend**: Blazor Web App mit MudBlazor UI Framework
- **Backend APIs**: 
  - **BackendApi** - Incident & Customer Management API (SQL Server)
  - **sims-api** - User Management API mit JWT Authentication (PostgreSQL)
  - **sims-nosql-api** - Redis Logging API (Redis)
- **Datenbanken**:
  - **SQL Server** - Incident- und Customer-Daten (Entity Framework Core)
  - **PostgreSQL** - User- und Role-Daten
  - **Redis** - NoSQL Logging-Datenbank

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
│  • BackendApi (Port 5001)                │
│    - Incidents & Customers (SQL Server) │
│    - Logs (Redis Integration)           │
│  • sims-api (Port 5000)                  │
│    - User Management (PostgreSQL)       │
│    - JWT Authentication                  │
│  • sims-nosql-api (Port 8081)            │
│    - Redis Logging API                   │
└──────┬──────────────────────────────────┘
       │
       ├──► PostgreSQL (User & Role DB)
       ├──► SQL Server (Incident & Customer DB)
       └──► Redis (Logging DB)
```

### Frontend-Routen

- `/` - Login-Seite und Dashboard
- `/incidents` - Incident-Übersicht und Liste
- `/incidents/[incId]` - Incident-Detailansicht
- `/customers` - Kundenverwaltung (nur Admins)
- `/users` - Benutzerverwaltung (nur Admins)
- `/logs` - Log-Übersicht (NoSQL)

## 📁 Projektstruktur

```
simsfh_ws25/
├── BackendApi/                     # Incident & Customer Management API
│   ├── Controllers/
│   │   ├── IncidentsController.cs  # Incident CRUD Endpoints
│   │   ├── CustomersController.cs   # Customer CRUD Endpoints
│   │   └── LogsController.cs        # Log Retrieval Endpoints
│   ├── Data/
│   │   ├── Database/
│   │   │   └── MsSqlDbContext.cs    # Entity Framework Context
│   │   ├── Model/
│   │   │   ├── Customer/           # Customer Models
│   │   │   ├── Enum/               # IncidentStatus, Severity, Type
│   │   │   ├── Incident/           # Incident & IncidentComment Models
│   │   │   └── LogData/            # LogEntry Models
│   │   └── Services/
│   │       └── GenerateIdService.cs # ID Generation Logic
│   ├── Migrations/                  # Entity Framework Migrations
│   ├── RedisLoggingMiddleware.cs   # Middleware für Redis Logging
│   ├── compose.yaml                # Docker Compose für BackendApi + SQL Server + Redis
│   ├── Dockerfile
│   └── Program.cs
│
├── sims-api/                        # User Management API
│   ├── Controllers/
│   │   ├── AuthController.cs       # Authentication Endpoints
│   │   └── UsersController.cs      # User CRUD Endpoints
│   ├── Data/
│   │   └── UserDbContext.cs        # PostgreSQL EF Context
│   ├── Models/
│   │   ├── User.cs                 # User Model
│   │   ├── Role.cs                 # Role Model
│   │   └── BlacklistedToken.cs     # Token Blacklist Model
│   ├── Services/
│   │   ├── EventLogger.cs          # Event Logging Service
│   │   └── IEventLogger.cs        # Event Logger Interface
│   ├── Migrations/                  # Entity Framework Migrations
│   ├── docker-compose.yaml         # PostgreSQL + User API Setup
│   ├── Dockerfile
│   └── Program.cs
│
├── sims-nosql-api/                  # Redis Logging API
│   ├── Controller/
│   │   └── RedisController.cs      # Redis Log Endpoints
│   ├── Database/
│   │   └── RedisConnection.cs      # Redis Connection Management
│   ├── Services/
│   │   ├── LogEntry.cs             # Log Entry Model
│   │   └── RedisService.cs         # Redis Business Logic
│   ├── docker-compose.yaml         # Redis + NoSQL API Setup
│   ├── Dockerfile
│   └── Program.cs
│
├── sims-web_app/                    # Blazor Frontend
│   ├── Components/
│   │   ├── Identity/               # Authentication Components
│   │   ├── Layout/                 # Layout Components
│   │   └── Pages/                  # Page Components
│   │       ├── Account/            # Login Pages
│   │       ├── Customer/           # Customer Management Pages
│   │       ├── Incident/           # Incident Management Pages
│   │       ├── LogPage/            # Log Overview Pages
│   │       └── User/               # User Management Pages
│   ├── Data/
│   │   └── Model/                  # Frontend Models
│   ├── Services/
│   │   ├── AuthApiHandler.cs       # Authentication API Handler
│   │   └── BackendApiHandler.cs    # Backend API Handler
│   └── Program.cs
│
├── BackendApi.Tests/                # Unit Tests
│   ├── CustomerEndpointsTest.cs
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
- **.NET 9.0** - BackendApi
- **.NET 8.0** - sims-api, sims-nosql-api
- **Entity Framework Core 9.0** - ORM für SQL Server & PostgreSQL
- **StackExchange.Redis** - Redis Client Library
- **Swagger/OpenAPI** - API Dokumentation
- **BCrypt.Net** - Password Hashing

### Datenbanken
- **SQL Server** - Incident-Daten (Entity Framework)
- **PostgreSQL 16** - Keycloak Datenbank (Users & Roles)
- **Redis** - NoSQL Logging-Datenbank

### Authentication & Authorization
- **JWT Bearer Tokens** - Token-basierte Authentifizierung
- **Token Blacklisting** - Token-Invalidierung bei Logout
- **Rollenbasierte Zugriffskontrolle** - Admin/User Rollen

### Containerisierung
- **Docker** - Container-Orchestrierung
- **Docker Compose** - Multi-Container Setup

## 🔌 API-Endpunkte

### BackendApi (`http://localhost:5001/api/v1`)

Die BackendApi ist die Haupt-API für das Incident Management System. Sie verwaltet Incidents, Kunden und bietet Zugriff auf Logs.

**API-Basis-URL:** `http://localhost:5001/api/v1`

#### Incidents API

| Methode | Endpunkt | Beschreibung | Response Codes |
|---------|----------|--------------|----------------|
| GET | `/api/v1/incidents` | Alle Incidents abrufen | 200 OK |
| GET | `/api/v1/incidents/{id}` | Incident-Details abrufen | 200 OK, 404 Not Found |
| POST | `/api/v1/incidents` | Neues Incident erstellen | 201 Created, 400 Bad Request |
| PUT | `/api/v1/incidents/{id}` | Incident aktualisieren | 200 OK, 404 Not Found, 400 Bad Request |
| DELETE | `/api/v1/incidents/{id}` | Incident löschen | 204 No Content, 404 Not Found |

**Beispiel-Request (POST /api/v1/incidents):**
```json
{
  "summary": "System outage detected",
  "description": "Unexpected system failure in production environment",
  "severity": 2,
  "incidentType": 0,
  "assignedPerson": "Max Mustermann",
  "customerId": "CUST-0001",
  "status": 0
}
```

**Beispiel-Response (POST):**
```json
{
  "id": "INC-0001",
  "uUId": "550e8400-e29b-41d4-a716-446655440000",
  "summary": "System outage detected",
  "description": "Unexpected system failure in production environment",
  "createDate": "2025-02-07T12:00:00",
  "changeDate": "2025-02-07T12:00:00",
  "closedDate": null,
  "status": 0,
  "incidentType": 0,
  "severity": 2,
  "assignedPerson": "Max Mustermann",
  "customerId": "CUST-0001",
  "comments": []
}
```

**Beispiel-Request (PUT /api/v1/incidents/INC-0001):**
```json
{
  "summary": "System outage detected - RESOLVED",
  "status": 4,
  "closedDate": "2025-02-07T14:00:00"
}
```

**Hinweis:** Bei PUT-Requests werden nur die übergebenen Felder aktualisiert. Die Incident-ID wird automatisch generiert (Format: INC-0001, INC-0002, ...).

#### Customers API

| Methode | Endpunkt | Beschreibung | Response Codes |
|---------|----------|--------------|----------------|
| GET | `/api/v1/customers` | Alle Kunden abrufen | 200 OK |
| GET | `/api/v1/customers/{id}` | Kunde nach ID abrufen | 200 OK, 404 Not Found |
| POST | `/api/v1/customers` | Neuen Kunden erstellen | 201 Created, 400 Bad Request |
| PUT | `/api/v1/customers/{id}` | Kunde aktualisieren | 200 OK, 400 Bad Request |
| DELETE | `/api/v1/customers/{id}` | Kunde löschen | 204 No Content, 404 Not Found |

**Beispiel-Request (POST /api/v1/customers):**
```json
{
  "companyName": "Beispiel GmbH",
  "email": "info@beispiel.de",
  "phoneNumber": "+43 123 456789",
  "address": "Musterstraße 1",
  "city": "Wien",
  "state": "Wien",
  "zipCode": "1010",
  "country": "Österreich",
  "active": true
}
```

**Beispiel-Response (POST):**
```json
{
  "id": "CUST-0001",
  "uUId": "660e8400-e29b-41d4-a716-446655440000",
  "companyName": "Beispiel GmbH",
  "email": "info@beispiel.de",
  "phoneNumber": "+43 123 456789",
  "address": "Musterstraße 1",
  "city": "Wien",
  "state": "Wien",
  "zipCode": "1010",
  "country": "Österreich",
  "active": true,
  "createDate": "2025-02-07T12:00:00",
  "changeDate": "2025-02-07T12:00:00"
}
```

**Validierungsregeln:**
- `companyName`: Muss vorhanden sein, darf nicht leer sein
- `email`: Muss vorhanden sein, muss gültiges E-Mail-Format haben

**Hinweis:** Die Kunden-ID wird automatisch generiert (Format: CUST-0001, CUST-0002, ...).

#### Logs API

| Methode | Endpunkt | Beschreibung | Response Codes |
|---------|----------|--------------|----------------|
| GET | `/api/v1/logs` | Logs abrufen (optional: `?count=50`) | 200 OK |
| GET | `/api/v1/logs/{id}` | Log nach ID abrufen | 200 OK, 404 Not Found |

**Beispiel-Request (GET /api/v1/logs?count=10):**
```bash
curl -X GET "http://localhost:5001/api/v1/logs?count=10"
```

**Beispiel-Response:**
```json
[
  {
    "id": "log-001",
    "timestamp": "2025-02-07T12:00:00",
    "message": "API request received",
    "severity": 1
  },
  {
    "id": "log-002",
    "timestamp": "2025-02-07T12:01:00",
    "message": "Database connection established",
    "severity": 1
  }
]
```

**Hinweis:** Die Logs werden aus Redis abgerufen. Die BackendApi verwendet automatisches Logging via Middleware.

### sims-api (`http://localhost:5000/api`)

Die sims-api verwaltet Benutzer und bietet JWT-basierte Authentifizierung. Alle Endpunkte (außer Login) erfordern ein gültiges JWT Token im Authorization Header.

**API-Basis-URL:** `http://localhost:5000/api`

**Authentifizierung:**
Alle geschützten Endpunkte erfordern einen JWT Token im Header:
```
Authorization: Bearer <token>
```

#### Authentication API

| Methode | Endpunkt | Beschreibung | Authentifizierung | Response Codes |
|---------|----------|--------------|-------------------|----------------|
| POST | `/api/auth/login` | Benutzer anmelden | ❌ | 200 OK, 401 Unauthorized |
| POST | `/api/auth/logout` | Benutzer abmelden | ✅ JWT Token | 200 OK, 400 Bad Request |

**Beispiel-Request (POST /api/auth/login):**
```json
{
  "email": "user@example.com",
  "password": "securePassword123"
}
```

**Beispiel-Response (POST /api/auth/login):**
```json
{
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "uid": 1,
  "firstname": "Max",
  "lastname": "Mustermann",
  "role": "Admin"
}
```

**Beispiel-Request (POST /api/auth/logout):**
```bash
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Beispiel-Response (POST /api/auth/logout):**
```json
{
  "message": "Logout successful. Token invalidated."
}
```

**Hinweis:** Nach dem Logout wird der Token in die Blacklist aufgenommen und kann nicht mehr verwendet werden.

#### Users API

| Methode | Endpunkt | Beschreibung | Authentifizierung | Response Codes |
|---------|----------|--------------|-------------------|----------------|
| POST | `/api/users/create` | Neuen Benutzer erstellen | ✅ Admin | 200 OK, 400 Bad Request |
| PUT | `/api/users/modify?uid={uid}` | Benutzer aktualisieren | ✅ Admin | 200 OK, 400 Bad Request, 404 Not Found |
| DELETE | `/api/users/delete?uid={uid}` | Benutzer löschen | ✅ Admin | 200 OK, 404 Not Found |
| GET | `/api/users/getAll` | Alle Benutzer abrufen | ✅ Admin | 200 OK |
| GET | `/api/users/getCurrent` | Aktuellen Benutzer abrufen | ✅ JWT Token | 200 OK |

**Beispiel-Request (POST /api/users/create):**
```json
{
  "firstname": "Anna",
  "lastname": "Schmidt",
  "email": "anna.schmidt@example.com",
  "roleId": 2,
  "password": "securePassword123"
}
```

**Beispiel-Response (POST /api/users/create):**
```json
{
  "message": "User successfully created.",
  "uid": 5
}
```

**Beispiel-Request (PUT /api/users/modify?uid=5):**
```json
{
  "firstname": "Anna Maria",
  "lastname": "Schmidt",
  "email": "anna.schmidt@example.com",
  "roleId": 1,
  "password": "newSecurePassword123"
}
```

**Beispiel-Response (PUT):**
```json
{
  "message": "User successfully updated.",
  "uid": 5
}
```

**Hinweis:** Bei PUT-Requests sind alle Felder optional. Nur die übergebenen Felder werden aktualisiert.

**Beispiel-Request (GET /api/users/getAll):**
```bash
curl -X GET http://user-api:5000/api/users/getAll \
  -H "Authorization: Bearer <token>"
```

**Hinweis:** Bei diversen APIs ist auch die Admin-Rolle vorausgesetzt, die im Token beinhaltet ist.

**Beispiel-Response (GET /api/users/getAll):**
```json
[
  {
    "uid": 1,
    "firstname": "Max",
    "lastname": "Mustermann",
    "email": "max.mustermann@example.com",
    "role": "Admin"
  },
  {
    "uid": 2,
    "firstname": "Anna",
    "lastname": "Schmidt",
    "email": "anna.schmidt@example.com",
    "role": "User"
  }
]
```

**Beispiel-Request (GET /api/users/getCurrent):**
```bash
curl -X GET http://user-api:5000/api/users/getCurrent \
  -H "Authorization: Bearer <token>"
```

**Beispiel-Response (GET /api/users/getCurrent):**
```json
{
  "userId": "1",
  "email": "max.mustermann@example.com",
  "role": "Admin"
}
```

**Validierungsregeln:**
- `email`: Muss eindeutig sein, gültiges E-Mail-Format
- `password`: Wird durch BCrypt mit Salt gehasht gespeichert
- `roleId`: Muss eine gültige Rolle referenzieren (1 = Admin, 2 = User)

### sims-nosql-api (`http://localhost:8081/api`)

#### Redis Logging API

Die sims-nosql-api ist eine spezialisierte API für das Logging von Systemereignissen in Redis. Sie bietet eine einfache RESTful Schnittstelle zum Speichern und Abrufen von Log-Einträgen.

**API-Basis-URL:** `http://localhost:8081`

| Methode | Endpunkt | Beschreibung |
|---------|----------|--------------|
| POST | `/api/Redis/log` | Log-Eintrag speichern |
| GET | `/api/Redis/log/{id}` | Log-Eintrag nach ID abrufen |
| GET | `/api/Redis/logs` | Alle Log-Einträge abrufen |

**Log Entry Format (POST Request):**
```json
{
  "logId": 1,
  "timestamp": "2025-02-07T12:34:56",
  "message": "Beispiel-Log",
  "severity": 3
}
```

**Redis Speicherung:**
- **Key Format**: `log:{LogId}`
- **Value Format**: JSON (siehe oben)

**Beispiel-Request (POST /api/Redis/log):**
```bash
curl -X POST http://localhost:8081/api/Redis/log \
  -H "Content-Type: application/json" \
  -d '{
    "logId": 1,
    "timestamp": "2025-02-07T12:34:56",
    "message": "System started successfully",
    "severity": 1
  }'
```

**Beispiel-Response (POST):**
```json
{
  "message": "Log-Eintrag 1 gespeichert."
}
```

**Beispiel-Request (GET /api/Redis/log/1):**
```bash
curl -X GET http://localhost:8081/api/Redis/log/1
```

**Beispiel-Response (GET):**
```json
{
  "logId": 1,
  "timestamp": "2025-02-07T12:34:56",
  "message": "System started successfully",
  "severity": 1
}
```

**Beispiel-Request (GET /api/Redis/logs):**
```bash
curl -X GET http://localhost:8081/api/Redis/logs
```

**Beispiel-Response (GET all):**
```json
[
  {
    "logId": 1,
    "timestamp": "2025-02-07T12:34:56",
    "message": "System started successfully",
    "severity": 1
  },
  {
    "logId": 2,
    "timestamp": "2025-02-07T13:00:00",
    "message": "User logged in",
    "severity": 2
  }
]
```

**Validierungsregeln:**
- `logId`: Muss größer als 0 sein
- `timestamp`: Muss ein gültiges Datum/Zeit-Format sein
- `message`: Darf nicht leer sein, maximale Länge: 500 Zeichen
- `severity`: Muss innerhalb des Bereichs 0-4 liegen, darf nicht negativ sein

**Technologien:**
- .NET 8 / ASP.NET Core Web API
- Redis (NoSQL-Datenbank)
- Docker & Docker Compose
- Swagger/OpenAPI für Dokumentation und Testing
- JSON für Datenaustausch

**Architektur:**
Die API besteht aus 2 Containern:
- **Redis**: NoSQL-Datenbank zum Speichern der Logs
- **sims-nosql-api**: Web-API zum Verarbeiten der Log-Daten

**Features:**
- ✅ Log Einträge speichern
- ✅ Log Einträge nach ID abrufen
- ✅ Alle gespeicherten Logeinträge abrufen
- ✅ Speicherung in Redis als JSON
- ✅ Docker Container für API & Redis
- ✅ Swagger zur Dokumentation und zum Testen

## 💾 Datenbanken

### SQL Server - BackendApi Schema

#### Tabelle: `Incidents`
| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| `Id` | string (PK) | Eindeutige Incident-ID (Format: INC-0001) |
| `UUId` | Guid | Globally Unique Identifier |
| `Summary` | string | Kurzbeschreibung |
| `Description` | string | Detaillierte Beschreibung |
| `CreateDate` | DateTime | Erstellungsdatum |
| `ChangeDate` | DateTime | Letztes Update |
| `ClosedDate` | DateTime? | Schließungsdatum |
| `Status` | IncidentStatus | Status (Open, onProgress, onHold, Aborted, Solved) |
| `IncidentType` | IncidentType | Typ (SecurityIncident) |
| `Severity` | IncidentSeverity | Priorität (Low, Medium, High, Critical) |
| `AssignedPerson` | string? | Zugewiesene Person |
| `CustomerId` | string (FK) | Referenz zum Kunden |

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

#### Tabelle: `Customers`
| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| `Id` | string (PK) | Eindeutige Kunden-ID (Format: CUST-0001) |
| `UUId` | Guid | Globally Unique Identifier |
| `CompanyName` | string | Firmenname |
| `Email` | string | E-Mail-Adresse |
| `PhoneNumber` | string? | Telefonnummer |
| `Address` | string? | Adresse |
| `City` | string? | Stadt |
| `State` | string? | Bundesland/Staat |
| `ZipCode` | string? | Postleitzahl |
| `Country` | string? | Land |
| `Active` | bool | Aktiv-Status |
| `CreateDate` | DateTime | Erstellungsdatum |
| `ChangeDate` | DateTime | Letztes Update |

### PostgreSQL - sims-api Schema

#### Tabelle: `Users`
| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| `Uid` | int (PK) | Primärschlüssel |
| `Firstname` | string | Vorname |
| `Lastname` | string | Nachname |
| `Email` | string | E-Mail-Adresse (Unique) |
| `RoleId` | int (FK) | Referenz zur Rolle |
| `PasswordHash` | string | BCrypt Passwort-Hash |

#### Tabelle: `Roles`
| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| `RoleId` | int (PK) | Primärschlüssel |
| `RoleName` | string | Rollenname (z.B. "Admin", "User") |
| `Description` | string? | Beschreibung |

#### Tabelle: `BlacklistedTokens`
| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| `Id` | int (PK) | Primärschlüssel |
| `Token` | string | Blacklisted JWT Token |
| `BlacklistedAt` | DateTime | Zeitpunkt der Blacklistierung |

### Redis - Logging Schema

**Speicherung in Redis:**
- **Key Format**: `log:{LogId}`
- **Value Format**: JSON

| Feld | Typ | Beschreibung | Validierung |
|------|-----|--------------|-------------|
| `logId` | int | Log-ID | Muss > 0 sein |
| `timestamp` | DateTime | Zeitstempel | Muss gültiges Datum sein |
| `message` | string | Log-Nachricht | Nicht leer, max. 500 Zeichen |
| `severity` | int | Schweregrad | 0-4, nicht negativ |

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

   **Wichtig:** Alle Passwörter, Connection Strings und Secrets befinden sich im **PasswordDepot**. Bitte dort nach den entsprechenden Credentials suchen.

   **BackendApi** verwendet Umgebungsvariablen:
   - `MSSQL_CONNECTION`: SQL Server Connection String (siehe PasswordDepot)
   - `REDIS_CONNECTION`: Redis Connection String (siehe PasswordDepot)

   **sims-api/appsettings.json:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=userapi;Username=<siehe PasswordDepot>;Password=<siehe PasswordDepot>"
     },
     "Jwt": {
       "Key": "<siehe PasswordDepot>",
       "Issuer": "sims-api"
     }
   }
   ```

   **sims-nosql-api/appsettings.json:**
   ```json
   {
     "ConnectionStrings": {
       "Redis": "localhost:6379"
     }
   }
   ```

3. **PostgreSQL & User API starten**
   ```bash
   cd sims-api
   docker-compose up -d
   ```
   - PostgreSQL: Port 5432
   - User API: Port 5000

4. **Redis & NoSQL API starten**
   ```bash
   cd sims-nosql-api
   docker-compose up -d
   ```
   - Redis: Port 6379
   - NoSQL API: Port 8081

5. **BackendApi mit SQL Server & Redis starten**
   ```bash
   cd BackendApi
   docker-compose up -d
   ```
   - SQL Server: Port 1433
   - Redis: Port 6379 (intern)
   - BackendApi: Port 5001

6. **Datenbank-Migrationen ausführen**

   **BackendApi:**
   ```bash
   cd BackendApi
   dotnet ef database update
   ```

   **sims-api:**
   ```bash
   cd sims-api
   dotnet ef database update
   ```
   (Migrationen werden automatisch beim Start ausgeführt)

7. **Anwendungen lokal starten (Alternative zu Docker)**

   **Terminal 1 - BackendApi:**
   ```bash
   cd BackendApi
   dotnet run
   ```
   API verfügbar unter: `http://localhost:5001`

   **Terminal 2 - sims-api:**
   ```bash
   cd sims-api
   dotnet run
   ```
   API verfügbar unter: `http://localhost:5000`

   **Terminal 3 - sims-nosql-api:**
   ```bash
   cd sims-nosql-api
   dotnet run
   ```
   API verfügbar unter: `http://localhost:8081`

   **Terminal 4 - Frontend:**
   ```bash
   cd sims-web_app
   dotnet run
   ```
   Frontend verfügbar unter: `http://localhost:5000` oder `https://localhost:5001`

## 🐳 Docker Deployment

### BackendApi mit SQL Server & Redis

```bash
cd BackendApi
docker-compose up -d
```

Dies startet:
- **SQL Server Container** (Port 1433)
  - Database: `SIMSData`
  - SA Password: (siehe PasswordDepot)
- **Redis Container** (Port 6379, intern)
  - Password: (siehe PasswordDepot)
- **BackendApi Container** (Port 5001)

**Umgebungsvariablen:**
- `MSSQL_CONNECTION`: Automatisch konfiguriert
- `REDIS_CONNECTION`: Automatisch konfiguriert

### sims-api mit PostgreSQL

```bash
cd sims-api
docker-compose up -d
```

Dies startet:
- **PostgreSQL Container** (Port 5432)
  - Database: `userapi`
  - User: (siehe PasswordDepot)
  - Password: (siehe PasswordDepot)
- **User API Container** (Port 5000)

**Features:**
- Automatische Datenbank-Migrationen beim Start
- Health Checks für PostgreSQL
- JWT Token-basierte Authentifizierung

### sims-nosql-api mit Redis

```bash
cd sims-nosql-api
docker-compose up -d
```

Dies startet:
- **Redis Container** (Port 6379)
- **NoSQL API Container** (Port 8081)

**Netzwerk:**
- Alle Container nutzen das `sims-network` Docker-Netzwerk

### Vollständiges Setup (Alle Services)

Für ein vollständiges Setup müssen alle drei Docker Compose Dateien gestartet werden:

```bash
# Terminal 1: BackendApi
cd BackendApi
docker-compose up -d

# Terminal 2: sims-api
cd sims-api
docker-compose up -d

# Terminal 3: sims-nosql-api
cd sims-nosql-api
docker-compose up -d
```

**Hinweis:** Ein zentrales Docker Compose für alle Container ist in Planung (siehe ToDos).

## ✨ Features

### Incident Management
- ✅ CRUD-Operationen für Incidents
- ✅ Incident-Status-Verwaltung (Open, onProgress, onHold, Aborted, Solved)
- ✅ Prioritätsverwaltung (Low, Medium, High, Critical)
- ✅ Automatische Incident-ID-Generierung (INC-0001, INC-0002, ...)
- ✅ Kommentar-System für Incidents
- ✅ Zuweisung von Incidents an Personen
- ✅ Kundenverknüpfung für Incidents
- ✅ Zeitstempel (Erstellung, Update, Schließung)

### Customer Management
- ✅ CRUD-Operationen für Kunden
- ✅ Automatische Kunden-ID-Generierung (CUST-0001, CUST-0002, ...)
- ✅ Vollständige Adressverwaltung
- ✅ Aktiv/Inaktiv Status-Verwaltung

### Benutzerverwaltung
- ✅ JWT-basierte Authentifizierung
- ✅ Rollenbasierte Zugriffskontrolle (Admin/User)
- ✅ Benutzer-CRUD-Operationen (nur für Admins)
- ✅ Token Blacklisting bei Logout
- ✅ BCrypt Password Hashing
- ✅ Event Logging für Benutzeraktionen

### Logging
- ✅ Redis-basierte Logging-Infrastruktur
- ✅ JSON-Format Speicherung in Redis
- ✅ RESTful API für Log-Zugriff
- ✅ Automatisches Logging via Middleware (BackendApi)
- ✅ Event Logging Service (sims-api)
- ✅ Input-Validierung für Log-Einträge

### Frontend
- ✅ Moderne Blazor Web App mit MudBlazor
- ✅ Responsive Design
- ✅ Incident-Übersicht mit interaktiver Tabelle
- ✅ Customer Management UI
- ✅ User Management UI
- ✅ Authentifizierung und Autorisierung
- ✅ Benutzerfreundliche Navigation

### API-Dokumentation
- ✅ Swagger/OpenAPI Integration
- ✅ Automatische API-Dokumentation
- ✅ Interaktive API-Tests über Swagger UI
- ✅ Swagger nur im Development-Modus aktiviert

## 🔒 Security Features

### sims-nosql-api (Redis Logging)

**Implementierte Security Features:**
- ✅ **Dependency Injection** statt statischer Zugriffe - verhindert mehrfaches Öffnen von Redis-Verbindungen
- ✅ **Lazy Connection Multiplexer** - verhindert Verbindungsfehler, baut Redis-Verbindung nur auf wenn wirklich benötigt
- ✅ **ConnectionString in appsettings.json** - kein Connection String im Code
- ✅ **Input Validierung** bei Log-Entry:
  - `logId`: Muss größer als 0 sein
  - `timestamp`: Muss ein gültiges Datum sein
  - `message`: Darf nicht leer sein, maximale Länge: 500 Zeichen
  - `severity`: Muss innerhalb eines gültigen Bereichs liegen (0-4), darf nicht negativ sein

**Offene Security Tasks:**
- ⚠️ **Swagger in Produktion deaktivieren** - sonst sind die Endpunkte öffentlich sichtbar (OFFEN)
- ⚠️ **HTTPS erzwingen** (OFFEN)
- ⚠️ **Redis Passwort** implementieren (OFFEN)
- ⚠️ **API Authentifizierung** hinzufügen (OFFEN)

### sims-api (User Management)

- ✅ **JWT Token Validation** mit Blacklisting
- ✅ **BCrypt Password Hashing**
- ✅ **Rollenbasierte Zugriffskontrolle**
- ✅ **Token Blacklist** für Logout-Funktionalität
- ✅ **Swagger nur im Development-Modus**

### BackendApi

- ✅ **Redis Logging Middleware** für automatisches Logging
- ✅ **Entity Framework Migrations**
- ✅ **Input Validierung** in Controllern

## 🧪 Testing

Unit Tests sind im Projekt `BackendApi.Tests` enthalten:

```bash
cd BackendApi.Tests
dotnet test
```

**Test Coverage:**
- Customer Endpoints Tests
- Incident Endpoints Tests

## 👥 Autoren

Dieses Projekt wurde im Rahmen des SW-AC (Software Architecture) Kurses an der **University of Applied Sciences St. Pölten** entwickelt.

**Projektteam:**
- Grabner Gabriel
- Macala Konatsu
- Mühlparzer Philipp
- Puschnig Margarethe
- Radler Maximilian


## 📝 Aktuelle ToDos

### In Arbeit
- 🔄 **UI für Logs einbinden (NoSQL)** - Log-Übersicht im Frontend implementieren
- 🔄 **BackendLogs nach NoSQL senden (final)** - Finalisierung der Log-Integration
- 🔄 **SAST machen für alle Apps** - Static Application Security Testing via Semgrep (bereits in Arbeit)

### Geplant
- ⏳ **UI Incident umstellen auf "Neue Form"** - UI-Update für Incident-Formulare
- ⏳ **UserApi Anpassungen** - RoleId, Create/Update Thema
- ⏳ **UserApi testen** - Test-Suite für User API
- ⏳ **SA User für DB Zugriff anpassen** - SQL Server SA User Konfiguration
- ⏳ **Docker Compose für alle Container machen** - Zentrales Docker Compose Setup
- ⏳ **BOM + Check via Container von Gerald** - Bill of Materials und Security Checks

## 📝 Weitere Informationen

- **Architektur-Diagramm**: Siehe `Flipcharts/Schema.drawio` oder `Flipcharts/Schema.png`
- **Swagger/OpenAPI Dokumentation** (interaktiv): 
  - BackendApi: `http://localhost:5001/swagger`
  - sims-api: `http://localhost:5000/swagger` (nur Development)
  - sims-nosql-api: `http://localhost:8081/swagger`
- **Credentials & Secrets**: Alle Passwörter, Connection Strings und API-Keys befinden sich im **PasswordDepot**

**Hinweis:** Die vollständige API-Dokumentation ist oben im Abschnitt [API-Endpunkte](#-api-endpunkte) enthalten. Die Redis Logging API Details sind im Abschnitt [sims-nosql-api](#sims-nosql-api-httplocalhost8081api) dokumentiert.

## 📄 Lizenz

[Lizenz-Informationen hier einfügen]

---

**Hinweis**: Dieses System ist für Entwicklungs- und Lernzwecke konzipiert. Für Produktionsumgebungen sollten zusätzliche Sicherheitsmaßnahmen implementiert werden (siehe Security Features).
