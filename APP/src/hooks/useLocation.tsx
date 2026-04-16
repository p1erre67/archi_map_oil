import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import * as Location from "expo-location";

interface LocationState {
  latitude: number;
  longitude: number;
  ready: boolean;
}

const LocationContext = createContext<LocationState>({
  latitude: 0,
  longitude: 0,
  ready: false,
});

function withTimeout<T>(promise: Promise<T>, ms: number): Promise<T> {
  return Promise.race([
    promise,
    new Promise<T>((_, reject) =>
      setTimeout(() => reject(new Error(`Timeout apres ${ms}ms`)), ms)
    ),
  ]);
}

// Position par defaut si la geoloc echoue (Paris)
const FALLBACK: LocationState = { latitude: 48.8566, longitude: 2.3522, ready: true };

export function LocationProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<LocationState>({
    latitude: 0,
    longitude: 0,
    ready: false,
  });

  useEffect(() => {
    (async () => {
      try {
        const { status } = await Location.requestForegroundPermissionsAsync();
        if (status !== "granted") {
          // Permission refusee → on demarre avec Paris, l'utilisateur peut choisir une ville
          setState(FALLBACK);
          return;
        }

        // Position deja connue du device : instantane si disponible
        const cached = await Location.getLastKnownPositionAsync();
        if (cached) {
          setState({
            latitude: cached.coords.latitude,
            longitude: cached.coords.longitude,
            ready: true,
          });
        }

        // Rafraichissement : Accuracy.Balanced utilise reseau + GPS via FusedLocationProvider
        const fresh = await withTimeout(
          Location.getCurrentPositionAsync({
            accuracy: Location.Accuracy.Balanced,
          }),
          5000
        );
        setState({
          latitude: fresh.coords.latitude,
          longitude: fresh.coords.longitude,
          ready: true,
        });
      } catch (err) {
        console.error("[useLocation] Failed to get location:", err);
        // Timeout ou erreur GPS → on demarre avec Paris plutot que de bloquer l'UI
        if (!state.ready) {
          setState(FALLBACK);
        }
      }
    })();
  }, []);

  return (
    <LocationContext.Provider value={state}>{children}</LocationContext.Provider>
  );
}

export function useLocation() {
  return useContext(LocationContext);
}
