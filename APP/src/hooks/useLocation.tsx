import { createContext, useContext, useEffect, useState } from "react";
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

export function LocationProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<LocationState>({
    latitude: 0,
    longitude: 0,
    ready: false,
  });

  useEffect(() => {
    (async () => {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== "granted") {
        Alert.alert(
          "Permission refusee",
          "PriceWatch a besoin de votre position pour trouver les stations proches."
        );
        return;
      }

      // Position reseau (WiFi/antenne, ~1-2s, ~100m de precision)
      // Suffisant pour chercher des stations dans un rayon de 5-10km
      const loc = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.Lowest,
      });
      setState({
        latitude: loc.coords.latitude,
        longitude: loc.coords.longitude,
        ready: true,
      });
    })();
  }, []);

  return (
    <LocationContext.Provider value={state}>{children}</LocationContext.Provider>
  );
}

export function useLocation() {
  return useContext(LocationContext);
}
