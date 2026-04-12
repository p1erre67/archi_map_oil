import { StyleSheet } from "react-native";
import { WebView } from "react-native-webview";
import type { StationPriceDto } from "../types/api";

interface Props {
  stations: StationPriceDto[];
  center: { latitude: number; longitude: number };
}

export function StationsMap({ stations, center }: Props) {
  const markers = stations.map((s) => ({
    lat: s.latitude,
    lng: s.longitude,
    name: s.stationName,
    brand: s.brandName ?? "",
    prices: s.fuelPrices
      .map((f) => `${f.fuelType} : ${f.pricePerLiter.toFixed(3)} €/L`)
      .join("<br/>"),
  }));

  const html = `
<!DOCTYPE html>
<html>
<head>
  <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
  <style>
    * { margin: 0; padding: 0; }
    #map { width: 100vw; height: 100vh; }
    .popup-title { font-weight: 600; font-size: 14px; margin-bottom: 2px; }
    .popup-brand { color: #666; font-size: 12px; margin-bottom: 4px; }
    .popup-prices { font-size: 13px; color: #2563eb; }
  </style>
</head>
<body>
  <div id="map"></div>
  <script>
    var map = L.map('map').setView([${center.latitude}, ${center.longitude}], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    var markers = ${JSON.stringify(markers)};

    markers.forEach(function(m) {
      var popup = '<div class="popup-title">' + m.name + '</div>';
      if (m.brand) popup += '<div class="popup-brand">' + m.brand + '</div>';
      popup += '<div class="popup-prices">' + m.prices + '</div>';

      L.marker([m.lat, m.lng]).addTo(map).bindPopup(popup);
    });
  </script>
</body>
</html>`;

  return (
    <WebView
      style={styles.map}
      originWhitelist={["*"]}
      source={{ html }}
      javaScriptEnabled
      scrollEnabled={false}
    />
  );
}

const styles = StyleSheet.create({
  map: {
    flex: 1,
    minHeight: 400,
  },
});
