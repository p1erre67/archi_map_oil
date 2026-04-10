# PriceWatch

Suivi des prix des carburants en France. Synchronisation quotidienne depuis l'API gouvernementale, historique des prix, carte interactive.

## URLs

| Environnement | URL |
|---|---|
| Front (Vercel) | https://archi-map-oil.vercel.app |
| API (Azure Container Apps) | https://pricewatch-api.purpleflower-11ac4ef6.westeurope.azurecontainerapps.io |
| API locale (Scalar) | https://localhost:5001/scalar/v1 |

## Stack

| Couche | Techno |
|--------|--------|
| API | .NET 10, Minimal APIs, MediatR, EF Core |
| BDD | PostgreSQL (Supabase) |
| Front | React 19, TypeScript, Vite, Leaflet |
| CI/CD | GitHub Actions |
| Hosting API | Azure Container Apps (scale-to-zero) |
| Hosting Front | Vercel |

Architecture : monolithe modulaire, Clean Architecture, CQRS.

## Architecture cloud

```
┌─────────────────────────┐
│  Vercel (front React)   │
│  archi-map-oil.vercel   │
└───────────┬─────────────┘
            │ HTTPS
            ▼
┌─────────────────────────┐
│  Azure Container Apps   │
│  pricewatch-api         │
│  scale 0-1, free tier   │
└───────────┬─────────────┘
            │ SSL
            ▼
┌─────────────────────────┐
│  Supabase (PostgreSQL)  │
│  Session Pooler IPv4    │
└─────────────────────────┘
```

## CI/CD

| Workflow | Declencheur | Action |
|---|---|---|
| `deploy-api.yml` | Push sur master (fichiers `API/`) | Build Docker, push ACR, update Container App |
| `sync-cron.yml` | Cron quotidien 06:00 UTC | POST /api/prices/sync sur l'API en prod |

Le front est deploye automatiquement par Vercel a chaque push sur master.

## Modules

- **Prices** : stations, marques, prix carburants, synchro API gouv
- **History** : historique des prix pour le suivi d'evolution

## Lancer en local

```bash
# Configurer la connection string (une seule fois)
cd API
dotnet user-secrets set "ConnectionStrings:PriceWatch" "Host=localhost;Port=5432;Database=pricewatch;Username=postgres;Password=xxx"

# Migrations
dotnet ef database update --project src/Modules/PriceWatch.Modules.Prices --startup-project src/PriceWatch.Api
dotnet ef database update --project src/Modules/PriceWatch.Modules.History --startup-project src/PriceWatch.Api

# API
dotnet run --project src/PriceWatch.Api

# Front
cd ../front
npm install
npm run dev
```

En local, le background service synchronise les prix automatiquement (toutes les 24h). En production, c'est un cron GitHub Actions qui appelle l'endpoint sync (pour permettre le scale-to-zero).

## MCD

### Module Prices

```
┌─────────────────────────────────┐
│     prices_brands               │
├─────────────────────────────────┤
│ PK  id              INT        │
│     name            VARCHAR(100)│
│     short_name      VARCHAR(50) │
│     nb_stations     INT         │
└────────────┬────────────────────┘
             │ 1..N
┌────────────┴────────────────────┐
│     prices_station_prices       │
├─────────────────────────────────┤
│ PK  id              CHAR(36)   │
│ FK  brand_id        INT        │
│     external_station_id VARCHAR(100) UNIQUE │
│     station_name    VARCHAR(200)│
│     address         VARCHAR(300)│
│     city            VARCHAR(100)│
│     postal_code     VARCHAR(10) │
│     latitude        DOUBLE      │
│     longitude       DOUBLE      │
│     last_updated    DATETIME    │
└────────────┬────────────────────┘
             │ 1..N
┌────────────┴────────────────────┐
│     prices_fuel_prices          │
├─────────────────────────────────┤
│ PK  id              INT AUTO   │
│ FK  station_price_id CHAR(36)  │
│     fuel_type       VARCHAR(20) │
│     price_per_liter DECIMAL(8,3)│
│     updated_at      DATETIME    │
└─────────────────────────────────┘
```

### Module History

```
┌─────────────────────────────────┐
│     history_price_records       │
├─────────────────────────────────┤
│ PK  id              CHAR(36)   │
│     external_station_id VARCHAR(100) │
│     station_name    VARCHAR(200)│
│     city            VARCHAR(100)│
│     fuel_type       VARCHAR(20) │
│     price_per_liter DECIMAL(8,3)│
│     recorded_at     DATETIME    │
└─────────────────────────────────┘
```

## API endpoints

### Prices

| Methode | Route | Description |
|---------|-------|-------------|
| GET | `/api/prices/stations/{externalStationId}` | Prix d'une station |
| GET | `/api/prices/nearby?latitude&longitude&radiusKm` | Stations a proximite |
| GET | `/api/prices/cheapest?fuelType&limit` | Stations les moins cheres |
| POST | `/api/prices/sync` | Synchroniser depuis l'API gouv |

### History

| Methode | Route | Description |
|---------|-------|-------------|
| GET | `/api/history/station/{externalStationId}` | Historique d'une station |
| GET | `/api/history/global?fuelType&from&to&stationIds` | Evolution des prix (filtrable par stations) |
