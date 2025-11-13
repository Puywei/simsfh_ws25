# Basis-Image für Laufzeit
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build-Image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 🔹 KORRIGIERTER COPY-BEFEHL
COPY ["sims-nosql-api.csproj", "./"]
RUN dotnet restore "./sims-nosql-api.csproj"

# 🔹 Quellcode kopieren und Build durchführen
COPY . .
RUN dotnet build "./sims-nosql-api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 🔹 Veröffentlichen (Publish)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./sims-nosql-api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 🔹 Finale Laufzeitstufe
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "sims-nosql-api.dll"]
