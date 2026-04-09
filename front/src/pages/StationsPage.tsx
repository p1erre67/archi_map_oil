import { useState } from "react";
import { useNearbyStations } from "../hooks/useNearbyStations";
import { useSyncPrices } from "../hooks/useSyncPrices";
import { StationCard } from "../components/StationCard";
import { StationsMap } from "../components/StationsMap";
import { HistoryView } from "../components/HistoryView";
import { CityAutocomplete } from "../components/CityAutocomplete";
import type { City } from "../hooks/useCitySearch";

type ViewTab = "list" | "map" | "history";

export function StationsPage() {
  const [selectedCity, setSelectedCity] = useState<City | null>(null);
  const [radiusKm, setRadiusKm] = useState(5);
  const [activeTab, setActiveTab] = useState<ViewTab>("list");

  const { data: stations, isLoading, error } = useNearbyStations(
    selectedCity?.latitude ?? 0,
    selectedCity?.longitude ?? 0,
    radiusKm
  );
  const sync = useSyncPrices();

  function handleSync() {
    if (!selectedCity) return;
    sync.mutate({ latitude: selectedCity.latitude, longitude: selectedCity.longitude, radiusKm });
  }

  return (
    <div className="page">
      <h1>PriceWatch</h1>

      <div className="filters">
        <label>
          Ville
          <CityAutocomplete onSelect={setSelectedCity} />
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

        <button onClick={handleSync} disabled={sync.isPending || !selectedCity}>
          {sync.isPending ? "Sync en cours..." : "Synchroniser les prix"}
        </button>
      </div>

      {sync.isSuccess && <p className="success">{sync.data.message}</p>}
      {sync.isError && <p className="error">Erreur sync : {sync.error.message}</p>}

      <div className="tabs">
        <button
          className={`tab ${activeTab === "list" ? "tab-active" : ""}`}
          onClick={() => setActiveTab("list")}
        >
          Liste
        </button>
        <button
          className={`tab ${activeTab === "map" ? "tab-active" : ""}`}
          onClick={() => setActiveTab("map")}
        >
          Carte
        </button>
        <button
          className={`tab ${activeTab === "history" ? "tab-active" : ""}`}
          onClick={() => setActiveTab("history")}
        >
          Evolution
        </button>
      </div>

      {isLoading && <p>Chargement...</p>}
      {error && <p className="error">Erreur : {error.message}</p>}

      {activeTab === "list" && (
        <>
          <div className="stations-grid">
            {stations?.map((station) => (
              <StationCard key={station.id} station={station} />
            ))}
          </div>

          {stations && stations.length === 0 && selectedCity && (
            <p>Aucune station trouvee. Essayez de synchroniser d'abord.</p>
          )}
        </>
      )}

      {activeTab === "map" && selectedCity && stations && (
        <StationsMap
          stations={stations}
          center={[selectedCity.latitude, selectedCity.longitude]}
        />
      )}

      {activeTab === "map" && !selectedCity && (
        <p>Selectionnez une ville pour afficher la carte.</p>
      )}

      {activeTab === "history" && selectedCity && stations && (
        <HistoryView stations={stations} />
      )}

      {activeTab === "history" && !selectedCity && (
        <p>Selectionnez une ville pour afficher l'evolution des prix.</p>
      )}
    </div>
  );
}
