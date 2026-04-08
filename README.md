# PriceWatch

Suivi des prix des carburants en France. Synchronisation quotidienne depuis l'API gouvernementale, historique des prix, carte interactive.

- **Front** : http://localhost:5173
- **API (Scalar)** : https://localhost:56819/scalar/v1

## Stack

| Couche | Techno |
|--------|--------|
| API | .NET 10, Minimal APIs, MediatR, EF Core |
| BDD | MariaDB 11.4 |
| Front | React 19, TypeScript, Vite, Leaflet |

Architecture : monolithe modulaire, Clean Architecture, CQRS.

## Modules

- **Prices** : stations, marques, prix carburants, synchro API gouv
- **History** : historique des prix pour le suivi d'evolution

## Lancer le projet

```bash
# BDD
docker-compose up -d mariadb

# API
cd API
dotnet ef database update --project src/Modules/PriceWatch.Modules.Prices --startup-project src/PriceWatch.Api
dotnet ef database update --project src/Modules/PriceWatch.Modules.History --startup-project src/PriceWatch.Api
dotnet run --project src/PriceWatch.Api

# Front
cd front
npm install
npm run dev
```

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
| GET | `/api/history/global?fuelType&from&to` | Evolution globale des prix |
