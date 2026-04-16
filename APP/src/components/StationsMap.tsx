import { useRef, useEffect, useMemo } from "react";
import { StyleSheet, Linking } from "react-native";
import { WebView } from "react-native-webview";
import type { StationPriceDto } from "../types/api";

interface Props {
  stations: StationPriceDto[];
  center: { latitude: number; longitude: number };
  selectedFuelType?: string;
  onCenterChange?: (lat: number, lng: number) => void;
}

interface WebViewMessage {
  type: "nav" | "center";
  lat: number;
  lng: number;
  app?: "google" | "waze" | "apple";
}

export function StationsMap({ stations, center, selectedFuelType, onCenterChange }: Props) {
  const webViewRef = useRef<WebView>(null);

  // Normalise SP95/SP95-E10 → garde le plus recent sous "SP95"
  function normalizeFuels(fuels: { fuelType: string; pricePerLiter: number; updatedAt: string }[]) {
    const result: typeof fuels = [];
    let sp95: (typeof fuels)[number] | null = null;
    for (const f of fuels) {
      if (f.fuelType === "SP95" || f.fuelType === "SP95-E10") {
        if (!sp95 || new Date(f.updatedAt) > new Date(sp95.updatedAt)) {
          sp95 = { ...f, fuelType: "SP95" };
        }
      } else {
        result.push(f);
      }
    }
    if (sp95) result.push(sp95);
    return result;
  }

  // Memorise le HTML : recalcule uniquement quand stations ou center changent
  // PAS quand selectedFuelType change (gere via injectJavaScript)
  const markers = useMemo(() => stations.map((s) => {
    const fuels = normalizeFuels(s.fuelPrices);
    return {
      lat: s.latitude,
      lng: s.longitude,
      name: s.stationName,
      brand: s.brandName ?? "",
      fuelPrices: fuels.map((f) => ({
        type: f.fuelType,
        price: f.pricePerLiter,
      })),
      pricesHtml: fuels
        .map((f) => {
          const date = new Date(f.updatedAt).toLocaleDateString("fr-FR");
          return `<div class="popup-price-row"><span>${f.fuelType} : ${f.pricePerLiter.toFixed(3)} €/L</span><span class="popup-price-date">${date}</span></div>`;
        })
        .join(""),
    };
  }), [stations]);

  // Quand le fuelType change, on injecte du JS pour mettre a jour les markers
  // sans recharger la WebView (donc sans recentrer la carte)
  useEffect(() => {
    if (webViewRef.current && selectedFuelType) {
      webViewRef.current.injectJavaScript(`
        updateFuelFilter('${selectedFuelType}');
        true;
      `);
    }
  }, [selectedFuelType]);

  const html = useMemo(() => `
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
    .popup-price-row { display: flex; justify-content: space-between; align-items: center; }
    .popup-price-date { color: #94a3b8; font-size: 10px; text-align: right; }
    .popup-nav-row {
      display: flex;
      gap: 10px;
      margin-top: 4px;
    }
    .popup-nav-btn {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 2px;
      cursor: pointer;
      border: none;
      background: none;
      padding: 0;
    }
    .popup-nav-icon {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .popup-nav-icon svg {
      width: 18px;
      height: 18px;
      fill: white;
    }
    .popup-nav-icon.google { background: #EA4335; }
    .popup-nav-icon.waze { background: #33CCFF; }
    .popup-nav-icon.apple { background: #333333; }
    .popup-nav-label {
      font-size: 9px;
      color: #64748b;
      font-weight: 500;
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
    .price-marker.cheapest {
      background: #dcfce7;
      border-color: #16a34a;
      color: #15803d;
    }
    .price-marker.cheapest::after {
      border-top-color: #16a34a;
    }
    .price-marker.expensive {
      background: #fee2e2;
      border-color: #dc2626;
      color: #b91c1c;
    }
    .price-marker.expensive::after {
      border-top-color: #dc2626;
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

    var stationsData = ${JSON.stringify(markers)};
    var leafletMarkers = [];
    var currentFuel = '${selectedFuelType ?? "Tous"}';

    function getPrice(station, fuelType) {
      if (fuelType === 'Tous') return null;
      for (var i = 0; i < station.fuelPrices.length; i++) {
        var f = station.fuelPrices[i];
        if (f.type === fuelType) return f.price;
        if (fuelType === 'SP95' && f.type === 'SP95-E10') return f.price;
      }
      return null;
    }

    function buildPopup(m) {
      var popup = '<div class="popup-title">' + m.name + '</div>';
      if (m.brand) popup += '<div class="popup-brand">' + m.brand + '</div>';
      popup += '<div class="popup-prices">' + m.pricesHtml + '</div>';
      var escapedName = m.name.replace(/'/g, "\\\\'");
      popup += '<div class="popup-nav-row">';
      popup += '<button class="popup-nav-btn" onclick="askNav(\\'google\\',' + m.lat + ',' + m.lng + ')"><div class="popup-nav-icon google"><svg viewBox="0 0 24 24"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/></svg></div><span class="popup-nav-label">Google</span></button>';
      popup += '<button class="popup-nav-btn" onclick="askNav(\\'waze\\',' + m.lat + ',' + m.lng + ')"><div class="popup-nav-icon waze"><svg viewBox="0 0 24 24"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 17h-2v-2h2v2zm2.07-7.75l-.9.92C13.45 12.9 13 13.5 13 15h-2v-.5c0-1.1.45-2.1 1.17-2.83l1.24-1.26c.37-.36.59-.86.59-1.41 0-1.1-.9-2-2-2s-2 .9-2 2H8c0-2.21 1.79-4 4-4s4 1.79 4 4c0 .88-.36 1.68-.93 2.25z"/></svg></div><span class="popup-nav-label">Waze</span></button>';
      popup += '<button class="popup-nav-btn" onclick="askNav(\\'apple\\',' + m.lat + ',' + m.lng + ')"><div class="popup-nav-icon apple"><svg viewBox="0 0 24 24"><path d="M12 2L4.5 20.29l.71.71L12 18l6.79 3 .71-.71z"/></svg></div><span class="popup-nav-label">Plans</span></button>';
      popup += '</div>';
      return popup;
    }

    function renderMarkers() {
      // Supprimer les anciens markers
      leafletMarkers.forEach(function(lm) { map.removeLayer(lm); });
      leafletMarkers = [];

      // Trouver les 3 moins chers et les 3 plus chers
      var cheapestPrices = [];
      var expensivePrices = [];
      if (currentFuel !== 'Tous') {
        var allPrices = stationsData
          .map(function(m) { return getPrice(m, currentFuel); })
          .filter(function(p) { return p !== null; })
          .sort(function(a, b) { return a - b; });
        cheapestPrices = allPrices.slice(0, 3);
        expensivePrices = allPrices.slice(-3).reverse();
      }

      stationsData.forEach(function(m) {
        var popup = buildPopup(m);
        var price = getPrice(m, currentFuel);
        var marker;

        if (price !== null) {
          var isCheap = cheapestPrices.indexOf(price) !== -1;
          var isExpensive = !isCheap && expensivePrices.indexOf(price) !== -1;
          var cssClass = 'price-marker' + (isCheap ? ' cheapest' : '') + (isExpensive ? ' expensive' : '');
          var icon = L.divIcon({
            className: '',
            html: '<div class="' + cssClass + '" style="position:relative">' + price.toFixed(2) + '€</div>',
            iconSize: [60, 28],
            iconAnchor: [30, 34],
            popupAnchor: [0, -36]
          });
          marker = L.marker([m.lat, m.lng], { icon: icon });
        } else {
          marker = L.marker([m.lat, m.lng]);
        }

        marker.addTo(map).bindPopup(popup);
        leafletMarkers.push(marker);
      });
    }

    function updateFuelFilter(fuel) {
      currentFuel = fuel;
      renderMarkers();
    }

    // Rendu initial
    renderMarkers();

    map.on('moveend', function() {
      var c = map.getCenter();
      window.ReactNativeWebView.postMessage(JSON.stringify({
        type: 'center',
        lat: c.lat,
        lng: c.lng
      }));
    });

    function askNav(app, lat, lng) {
      window.ReactNativeWebView.postMessage(JSON.stringify({
        type: 'nav',
        app: app,
        lat: lat,
        lng: lng
      }));
    }
  </script>
</body>
  // deps = primitives uniquement (pas d'objet "center" qui change de reference a chaque render)
  // eslint-disable-next-line react-hooks/exhaustive-deps
</html>`, [markers, center.latitude, center.longitude]);

  function handleMessage(event: { nativeEvent: { data: string } }) {
    try {
      const message: WebViewMessage = JSON.parse(event.nativeEvent.data);

      if (message.type === "center" && onCenterChange) {
        onCenterChange(message.lat, message.lng);
        return;
      }

      if (message.type === "nav" && message.app) {
        const { lat, lng, app } = message;

        const urls: Record<string, string> = {
          google: `https://www.google.com/maps/dir/?api=1&destination=${lat},${lng}`,
          waze: `waze://?ll=${lat},${lng}&navigate=yes`,
          apple: `https://maps.apple.com/?daddr=${lat},${lng}`,
        };

        Linking.openURL(urls[app]).catch(() => {
          if (app === "waze") {
            Linking.openURL(`https://waze.com/ul?ll=${lat},${lng}&navigate=yes`);
          }
        });
      }
    } catch {
      // message invalide, on ignore
    }
  }

  return (
    <WebView
      ref={webViewRef}
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
