import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import * as Location from "expo-location";
import { Alert } from "react-native";

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
          Alert.alert(
            "Permission refusee",
            "PriceWatch a besoin de votre position pour trouver les stations proches."
          );
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
        // (Accuracy.Lowest correspond a PRIORITY_PASSIVE qui hang si aucune autre app
        // ne demande la localisation.)
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
        Alert.alert(
          "Erreur de localisation",
          err instanceof Error ? err.message : "Unknown error"
        );
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
