import { useState } from "react";
import { useNearbyStations } from "../hooks/useNearbyStations";
import { useSyncPrices } from "../hooks/useSyncPrices";
import { StationCard } from "../components/StationCard";

export function StationsPage() {
  const [latitude, setLatitude] = useState(48.8566);
  const [longitude, setLongitude] = useState(2.3522);
  const [radiusKm, setRadiusKm] = useState(10);

  const { data: stations, isLoading, error } = useNearbyStations(latitude, longitude, radiusKm);
  const sync = useSyncPrices();

  function handleSync() {
    sync.mutate({ latitude, longitude, radiusKm });
  }

  return (
    <div className="page">
      <h1>Stations proches</h1>

      <div className="filters">
        <label>
          Latitude
          <input
            type="number"
            step="0.0001"
            value={latitude}
            onChange={(e) => setLatitude(Number(e.target.value))}
          />
        </label>

        <label>
          Longitude
          <input
            type="number"
            step="0.0001"
            value={longitude}
            onChange={(e) => setLongitude(Number(e.target.value))}
          />
        </label>

        <label>
          Rayon (km)
          <input
            type="number"
            min={1}
            value={radiusKm}
            onChange={(e) => setRadiusKm(Number(e.target.value))}
          />
        </label>

        <button onClick={handleSync} disabled={sync.isPending}>
          {sync.isPending ? "Sync en cours..." : "Synchroniser les prix"}
        </button>
      </div>

      {sync.isSuccess && <p className="success">{sync.data.message}</p>}
      {sync.isError && <p className="error">Erreur sync : {sync.error.message}</p>}

      {isLoading && <p>Chargement...</p>}
      {error && <p className="error">Erreur : {error.message}</p>}

      <div className="stations-grid">
        {stations?.map((station) => (
          <StationCard key={station.id} station={station} />
        ))}
      </div>

      {stations && stations.length === 0 && (
        <p>Aucune station trouvee. Essayez de synchroniser d'abord.</p>
      )}
    </div>
  );
}
