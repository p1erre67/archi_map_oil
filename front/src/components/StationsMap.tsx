import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import type { StationPriceDto } from "../types/api";

// Fix: Leaflet auto-detects icon paths via CSS parsing, which breaks with Vite.
// We disable auto-detection and import the images so Vite resolves them as static assets.
import iconUrl from "leaflet/dist/images/marker-icon.png";
import iconRetinaUrl from "leaflet/dist/images/marker-icon-2x.png";
import shadowUrl from "leaflet/dist/images/marker-shadow.png";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
delete (L.Icon.Default.prototype as any)._getIconUrl;

const markerIcon = new L.Icon({
  iconUrl,
  iconRetinaUrl,
  shadowUrl,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

interface Props {
  stations: StationPriceDto[];
  center: [number, number];
}

export function StationsMap({ stations, center }: Props) {
  return (
    <MapContainer center={center} zoom={13} className="stations-map">
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />
      {stations.map((station) => (
        <Marker key={station.id} position={[station.latitude, station.longitude]} icon={markerIcon}>
          <Popup>
            <strong>{station.stationName}</strong>
            {station.brandName && <em> — {station.brandName}</em>}
            <br />
            <span>{station.address}, {station.postalCode} {station.city}</span>
            <table style={{ marginTop: 6, fontSize: "0.8rem", borderCollapse: "collapse" }}>
              <tbody>
                {station.fuelPrices.map((fp) => (
                  <tr key={fp.fuelType}>
                    <td style={{ paddingRight: 8 }}>{fp.fuelType}</td>
                    <td><strong>{fp.pricePerLiter.toFixed(3)} €/L</strong></td>
                  </tr>
                ))}
              </tbody>
            </table>
            <a
              href={`https://www.google.com/maps/dir/?api=1&destination=${station.latitude},${station.longitude}`}
              target="_blank"
              rel="noopener noreferrer"
              className="directions-link"
            >
              Itineraire
            </a>
          </Popup>
        </Marker>
      ))}
    </MapContainer>
  );
}
