import type { StationPriceDto } from "../types/api";

interface StationCardProps {
  station: StationPriceDto;
}

export function StationCard({ station }: StationCardProps) {
  return (
    <div className="station-card">
      <h3>{station.stationName}</h3>
      <h5>{station.brandName}</h5>
      <p className="station-location">
        {station.address}, {station.postalCode} {station.city}
      </p>
      <table>
        <thead>
          <tr>
            <th>Carburant</th>
            <th>Prix/L</th>
            <th>MAJ</th>
          </tr>
        </thead>
        <tbody>
          {station.fuelPrices.map((fp) => (
            <tr key={fp.fuelType}>
              <td>{fp.fuelType}</td>
              <td>{fp.pricePerLiter.toFixed(3)} €</td>
              <td>{new Date(fp.updatedAt).toLocaleDateString("fr-FR")}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
