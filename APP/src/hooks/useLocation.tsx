import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from "react";
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

// Position par defaut si la geoloc echoue (Paris)
const FALLBACK: LocationState = { latitude: 48.8566, longitude: 2.3522, ready: true };

export function LocationProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<LocationState>({
    latitude: 0,
    longitude: 0,
    ready: false,
  });
  const resolved = useRef(false);

  useEffect(() => {
    console.log("[Location] Starting location flow...");

    // Securite : si rien n'a resolu apres 5s, fallback sur Paris
    const fallbackTimer = setTimeout(() => {
      console.log("[Location] Fallback timer triggered, resolved:", resolved.current);
      if (!resolved.current) {
        resolved.current = true;
        setState(FALLBACK);
      }
    }, 5000);

    (async () => {
      try {
        console.log("[Location] Requesting permission...");
        const { status } = await Location.requestForegroundPermissionsAsync();
        console.log("[Location] Permission status:", status);
        if (status !== "granted") {
          if (!resolved.current) {
            resolved.current = true;
            setState(FALLBACK);
          }
          return;
        }

        // Position deja connue du device : instantane si disponible
        console.log("[Location] Calling getLastKnownPositionAsync...");
        const cached = await Location.getLastKnownPositionAsync();
        console.log("[Location] getLastKnown result:", cached ? `${cached.coords.latitude},${cached.coords.longitude}` : "null");
        if (cached && !resolved.current) {
          resolved.current = true;
          setState({
            latitude: cached.coords.latitude,
            longitude: cached.coords.longitude,
            ready: true,
          });
        }

        // Rafraichissement en arriere-plan
        console.log("[Location] Calling getCurrentPositionAsync(Balanced)...");
        const fresh = await Location.getCurrentPositionAsync({
          accuracy: Location.Accuracy.Balanced,
        });
        console.log("[Location] getCurrentPosition result:", fresh.coords.latitude, fresh.coords.longitude);
        // Met a jour meme si le fallback a deja resolve (position plus precise)
        setState({
          latitude: fresh.coords.latitude,
          longitude: fresh.coords.longitude,
          ready: true,
        });
        resolved.current = true;
      } catch (err) {
        console.error("[Location] Failed:", err);
        if (!resolved.current) {
          resolved.current = true;
          setState(FALLBACK);
        }
      }
    })();

    return () => clearTimeout(fallbackTimer);
  }, []);

  return (
    <LocationContext.Provider value={state}>{children}</LocationContext.Provider>
  );
}

export function useLocation() {
  return useContext(LocationContext);
}
