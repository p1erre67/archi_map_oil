# PriceWatch

> Projet personnel centre sur la mise en pratique de Clean Architecture, DDD, CQRS et Event-Driven design avec .NET 10.

Fonctionnellement : suivi des prix des carburants en France (stations-service), synchronisation quotidienne depuis l'API gouvernementale, historique des prix, carte interactive web et mobile.

## URLs

| Environnement | URL |
|---|---|
| Front (Vercel) | https://archi-map-oil.vercel.app |
| API (Azure Container Apps) | https://pricewatch-api.purpleflower-11ac4ef6.westeurope.azurecontainerapps.io |
| API locale (Scalar) | https://localhost:5001/scalar/v1 |

## Stack

| Couche | Techno |
|--------|--------|
| **API** | .NET 10, Minimal APIs, MediatR, EF Core |
| **BDD** | PostgreSQL (Supabase) |
| **Front web** | React 19, TypeScript, Vite, React Query, Leaflet |
| **Mobile** | Expo, React Native, expo-router, Leaflet (WebView) |
| **CI/CD** | GitHub Actions |
| **Hosting API** | Azure Container Apps (scale-to-zero) |
| **Hosting Front** | Vercel |

---

# Back-end — le cœur du projet

Le focus du projet est sur **l'architecture back-end**. Le but n'etait pas de livrer un produit commercial mais de **mettre en pratique des patterns d'architecture** qu'on retrouve en entreprise : Clean Architecture, DDD, CQRS, Event-Driven. Le domaine fonctionnel (prix carburants) est volontairement simple pour que le temps soit investi sur la **structure du code**, pas sur les regles metier.

## Architecture d'ensemble

```
┌──────────────────────────────────────────────────────────────────┐
│                      PriceWatch.Api (host)                       │
│   Minimal APIs · DI container · Middleware · BackgroundService   │
└─────────────────┬────────────────────────────┬───────────────────┘
                  │                            │
    ┌─────────────┴──────────────┐ ┌───────────┴──────────────────┐
    │  PriceWatch.Modules.Prices │ │ PriceWatch.Modules.History   │
    │                            │ │                              │
    │  Domain                    │ │  Domain                      │
    │  Application (CQRS)        │ │  Application (CQRS)          │
    │  Infrastructure            │ │  Infrastructure              │
    │  Endpoints                 │ │  Endpoints                   │
    └─────────────┬──────────────┘ └───────────┬──────────────────┘
                  │                            │
                  └──────────┬─────────────────┘
                             │
                  ┌──────────┴──────────────┐
                  │  PriceWatch.SharedKernel │
                  │                          │
                  │  Entity, AggregateRoot   │
                  │  DomainEvent, Result     │
                  │  IEndpoint, Behaviours   │
                  └──────────────────────────┘
```

**Monolithe modulaire** : chaque module est un projet .NET autonome avec ses propres couches Clean Architecture. Les modules ne se parlent **pas en direct** (pas de `using PriceWatch.Modules.Other`). La communication inter-modules passe exclusivement par des **events**.

Chaque module a son propre `DbContext` et ses propres tables avec un prefixe (`prices_*`, `history_*`). La meme base physique Postgres est utilisee, mais les frontieres logiques sont strictes — demain, un module peut devenir un microservice sans rewrite du code metier.

## Patterns implementes et pourquoi

### 1. Clean Architecture (par module)

Chaque module respecte une **inversion de dependances** stricte :

```
Endpoints ──► Application ──► Domain
                   │              ▲
                   └── Infrastructure (implemente les interfaces de Domain)
```

| Couche | Ce qu'elle contient | Ce qu'elle connait |
|---|---|---|
| **Domain** | Entities, Value Objects, Domain Events, Repository *interfaces*, Errors | Rien d'externe. Pur C# + SharedKernel |
| **Application** | Commands, Queries, Handlers (via MediatR), DTOs, Validators, UseCases | Domain uniquement |
| **Infrastructure** | EF Core DbContext, Repository *implementations*, HTTP clients externes, DI registration | Application + Domain |
| **Endpoints** | Minimal APIs (`MapGet`, `MapPost`) | Application uniquement via `ISender` |

**Test d'isolation** : le dossier `Domain/` ne contient **aucun** `using Microsoft.EntityFrameworkCore`. Le domaine metier peut etre testable sans base de donnees, sans framework web, sans rien. C'est la promesse de Clean Architecture.

**Pourquoi** : maintenabilite a long terme. Le metier (le plus stable) ne depend jamais de l'infra (qui bouge souvent : Postgres → Mongo, EF → Dapper, etc.). Quand on change un provider DB, seul le dossier `Infrastructure/` est touche.

### 2. Domain-Driven Design (DDD)

Les entites metier sont **anemiques interdites**. Toute regle metier est dans l'aggregate, pas dans un "service".

Exemple : `StationPrice.UpsertFuelPrice(fuelType, price, updatedAt)` — l'aggregate decide si c'est un insert ou un update, applique les invariants, et leve un Domain Event. Le handler ne fait que orchestrer.

```csharp
public sealed class StationPrice : AggregateRoot<StationPriceId>
{
    public void UpsertFuelPrice(string fuelType, decimal price, DateTime updatedAt)
    {
        var existing = _fuelPrices.FirstOrDefault(fp => fp.FuelType == fuelType);
        if (existing is not null)
            existing.UpdatePrice(price, updatedAt);
        else
            _fuelPrices.Add(FuelPrice.Create(fuelType, price, updatedAt));

        LastUpdated = DateTime.UtcNow;
    }

    public void MarkAsSynced(int stationCount)
    {
        RaiseDomainEvent(new StationPricesSyncedEvent(stationCount));
    }
}
```

**Building blocks dans le SharedKernel** :
- `Entity<TId>` : egalite basee sur l'identite (pas la reference memoire)
- `AggregateRoot<TId>` : herite de de Entity + collection de `DomainEvents` en attente
- `DomainEvent` : record immuable qui implement `INotification` (MediatR)
- `Result` / `Result<T>` : pattern fonctionnel pour les erreurs sans exceptions
- `Error.Validation(...)`, `Error.NotFound(...)` : typage strict des erreurs

**Pourquoi** : le code metier est lisible et auto-documente. Un nouveau dev regarde `StationPrice.cs` et comprend immediatement quelles operations sont autorisees et leurs regles, sans avoir a suivre une chaine de services.

### 3. CQRS via MediatR

Chaque use-case est materialise par une **Command** (ecriture) ou une **Query** (lecture) + un Handler. **Aucune classe "Service" geante** qui regroupe des responsabilites disparates.

```
Application/
├── Commands/
│   └── SyncStationPrices/
│       ├── SyncStationPricesCommand.cs         ← juste les donnees (record)
│       ├── SyncStationPricesCommandHandler.cs  ← la logique
│       └── SyncStationPricesCommandValidator.cs ← FluentValidation
├── Queries/
│   ├── GetNearbyStations/
│   ├── GetCheapestStations/
│   └── GetStationPrices/
```

Un endpoint Minimal API ne fait que **traduire l'HTTP en Command/Query** et retourner le `Result` :

```csharp
group.MapGet("/nearby", async (double latitude, double longitude, double radiusKm,
                                ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(
        new GetNearbyStationsQuery(latitude, longitude, radiusKm), ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.Problem(result.Error.Description);
});
```

**Pourquoi** :
- **Lisibilite** : un use-case = un dossier = 3 fichiers courts. On trouve instantanement le code qui execute "synchroniser les prix".
- **Testabilite** : un handler se teste en isolation, on mocke les repositories.
- **Evolutivite** : on peut separer les modeles de lecture et d'ecriture. Les queries peuvent utiliser des projections denormalisees, les commands restent sur les aggregates.

### 4. Pipeline Behaviors (cross-cutting concerns)

MediatR permet d'intercaler des **middlewares** entre `Send()` et le Handler, appliques automatiquement a **tous les handlers**. Exemple dans ce projet : `ValidationPipelineBehavior` :

```csharp
public sealed class ValidationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var failures = _validators
            .Select(v => v.Validate(new ValidationContext<TRequest>(request)))
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count > 0)
            return Result.CreateFailure<TResponse>(
                Error.Validation(failures[0].PropertyName,
                    string.Join("; ", failures.Select(f => f.ErrorMessage))));

        return await next();
    }
}
```

**Ce que ca donne** : on ecrit un `FluentValidator` a cote de chaque Command, et **la validation s'applique automatiquement** avant le Handler, sans toucher au Handler. Pas de duplication, aucun boilerplate dans les use-cases.

**Pourquoi** : **DRY et separation des preoccupations**. Le Handler se concentre sur son metier. Les concerns transversaux (validation, logging, metriques, transactions, cache) sont ajoutes par-dessus sans modifier le code metier. Meme principe qu'un middleware HTTP mais au niveau application.

### 5. Domain Events & Event-Driven

Quand `StationPrice.MarkAsSynced()` leve un `StationPricesSyncedEvent`, il est dispatche via MediatR apres le `SaveChangesAsync` (dans le `DbContext` override). **N reacteurs** peuvent y reagir sans que l'aggregate les connaisse.

Dans le projet :

```
StationPricesSyncedEvent levé par l'aggregate
        │
        ▼
PricesDbContext.SaveChangesAsync() ─ collecte events, save, dispatch
        │
        ├──► LogStationPricesSyncedHandler        (observabilite, Serilog)
        ├──► DetectPriceAnomalyHandler            (metier, alerte si prix anormal)
        └──► PublishIntegrationEventOnSyncHandler (adapter cross-module)
                │
                ▼
        StationPricesSyncedIntegrationEvent
                │
                ▼
        RecordPricesOnSyncHandler (module History)
                │
                └──► INSERT INTO history_price_records
```

**Distinction cle** : **Domain Events** restent *dans le meme module*. **Integration Events** traversent les frontieres de module (Prices → History). Le module History ne connait pas `StationPricesSyncedEvent` — il ecoute `StationPricesSyncedIntegrationEvent` qui est defini dans le SharedKernel.

**Pourquoi** :
- **Decouplage fort** : le module Prices ignore totalement l'existence du module History. On peut desactiver History sans rien changer dans Prices.
- **Ouvert a l'extension** : ajouter un nouveau module "Notifications" qui envoie un email quand un prix baisse ? Un nouveau handler sur l'event existant, zero changement ailleurs.
- **Evolutivite architecturale** : cette structure est le chemin naturel vers un split en microservices (voir section "Pour aller plus loin").

### 6. Unit of Work (via EF Core)

Le `DbContext` joue le role d'**Unit of Work** — il tracke tous les changements en memoire, et `SaveChangesAsync()` commit le tout dans une seule transaction. C'est expose a la couche Application via une interface `IPricesUnitOfWork` (qui ne depend **pas** d'EF Core dans son namespace) pour que le domaine reste ignorant de l'ORM.

```csharp
// Dans Application — ne connait rien d'EF
public interface IPricesUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Dans Infrastructure — l'implementation pointe vers EF
services.AddScoped<IPricesUnitOfWork>(sp => sp.GetRequiredService<PricesDbContext>());
```

**Pourquoi** : tester un handler sans demarrer une vraie DB, ou changer d'ORM sans reecrire le metier.

### 7. Repository Pattern

Chaque aggregate a son `IRepository` dans le Domain et une implementation EF dans l'Infrastructure. Les queries complexes (ex : stations les moins cheres, recherche par distance Haversine) restent dans le repository concret — l'Application consomme une methode nommee, pas un `IQueryable`.

```csharp
public interface IStationPriceRepository
{
    Task<StationPrice?> GetByExternalIdAsync(string externalStationId, CancellationToken ct);
    Task<IReadOnlyList<StationPrice>> GetNearbyAsync(double lat, double lng, double radiusKm, CancellationToken ct);
    Task<IReadOnlyList<StationPrice>> GetCheapestByFuelTypeAsync(string fuelType, int limit, CancellationToken ct);
    void Add(StationPrice station);
    void Update(StationPrice station);
}
```

**Pourquoi** : les methodes expriment une **intention metier** (`GetCheapestByFuelTypeAsync`) plutot qu'une technique (`Query<T>`). L'Application reste lisible et les optimisations SQL restent dans un seul endroit.

## Structure de dossiers

```
API/src/
├── PriceWatch.Api/                         ← host ASP.NET Core
│   ├── Program.cs                          ← composition DI + middleware
│   ├── Infrastructure/
│   │   └── PriceSyncBackgroundService.cs   ← sync automatique en dev
│   └── Extensions/
│       └── WebApplicationExtensions.cs     ← auto-discovery des IEndpoint
│
├── PriceWatch.SharedKernel/                ← building blocks communs
│   ├── Domain/Primitives/                  ← Entity, AggregateRoot
│   ├── Domain/Events/                      ← DomainEvent, IHasDomainEvents
│   ├── Domain/Results/                     ← Result<T>, Error
│   ├── Application/Events/                 ← IntegrationEvents
│   ├── Application/Interfaces/             ← IEndpoint
│   └── Application/Behaviours/             ← ValidationPipelineBehavior
│
└── Modules/
    ├── PriceWatch.Modules.Prices/
    │   ├── Domain/                         ← StationPrice, Brand, FuelPrice, events
    │   ├── Application/                    ← CQRS + EventHandlers
    │   ├── Infrastructure/                 ← EF Core, repositories, DI
    │   └── Endpoints/                      ← PricesEndpoints.cs
    │
    └── PriceWatch.Modules.History/
        ├── Domain/
        ├── Application/
        ├── Infrastructure/
        └── Endpoints/
```

## Tests

La stratégie de tests suit la **pyramide de tests classique** : beaucoup de tests unitaires rapides à la base, quelques tests d'intégration plus lents au milieu, et des tests d'architecture transverses pour garantir que les invariants tiennent dans le temps.

```
             ┌──────────────────────┐
             │  Integration (8)     │  ← Postgres réel (Testcontainers)
             │  ~30s avec Docker    │     flux end-to-end HTTP → DB
             └──────────────────────┘
          ┌────────────────────────────┐
          │  Architecture (16)          │  ← règles structurelles
          │  ~200ms                     │     via NetArchTest
          └────────────────────────────┘
    ┌──────────────────────────────────────┐
    │  Unit (54)                            │  ← Domain + Handlers
    │  ~150ms — tous mockés                 │     via NSubstitute
    └──────────────────────────────────────┘
```

### Stack de test

| Lib | Usage |
|---|---|
| **xUnit** | Test runner |
| **NSubstitute** | Mocking (repositories, unit of work, logger) |
| **FluentAssertions** | Assertions lisibles (`.Should().Be(...)`) |
| **NetArchTest.Rules** | Règles d'architecture exécutées au `dotnet test` |
| **Testcontainers.PostgreSql** | Postgres 16 éphémère dans Docker pour les intégrations |
| **Microsoft.AspNetCore.Mvc.Testing** | `WebApplicationFactory<Program>` pour tester toute la pipeline HTTP en in-process |

### 1. Unit tests (54 tests)

Tests isolés qui vérifient une classe à la fois. Toutes les dépendances (repositories, EF Core, logger) sont **mockées**.

**Couverture** :
- **SharedKernel** — `Entity<TId>` (égalité par identité), `Result<T>` et pattern fonctionnel d'erreurs
- **Domain** — `StationPrice`, `Brand`, `FuelPrice`, `PriceRecord` : factories, invariants, domain events levés au bon moment
- **Application / Command handlers** — `SyncStationPricesCommandHandler` avec tous les cas (nouvelle station, existante, brand upsert, liste vide)
- **Application / Query handlers** — `GetNearby`, `GetCheapest`, `GetStationPrices` : mapping DTO, propagation du `CancellationToken`, gestion du `NotFound`
- **Application / Event handlers** — `DetectPriceAnomaly`, `PublishIntegrationEventOnSync` (module Prices), `RecordPricesOnSync` (module History) : y compris la coercion UTC pour éviter les soucis Postgres

### 2. Architecture tests (16 tests)

Tests qui **valident l'architecture au build**. Si un dev casse la Clean Architecture, la CI échoue.

```csharp
[Fact]
public void Prices_Domain_ShouldNotDependOn_EntityFrameworkCore()
{
    var result = Types.InAssembly(PricesAssembly)
        .That().ResideInNamespace("PriceWatch.Modules.Prices.Domain")
        .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
}
```

**Catégories de règles** :

| Fichier | Règles vérifiées |
|---|---|
| `CleanArchitectureTests` | Domain ne dépend ni d'Infrastructure ni d'EF Core · Application ne dépend pas d'Infrastructure · Domain ignore Application (inversion de dépendance) |
| `ModuleIsolationTests` | **Prices ⇏ History** et **History ⇏ Prices** — les modules communiquent uniquement via Integration Events du SharedKernel |
| `DddConventionsTests` | Domain Events héritent de `DomainEvent` · interfaces `IRepository` vivent dans `Domain.Repositories` · implémentations dans `Infrastructure.Repositories` · les classes `Repository` et `Handler` sont `sealed` |

**Intérêt** : c'est un **filet de sécurité** qui accompagne la revue de code. Un contributeur qui ajoute un `using Microsoft.EntityFrameworkCore` dans le dossier `Domain/` fait échouer la CI immédiatement, avec un message d'erreur explicite pointant vers le type fautif.

### 3. Integration tests (8 tests)

Tests end-to-end sur un **vrai Postgres** lancé par Testcontainers. Chaque test tourne en quelques ms, seul le démarrage du container coûte (~5-10s, une fois par suite).

```csharp
// Postgres 16 démarré dans un container Docker éphémère
private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
    .WithImage("postgres:16")
    .WithDatabase("pricewatch")
    ...
    .Build();

// EF Core migre les 2 modules sur le container
await sp.GetRequiredService<PricesDbContext>().Database.MigrateAsync();
await sp.GetRequiredService<HistoryDbContext>().Database.MigrateAsync();
```

**Ce qui est couvert** :

- **`SyncStationPricesIntegrationTests`** — valide le flux **cross-module** le plus important du projet : après un `SyncStationPricesCommand`, les données sont bien écrites dans `prices_*` **et** le module History a reçu l'integration event qui a rempli `history_price_records`. C'est la preuve que la communication event-driven entre modules fonctionne bout en bout.
- **`PricesEndpointsIntegrationTests`** — via un vrai `HttpClient` sur `WebApplicationFactory`, teste les endpoints Minimal APIs : status codes, sérialisation JSON, 404 sur station inconnue.
- **`PriceRecordRepositoryIntegrationTests`** — valide les requêtes complexes du repository History sur Postgres, notamment la résolution des **aliases SP95/SP95-E10** et les filtres par date / station IDs.

**Pattern "Arrange in-test"** : chaque test seed les données dont il a besoin via une méthode helper (`SeedRecords()`), utilise des external IDs uniques pour éviter les collisions entre tests, et s'appuie sur un scope DI frais à chaque test.

### Lancer les tests en local

```bash
cd API

# Unit + architecture (rapide, ~500ms, aucun prérequis)
dotnet test --filter "FullyQualifiedName~UnitTests|FullyQualifiedName~ArchitectureTests"

# Integration (nécessite Docker Desktop démarré)
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Tout
dotnet test
```

### En CI

Le workflow [`.github/workflows/deploy-api.yml`](.github/workflows/deploy-api.yml) exécute **les 3 catégories** avant chaque déploiement sur Azure. Les runners `ubuntu-latest` ont Docker préinstallé, donc les integration tests tournent nativement sans configuration supplémentaire. Si un test échoue, le job `deploy` ne démarre pas (`needs: test`) — **aucune version buguée ne peut arriver en prod**.

## MCD

### Module Prices

```
┌─────────────────────────────────┐
│     prices_brands               │
├─────────────────────────────────┤
│ PK  id              INT         │
│     name            VARCHAR(100)│
│     short_name      VARCHAR(50) │
│     nb_stations     INT         │
└────────────┬────────────────────┘
             │ 1..N
┌────────────┴────────────────────┐
│     prices_station_prices       │
├─────────────────────────────────┤
│ PK  id                    CHAR(36) │
│ FK  brand_id              INT      │
│     external_station_id   VARCHAR(100) UNIQUE │
│     station_name          VARCHAR(200) │
│     address, city, pc, lat, lon        │
│     last_updated          TIMESTAMPTZ  │
└────────────┬────────────────────┘
             │ 1..N (owned entity)
┌────────────┴────────────────────┐
│     prices_fuel_prices          │
├─────────────────────────────────┤
│ PK  id              INT AUTO    │
│ FK  station_price_id CHAR(36)   │
│     fuel_type       VARCHAR(20) │
│     price_per_liter DECIMAL(8,3)│
│     updated_at      TIMESTAMPTZ │
└─────────────────────────────────┘
```

### Module History

```
┌─────────────────────────────────┐
│     history_price_records       │  ← append-only, immuable
├─────────────────────────────────┤
│ PK  id                    CHAR(36) │
│     external_station_id   VARCHAR(100) [idx] │
│     station_name          VARCHAR(200) │
│     city                  VARCHAR(100) │
│     fuel_type             VARCHAR(20) [idx]  │
│     price_per_liter       DECIMAL(8,3) │
│     recorded_at           TIMESTAMPTZ [idx]  │
│                                       │
│  Index composite : (stationId, fuelType, recordedAt) │
└─────────────────────────────────┘
```

Chaque module a sa propre table de migrations EF (`__ef_migrations_prices`, `__ef_migrations_history`) pour que les deploiements soient independants.

## API endpoints

### Module Prices

| Methode | Route | Description |
|---------|-------|-------------|
| `GET` | `/api/prices/stations/{externalStationId}` | Prix d'une station |
| `GET` | `/api/prices/nearby?latitude&longitude&radiusKm` | Stations a proximite (Haversine) |
| `GET` | `/api/prices/cheapest?fuelType&limit` | Stations les moins cheres pour un carburant |
| `POST` | `/api/prices/sync` | Declenche une synchronisation depuis l'API gouv |

### Module History

| Methode | Route | Description |
|---------|-------|-------------|
| `GET` | `/api/history/stations/{externalStationId}?fuelType` | Historique d'une station |
| `GET` | `/api/history/global?fuelType&from&to&stationIds` | Evolution globale (filtrable par stations) |

---

# Infrastructure & DevOps

## Architecture cloud

```
┌──────────────────────────┐     ┌──────────────────────────┐
│   Vercel (front React)   │     │   Expo (mobile APK)      │
│   archi-map-oil.vercel   │     │   Android native build   │
└───────────┬──────────────┘     └──────────┬───────────────┘
            │                               │
            │         HTTPS                 │
            └──────────────┬────────────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │  Azure Container Apps   │
              │  pricewatch-api         │
              │  scale 0-1 · free tier  │
              │  image : ACR private    │
              └───────────┬─────────────┘
                          │ SSL (Session Pooler)
                          ▼
              ┌─────────────────────────┐
              │  Supabase (PostgreSQL)  │
              │  free tier · EU-west-3  │
              └─────────────────────────┘
```

**Couts** : 0 €/mois (scale-to-zero + free tiers). L'API dort la plupart du temps et se reveille uniquement sur demande (HTTP request) ou via le cron quotidien.

## CI/CD

| Workflow | Declencheur | Action |
|---|---|---|
| `.github/workflows/deploy-api.yml` | Push sur `master` (fichiers `API/`) | Tests unitaires → Build Docker → Push ACR → Update Container App |
| `.github/workflows/sync-cron.yml` | Cron quotidien 06:00 UTC | `POST /api/prices/sync` sur 3 villes (Paris, Strasbourg, Auxerre) |

Le front est deploye automatiquement par **Vercel** a chaque push sur `master`.

## Secrets management

- **Local** : `dotnet user-secrets` (stockage hors-repo dans `%APPDATA%`)
- **Azure** : `ContainerApp Secrets` injectes comme variables d'env (`ConnectionStrings__PriceWatch=secretref:db-connection`)
- **CI/CD** : GitHub Secrets (`AZURE_CREDENTIALS`, `ACR_NAME`, `API_BASE_URL`)
- **Repo Git** : **aucun secret** — verifie a chaque refactor

---

# Front-end

## Web (React + Vite + Leaflet)

- **TanStack React Query** pour le state serveur (cache, invalidation automatique, requetes concurrentes)
- **Leaflet + OpenStreetMap** (pas de cle API, pas de limite d'usage)
- **Autocomplete de villes** via l'API gouvernementale `geo.api.gouv.fr` (zero mapping local, IPv4/IPv6 compatible)
- **3 onglets** qui partagent le meme filtre (ville + rayon) : Liste, Carte, Evolution des prix
- Le filtre localite alimente directement la query d'historique : `GET /api/history/global?stationIds=...` → les courbes d'evolution reflettent uniquement les stations de la zone selectionnee

## Mobile (Expo + React Native)

- **Expo Router** (file-system based routing)
- **Carte Leaflet** dans une `WebView` avec `postMessage` → `Linking.openURL` pour declencher le picker natif "geo:" Android (Google Maps, Waze, etc.)
- **Geolocation** avec `Accuracy.Balanced` + timeout 5s (eviter le piege `PRIORITY_PASSIVE` d'Android qui hang)
- Synchronisation automatique des fuel types (SP95 et SP95-E10 traites comme une seule famille)

---

# Lancer en local

## API + front web

```bash
# Configurer la connection string (une seule fois)
cd API
dotnet user-secrets set "ConnectionStrings:PriceWatch" "Host=localhost;Port=5432;Database=pricewatch;Username=postgres;Password=xxx"

# Migrations
dotnet ef database update --project src/Modules/PriceWatch.Modules.Prices --startup-project src/PriceWatch.Api
dotnet ef database update --project src/Modules/PriceWatch.Modules.History --startup-project src/PriceWatch.Api

# API (port 5001)
dotnet run --project src/PriceWatch.Api

# Front web (port 5173)
cd ../front
npm install
npm run dev
```

En dev, un `PriceSyncBackgroundService` synchronise automatiquement les prix toutes les 24h. En prod, ce service est desactive et remplace par le cron GitHub Actions (pour permettre le scale-to-zero d'Azure Container Apps).

## App mobile (Expo)

```bash
cd APP
npm install
npx expo start
```

Scanner le QR code avec **Expo Go** sur le telephone (meme WiFi que le PC).

### Build APK (Android)

```bash
cd APP
npx expo prebuild --clean --platform android
cd android
./gradlew assembleRelease
```

L'APK est genere dans `APP/android/app/build/outputs/apk/release/app-release.apk`.

**Prerequis** :
- **JDK 17** (`winget install Microsoft.OpenJDK.17`)
- **Android SDK** (`ANDROID_HOME` configure)

### Configuration

Creer un fichier `APP/.env` :

```
EXPO_PUBLIC_API_URL=https://pricewatch-api.purpleflower-11ac4ef6.westeurope.azurecontainerapps.io
```

La variable est injectee au build. Modifier l'URL necessite un rebuild.

---

# Pour aller plus loin

Deux axes d'evolution interessants a creuser dans le cadre d'une montee en charge du systeme : robustifier le dispatch des events (write model) et optimiser les requetes complexes cote lecture avec Dapper (read model).

## Scalabilite des events

Cette section reflechit a la maniere dont l'architecture actuelle (events dispatches in-process synchrones via MediatR) pourrait evoluer vers des patterns plus robustes a mesure que le systeme grossit.

### Situation actuelle (in-process synchrone)

Les Domain Events sont dispatches par MediatR dans le meme processus, de maniere synchrone, apres le `SaveChangesAsync` :

```
HTTP Request
  └─ SyncStationPricesCommandHandler
       └─ StationPrice.MarkAsSynced()          ← aggregate leve l'event
       └─ PricesDbContext.SaveChangesAsync()
            ├─ INSERT INTO station_prices       ← persistance
            └─ MediatR.Publish(DomainEvent)     ← dispatch synchrone
                 ├─ LogHandler
                 ├─ DetectAnomalyHandler
                 └─ PublishIntegrationEventHandler
                      └─ MediatR.Publish(IntegrationEvent)
                           └─ RecordPricesOnSyncHandler
                                └─ HistoryDbContext.SaveChangesAsync()
```

**Limites** :
- Si l'app crash entre le save Prices et le dispatch : events perdus
- Si un handler echoue : pas de retry, les handlers suivants ne s'executent pas
- Tout est synchrone : la requete HTTP attend la fin de toute la chaine

### Niveau 2 — Outbox Pattern + Background Worker

L'Outbox garantit qu'un event n'est jamais perdu en l'inserant dans la **meme transaction** que les donnees metier.

**1. Table `outbox_messages`** dans le schema du module Prices :

```sql
CREATE TABLE prices_outbox_messages (
    id              UUID PRIMARY KEY,
    event_type      VARCHAR(500)  NOT NULL,
    payload         JSONB         NOT NULL,
    created_at      TIMESTAMPTZ   NOT NULL,
    processed_at    TIMESTAMPTZ   NULL        -- NULL = pas encore traite
);
```

**2. `SaveChangesAsync` insere les events dans l'outbox** (meme transaction) :

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var events = ChangeTracker.Entries()
        .Where(e => e.Entity is IHasDomainEvents)
        .SelectMany(e => ((IHasDomainEvents)e.Entity).DomainEvents)
        .ToList();

    foreach (var domainEvent in events)
    {
        OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = domainEvent.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            CreatedAt = DateTime.UtcNow
        });
    }

    // Donnees + outbox dans la MEME transaction
    return await base.SaveChangesAsync(ct);
}
```

**3. Background worker** lit les messages non traites et les publie :

```csharp
public class OutboxProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PricesDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

            var messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(20)
                .ToListAsync(ct);

            foreach (var message in messages)
            {
                var eventType = Type.GetType(message.EventType)!;
                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType)!;
                await publisher.Publish(domainEvent, ct);
                message.ProcessedAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(ct);
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
```

**Flux** :

```
HTTP Request (rapide — plus d'attente des handlers)
  └─ SaveChangesAsync()
       ├─ INSERT INTO station_prices        ← donnees
       └─ INSERT INTO outbox_messages       ← events (meme transaction)

Background Worker (toutes les 5s)
  └─ SELECT FROM outbox_messages WHERE processed_at IS NULL
       └─ MediatR.Publish(event)
       └─ UPDATE outbox_messages SET processed_at = NOW()
```

### Niveau 3 — Outbox + Message Broker (RabbitMQ / Azure Service Bus)

Quand le monolithe se decoupe en services independants (ex : le module History devient un microservice), un **broker externe** remplace MediatR pour la communication inter-services.

Avec **MassTransit + RabbitMQ** :

```csharp
// Le worker outbox publie vers le broker au lieu de MediatR
await publishEndpoint.Publish(domainEvent);  // MassTransit → RabbitMQ

// Le module History consomme depuis le broker
public class RecordPricesConsumer : IConsumer<StationPricesSyncedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StationPricesSyncedIntegrationEvent> context)
    {
        // Meme logique que RecordPricesOnSyncHandler actuel
    }
}
```

**Flux** :

```
Service Prices                          Service History
  └─ SaveChanges + Outbox                (processus separe)
       ↓
  └─ Worker lit outbox
       └─ Publish → [ RabbitMQ ] → Consumer
                                      └─ RecordPricesConsumer
                                           └─ HistoryDbContext.Save
```

### Comparatif

| | Actuel | Outbox (niveau 2) | Broker (niveau 3) |
|---|---|---|---|
| **Durabilite** | Events perdus si crash | Persistes en DB | Persistes en DB + broker |
| **Retry** | Non | Oui (worker relit) | Oui (broker) |
| **Performance** | Synchrone | Asynchrone | Asynchrone + distribue |
| **Scaling** | Monolithe unique | Monolithe unique | Multi-instances |
| **Complexite** | Faible | Moyenne | Elevee |

---

## Dapper dans le read model CQRS

### Le constat

Une archi CQRS separe les **Commands** (ecriture) des **Queries** (lecture). Cette separation n'est pas que cosmetique : les deux cotes ont des besoins techniques fondamentalement differents.

| Cote | Besoins | Outil naturel |
|---|---|---|
| **Command (write)** | Change tracking, invariants DDD, transactions, Unit of Work, migrations | **EF Core** |
| **Query (read lourde)** | Vitesse brute, SQL precis, aggregations en DB, pas de tracking | **Dapper** |

Le pattern industriel courant : **EF Core pour le write model, Dapper pour les queries complexes ou chaudes du read model**. Les deux cohabitent dans la meme couche Infrastructure sans se marcher dessus.

### Pourquoi EF Core n'est pas ideal cote read lourd

Regardons `GetGlobalPriceHistoryQueryHandler` du module History :

```csharp
var records = await _repository.GetGlobalAveragesAsync(
    request.FuelType, fromUtc, toUtc, request.StationIds, cancellationToken);

// Agrege cote CLIENT en LINQ (pas en SQL)
var points = records
    .GroupBy(r => r.RecordedAt.Date)
    .Select(g => new GlobalPricePointDto(
        g.Key,
        Math.Round(g.Average(r => r.PricePerLiter), 3),
        g.Min(r => r.PricePerLiter),
        g.Max(r => r.PricePerLiter),
        g.Select(r => r.ExternalStationId).Distinct().Count()))
    .OrderBy(p => p.Date)
    .ToList();
```

**Ce qui se passe reellement** :
1. EF genere un `SELECT *` qui ramene **toutes les lignes** de la periode en memoire
2. Chaque ligne est materialisee en `PriceRecord` avec change tracking
3. Le `GroupBy` / `Average` / `Min` / `Max` est execute **cote C#** apres le chargement
4. Sur 10k records ca va, sur 10M records l'app crash en OutOfMemory

Le meme resultat en SQL pur tiendrait sur un seul aller-retour, avec **zero tuple materialise**, grace a un `GROUP BY` natif Postgres.

### L'implementation avec Dapper

On introduit une **interface dediee aux queries lourdes**, injectee a cote du repository EF :

```csharp
// Application / Queries / GetGlobalPriceHistory
public interface IPriceHistoryQueries
{
    Task<IReadOnlyList<GlobalPricePointDto>> GetGlobalPricePointsAsync(
        string fuelType, DateTime from, DateTime to,
        IReadOnlyList<string>? stationIds, CancellationToken ct);
}

// Infrastructure / Queries
internal sealed class PriceHistoryQueries : IPriceHistoryQueries
{
    private readonly NpgsqlConnection _connection;

    public PriceHistoryQueries(NpgsqlConnection connection) => _connection = connection;

    public async Task<IReadOnlyList<GlobalPricePointDto>> GetGlobalPricePointsAsync(
        string fuelType, DateTime from, DateTime to,
        IReadOnlyList<string>? stationIds, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                date_trunc('day', recorded_at)          AS date,
                ROUND(AVG(price_per_liter)::numeric, 3) AS averagePrice,
                MIN(price_per_liter)                    AS minPrice,
                MAX(price_per_liter)                    AS maxPrice,
                COUNT(DISTINCT external_station_id)     AS stationCount
            FROM history_price_records
            WHERE fuel_type = @FuelType
              AND recorded_at BETWEEN @From AND @To
              AND (@StationIds IS NULL OR external_station_id = ANY(@StationIds))
            GROUP BY date_trunc('day', recorded_at)
            ORDER BY date;";

        var rows = await _connection.QueryAsync<GlobalPricePointDto>(
            new CommandDefinition(sql,
                new { FuelType = fuelType, From = from, To = to, StationIds = stationIds },
                cancellationToken: ct));

        return rows.AsList();
    }
}
```

Et le handler devient trivial :

```csharp
public async Task<Result<IReadOnlyList<GlobalPricePointDto>>> Handle(
    GetGlobalPriceHistoryQuery request, CancellationToken ct)
{
    var points = await _queries.GetGlobalPricePointsAsync(
        request.FuelType, request.From, request.To, request.StationIds, ct);

    return Result.Success(points);
}
```

### Ce qu'on gagne

- **Perf** : le GROUP BY / AVG / MIN / MAX tourne sur Postgres, pas en memoire C#. Sur des volumes eleves (100k+ records), gain d'un facteur 10 a 100.
- **Controle fin du SQL** : on peut utiliser `date_trunc`, `DISTINCT`, CTE, window functions... tout ce que Postgres offre et qu'EF ne traduit pas parfaitement.
- **Zero allocation inutile** : pas de change tracking, pas de proxies, pas de navigation properties charges pour rien.

### Ce qu'on ne casse pas

- **Le write model reste en EF Core** — les aggregates, les invariants DDD, les Unit of Work, les migrations : tout fonctionne comme avant. Dapper ne remplace pas EF, il complete.
- **Les deux repositories vivent dans la meme couche Infrastructure** — pas de fuite vers la couche Application. Les handlers dependent d'interfaces dans `Application/Queries/`, ils ne savent meme pas que Dapper existe.
- **Les architecture tests restent valides** — les interfaces sont toujours dans `Domain` / `Application`, les implementations dans `Infrastructure`.

### Query handlers candidats dans ce projet

Aujourd'hui, avec les volumes actuels (quelques centaines de stations, quelques milliers de records), **tout fonctionne tres bien en EF Core**. Ce refactor serait premature. Mais si le projet grossissait (par exemple en synchronisant toute la France : ~10 000 stations × 5 carburants × 365 jours = ~18M records / an), ces trois handlers deviendraient candidats a une migration vers Dapper :

| Handler | Raison |
|---|---|
| `GetGlobalPriceHistoryQueryHandler` | Aggregation GROUP BY / AVG / MIN / MAX faite cote client aujourd'hui |
| `GetCheapestStationsQueryHandler` | Le `ORDER BY MIN subquery` force EF a generer un SQL sous-optimal |
| `GetNearbyStationsQueryHandler` | Le calcul Haversine via `Math.Sin` / `Math.Cos` serait plus efficace en SQL natif, voire via l'extension **PostGIS** |

### Pourquoi garder EF Core par defaut

EF Core reste le bon choix **par defaut** pour 80% des requetes. Sur une lecture simple (`GetByExternalIdAsync`), le gain Dapper est negligeable et on perd la lisibilite de LINQ, le typage fort, le tracking optionnel. Le pattern hybride s'applique **uniquement aux queries couteuses ou tres frequentes** — pas a tout, pas systematiquement.

C'est la meme philosophie que l'Outbox Pattern plus haut : **ce sont des outils a sortir de la boite quand le besoin reel arrive**, pas des choix architecturaux a prendre des le premier commit.
