┌─────────────────────────────────┐
│     prices_station_prices       │
├─────────────────────────────────┤
│ PK  id              CHAR(36)    │
│     external_station_id VARCHAR(100) UNIQUE │
│     station_name    VARCHAR(200)│
│     address         VARCHAR(300)│
│     city            VARCHAR(100)│
│     postal_code     VARCHAR(10) │
│     latitude        DOUBLE      │
│     longitude       DOUBLE      │
│     last_updated    DATETIME    │
└────────────┬────────────────────┘
             │
             │  1..N
             │
┌────────────┴────────────────────┐
│       prices_fuel_prices        │
├─────────────────────────────────┤
│ PK  id              INT AUTO    │
│ FK  station_price_id CHAR(36)   │
│     fuel_type       VARCHAR(20) │
│     price_per_liter DECIMAL(8,3)│
│     updated_at      DATETIME    │
└─────────────────────────────────┘