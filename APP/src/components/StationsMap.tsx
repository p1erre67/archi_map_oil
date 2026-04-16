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
      gap: 8px;
      margin-top: 6px;
    }
    .popup-nav-btn {
      cursor: pointer;
      border: none;
      background: none;
      padding: 0;
      width: 30px;
      height: 30px;
    }
    .popup-nav-btn svg {
      width: 30px;
      height: 30px;
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
      popup += '<div class="popup-nav-row">';
      popup += '<button class="popup-nav-btn" onclick="askNav(\\'google\\',' + m.lat + ',' + m.lng + ')"><svg viewBox="0 0 232597 333333" fill-rule="evenodd" clip-rule="evenodd"><path d="M151444 5419C140355 1916 128560 0 116311 0 80573 0 48591 16155 27269 41534l54942 46222 69232-82338z" fill="#1a73e8"/><path d="M27244 41534C10257 61747 0 87832 0 116286c0 21876 4360 39594 11517 55472l70669-84002-54942-46222z" fill="#ea4335"/><path d="M116311 71828c24573 0 44483 19910 44483 44483 0 10938-3957 20969-10509 28706 0 0 35133-41786 69232-82313-14089-27093-38510-47936-68048-57286L82186 87756c8166-9753 20415-15928 34125-15928z" fill="#4285f4"/><path d="M116311 160769c-24573 0-44483-19910-44483-44483 0-10863 3906-20818 10358-28555l-70669 84027c12072 26791 32159 48289 52851 75381l85891-102122c-8141 9628-20339 15752-33948 15752z" fill="#fbbc04"/><path d="M148571 275014c38787-60663 84026-88210 84026-158728 0-19331-4738-37552-13080-53581L64393 247140c6578 8620 13206 17793 19683 27900 23590 36444 17037 58294 32260 58294 15172 0 8644-21876 32235-58320z" fill="#34a853"/></svg></button>';
      popup += '<button class="popup-nav-btn" onclick="askNav(\\'waze\\',' + m.lat + ',' + m.lng + ')"><svg viewBox="0 0 122.71 122.88"><path fill="#FFFFFF" d="M55.14,104.21c4.22,0,8.44,0.19,12.66-0.09c3.84-0.19,7.88-0.56,11.63-1.5c29.82-7.31,45.76-40.23,32.72-68.07C104.27,17.76,90.77,8.19,72.3,6.22c-14.16-1.5-26.82,2.72-37.51,12.28c-10.5,9.47-15.94,21.28-16.31,35.44c-0.09,3.28,0,6.66,0,9.94C18.38,71.02,14.35,76.55,7.5,78.7c-0.09,0-0.28,0.19-0.38,0.19c2.63,6.94,13.31,17.16,19.97,19.69C35.45,87.14,52.32,91.18,55.14,104.21z"/><path d="M54.95,110.49c-1.03,4.69-3.56,8.16-7.69,10.31c-5.25,2.72-10.6,2.63-15.57-0.56c-5.16-3.28-7.41-8.25-7.03-14.35c0.09-1.03-0.19-1.41-1.03-1.88c-9.1-4.78-16.31-11.44-21.28-20.44c-0.94-1.78-1.69-3.66-2.16-5.63c-0.66-2.72,0.38-4.03,3.19-4.31c3.38-0.38,6.38-1.69,7.88-4.88c0.66-1.41,1.03-3.09,1.03-4.69c0.19-4.03,0-8.06,0.19-12.1c1.03-15.57,7.5-28.5,19.32-38.63C42.67,3.97,55.42-0.43,69.76,0.03c25.04,0.94,46.51,18.57,51.57,43.23c4.59,22.32-2.34,40.98-20.07,55.51c-1.03,0.84-2.16,1.69-3.38,2.44c-0.66,0.47-0.84,0.84-0.56,1.59c2.34,7.13-0.94,15-7.5,18.38c-8.91,4.41-19.22-0.09-21.94-9.66c-0.09-0.38-0.56-0.84-0.84-0.84C63.11,110.4,59.07,110.49,54.95,110.49z M55.14,104.21c4.22,0,8.44,0.19,12.66-0.09c3.84-0.19,7.88-0.56,11.63-1.5c29.82-7.31,45.76-40.23,32.72-68.07C104.27,17.76,90.77,8.19,72.3,6.22c-14.16-1.5-26.82,2.72-37.51,12.28c-10.5,9.47-15.94,21.28-16.31,35.44c-0.09,3.28,0,6.66,0,9.94C18.38,71.02,14.35,76.55,7.5,78.7c-0.09,0-0.28,0.19-0.38,0.19c2.63,6.94,13.31,17.16,19.97,19.69C35.45,87.14,52.32,91.18,55.14,104.21z"/><path d="M74.92,79.74c-11.07-0.56-18.38-4.97-23.07-13.78c-1.13-2.16-0.09-4.31,2.06-4.78c1.31-0.28,2.53,0.66,3.47,2.16c1.22,1.88,2.44,3.75,4.03,5.25c8.81,8.34,23.25,5.72,28.79-5.06c0.66-1.31,1.5-2.34,3.09-2.34c2.34,0.09,3.66,2.44,2.63,4.59c-2.91,5.91-7.5,10.22-13.69,12.28C79.51,78.99,76.7,79.36,74.92,79.74z"/><path d="M55.32,48.98c-3.38,0-6.09-2.72-6.09-6.09s2.72-6.09,6.09-6.09s6.09,2.72,6.09,6.09C61.42,46.17,58.7,48.98,55.32,48.98z"/><path d="M98.27,42.79c0,3.38-2.72,6.09-6,6.19c-3.38,0-6.09-2.63-6.09-6.09c0-3.38,2.63-6.09,6-6.19C95.46,36.7,98.17,39.42,98.27,42.79z"/></svg></button>';
      popup += '<button class="popup-nav-btn" onclick="askNav(\\'apple\\',' + m.lat + ',' + m.lng + ')"><svg viewBox="0 0 640 640"><path d="M494.782 340.02c-.803-81.025 66.084-119.907 69.072-121.832-37.595-54.993-96.167-62.552-117.037-63.402-49.843-5.032-97.242 29.362-122.565 29.362-25.253 0-64.277-28.607-105.604-27.85-54.32.803-104.4 31.594-132.403 80.245C29.81 334.457 71.81 479.58 126.816 558.976c26.87 38.882 58.914 82.56 100.997 81 40.512-1.594 55.843-26.244 104.848-26.244 48.993 0 62.753 26.245 105.64 25.406 43.606-.803 71.232-39.638 97.925-78.65 30.887-45.12 43.548-88.75 44.316-90.994-.969-.437-85.029-32.634-85.879-129.439l.118-.035zM414.23 102.178C436.553 75.095 451.636 37.5 447.514-.024c-32.162 1.311-71.163 21.437-94.253 48.485-20.729 24.012-38.836 62.28-33.993 99.036 35.918 2.8 72.591-18.248 94.926-45.272l.036-.047z"/></svg></button>';
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
