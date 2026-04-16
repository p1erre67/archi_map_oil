import { useEffect } from "react";
import { Slot, SplashScreen } from "expo-router";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { StatusBar } from "expo-status-bar";
import { useFonts } from "expo-font";
import { Ionicons } from "@expo/vector-icons";
import { LocationProvider } from "../src/hooks/useLocation";

// Empeche le splash screen de se cacher automatiquement
// On le cache manuellement quand les fonts sont pretes
SplashScreen.preventAutoHideAsync();

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 min
      retry: 1,
    },
  },
});

export default function RootLayout() {
  const [fontsLoaded, fontError] = useFonts(Ionicons.font);

  useEffect(() => {
    // Cache le splash screen des que les fonts sont pretes OU en erreur
    if (fontsLoaded || fontError) {
      SplashScreen.hideAsync();
    }
  }, [fontsLoaded, fontError]);

  // Ne jamais retourner null — si les fonts echouent, on rend quand meme l'app
  // (les icones seront des carres vides mais l'app est utilisable)
  if (!fontsLoaded && !fontError) {
    return null;
  }

  return (
    <QueryClientProvider client={queryClient}>
      <LocationProvider>
        <StatusBar style="light" />
        <Slot />
      </LocationProvider>
    </QueryClientProvider>
  );
}
