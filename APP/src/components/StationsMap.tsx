import { StyleSheet, Linking, Platform } from "react-native";
import { WebView } from "react-native-webview";
import type { StationPriceDto } from "../types/api";

interface Props {
  stations: StationPriceDto[];
  center: { latitude: number; longitude: number };
  selectedFuelType?: string;
  onCenterChange?: (lat: number, lng: number) => void;
}

interface WebViewMessage {
  type: "directions" | "center";
  lat: number;
  lng: number;
  name?: string;
}

export function StationsMap({ stations, center, selectedFuelType, onCenterChange }: Props) {
  const markers = stations.map((s) => {
    const matchedFuel = selectedFuelType && selectedFuelType !== "Tous"
      ? s.fuelPrices.find(
          (f) => f.fuelType === selectedFuelType || (selectedFuelType === "SP95" && f.fuelType === "SP95-E10")
        )
      : null;

    return {
      lat: s.latitude,
      lng: s.longitude,
      name: s.stationName,
      brand: s.brandName ?? "",
      priceLabel: matchedFuel ? matchedFuel.pricePerLiter.toFixed(2) + "€" : "",
      prices: s.fuelPrices
        .map((f) => `${f.fuelType} : ${f.pricePerLiter.toFixed(3)} €/L`)
        .join("<br/>"),
    };
  });

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
    .popup-prices { font-size: 13px; color: #2563eb; margin-bottom: 8px; }
    .popup-directions {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      background: #2563eb;
      color: white;
      width: 36px;
      height: 36px;
      border-radius: 50%;
      cursor: pointer;
      border: none;
      padding: 0;
    }
    .popup-directions svg {
      width: 20px;
      height: 20px;
      fill: white;
    }
    .price-marker {
      background: #ffffff;
      border: 1.5px solid #2563eb;
      border-radius: 8px;
      padding: 3px 7px;
      font-size: 12px;
      font-weight: 700;
      color: #0f172a;
      white-space: nowrap;
      box-shadow: 0 2px 6px rgba(0,0,0,0.15);
      text-align: center;
      line-height: 1.2;
    }
    .price-marker::after {
      content: '';
      position: absolute;
      bottom: -6px;
      left: 50%;
      transform: translateX(-50%);
      width: 0;
      height: 0;
      border-left: 5px solid transparent;
      border-right: 5px solid transparent;
      border-top: 6px solid #2563eb;
    }
    .price-marker.no-price {
      border-color: #94a3b8;
      color: #94a3b8;
    }
    .price-marker.no-price::after {
      border-top-color: #94a3b8;
    }
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
      popup += '<button class="popup-directions" onclick="askDirections(' + m.lat + ',' + m.lng + ',\\'' + m.name.replace(/'/g, "\\\\'") + '\\')"><svg viewBox="0 0 24 24"><path d="M21.71 11.29l-9-9a1 1 0 0 0-1.42 0l-9 9a1 1 0 0 0 0 1.42l9 9a1 1 0 0 0 1.42 0l9-9a1 1 0 0 0 0-1.42zM14 14.5V12h-4v3H8v-4a1 1 0 0 1 1-1h5V7.5l3.5 3.5z"/></svg></button>';

      if (m.priceLabel) {
        var icon = L.divIcon({
          className: '',
          html: '<div class="price-marker" style="position:relative">' + m.priceLabel + '</div>',
          iconSize: [60, 28],
          iconAnchor: [30, 34],
          popupAnchor: [0, -36]
        });
        L.marker([m.lat, m.lng], { icon: icon }).addTo(map).bindPopup(popup);
      } else {
        L.marker([m.lat, m.lng]).addTo(map).bindPopup(popup);
      }
    });

    map.on('moveend', function() {
      var c = map.getCenter();
      window.ReactNativeWebView.postMessage(JSON.stringify({
        type: 'center',
        lat: c.lat,
        lng: c.lng
      }));
    });

    function askDirections(lat, lng, name) {
      window.ReactNativeWebView.postMessage(JSON.stringify({
        type: 'directions',
        lat: lat,
        lng: lng,
        name: name
      }));
    }
  </script>
</body>
</html>`;

  function handleMessage(event: { nativeEvent: { data: string } }) {
    try {
      const message: WebViewMessage = JSON.parse(event.nativeEvent.data);

      if (message.type === "center" && onCenterChange) {
        onCenterChange(message.lat, message.lng);
        return;
      }

      if (message.type === "directions" && message.name) {
        const url = Platform.OS === "android"
          ? `geo:${message.lat},${message.lng}?q=${message.lat},${message.lng}(${encodeURIComponent(message.name)})`
          : `https://maps.apple.com/?daddr=${message.lat},${message.lng}`;

        Linking.openURL(url);
      }
    } catch {
      // message invalide, on ignore
    }
  }

  return (
    <WebView
      style={styles.map}
      originWhitelist={["*"]}
      source={{ html }}
      javaScriptEnabled
      scrollEnabled={false}
      onMessage={handleMessage}
    />
  );
}

const styles = StyleSheet.create({
  map: {
    flex: 1,
    minHeight: 400,
  },
});
