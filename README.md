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
| Mobile | Expo, React Native, Leaflet (WebView) |
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

## App mobile (Expo)

### Developpement

```bash
cd app
npm install
npx expo start
```

Scanner le QR code 

### Build APK (Android)

```bash
cd app
npx expo prebuild --clean --platform android
cd android
./gradlew assembleRelease
```

L'APK est genere dans `app/android/app/build/outputs/apk/release/`.

Prerequis : JDK 17+ (`winget install Microsoft.OpenJDK.17`) et Android SDK (`ANDROID_HOME` configure).

### Configuration

Creer un fichier `app/.env` :

```
EXPO_PUBLIC_API_URL=xxx
```

La variable est injectee au build. Modifier l'URL necessite un rebuild.

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

## Pour aller plus loin : scalabilite des events

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

Limites :
- Si l'app crash entre le save Prices et le dispatch : events perdus
- Si un handler echoue : pas de retry, les handlers suivants ne s'executent pas
- Tout est synchrone : la requete HTTP attend la fin de toute la chaine

---

### Niveau 2 — Outbox Pattern + Background Worker

L'Outbox garantit qu'un event n'est jamais perdu en l'inserant dans la meme transaction que les donnees metier.

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

---

### Niveau 3 — Outbox + Message Broker (RabbitMQ / Azure Service Bus)

Quand le monolithe se decoupe en services independants (ex: le module History devient un microservice), un broker externe remplace MediatR pour la communication inter-services.

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

---

### Comparatif

| | Actuel | Outbox (niveau 2) | Broker (niveau 3) |
|---|---|---|---|
| Durabilite | Events perdus si crash | Persistes en DB | Persistes en DB + broker |
| Retry | Non | Oui (worker relit) | Oui (broker) |
| Performance | Synchrone | Asynchrone | Asynchrone + distribue |
| Scaling | Monolithe unique | Monolithe unique | Multi-instances |
| Complexite | Faible | Moyenne | Elevee |
