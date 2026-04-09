import { useState } from "react";
import { useGlobalPriceHistory } from "../hooks/useGlobalPriceHistory";
import { PriceChart } from "./PriceChart";
import type { StationPriceDto } from "../types/api";

const FUEL_TYPES = ["Gazole", "SP95", "SP98", "E10", "E85", "GPLc"];

interface Props {
  stations: StationPriceDto[];
}

export function HistoryView({ stations }: Props) {
  const [fuelType, setFuelType] = useState("Gazole");
  const [from, setFrom] = useState(() => {
    const d = new Date();
    d.setMonth(d.getMonth() - 1);
    return d.toISOString().split("T")[0];
  });
  const [to, setTo] = useState(() => new Date().toISOString().split("T")[0]);

  const stationIds = stations.map((s) => s.externalStationId);
  const { data, isLoading, error } = useGlobalPriceHistory(fuelType, from, to, stationIds);

  return (
    <div>
      <div className="filters">
        <label>
          Carburant
          <select value={fuelType} onChange={(e) => setFuelType(e.target.value)}>
            {FUEL_TYPES.map((ft) => (
              <option key={ft} value={ft}>{ft}</option>
            ))}
          </select>
        </label>

        <label>
          Du
          <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        </label>

        <label>
          Au
          <input type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        </label>
      </div>

      <p className="info">
        Evolution sur <strong>{stations.length}</strong> station(s) de la zone selectionnee.
      </p>

      {isLoading && <p>Chargement...</p>}
      {error && <p className="error">Erreur : {error.message}</p>}
      {data && data.length === 0 && <p>Aucune donnee pour cette periode.</p>}
      {data && data.length > 0 && <PriceChart data={data} />}
    </div>
  );
}
