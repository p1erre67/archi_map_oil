import { Slot } from "expo-router";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { StatusBar } from "expo-status-bar";
import { useFonts } from "expo-font";
import { Ionicons } from "@expo/vector-icons";
import { LocationProvider } from "../src/hooks/useLocation";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 min
      retry: 1,
    },
  },
});

export default function RootLayout() {
  // Charge les fonts au runtime EN PLUS de l'embarquement natif (ceinture et bretelles).
  // Ne bloque jamais l'UI : on rend toujours, les icones apparaissent des qu'elles sont pretes.
  const [fontsLoaded, fontError] = useFonts(Ionicons.font);
  console.log("[Fonts] loaded:", fontsLoaded, "error:", fontError);

  return (
    <QueryClientProvider client={queryClient}>
      <LocationProvider>
        <StatusBar style="light" />
        <Slot />
      </LocationProvider>
    </QueryClientProvider>
  );
}
