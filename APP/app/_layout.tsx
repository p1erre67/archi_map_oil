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
  // Precharge les fonts vectorielles utilisees par les tab icons.
  // Sans ca, les Ionicons s'affichent comme des carres vides dans un APK standalone
  // (Expo Go les embarque deja, pas un build natif).
  const [fontsLoaded] = useFonts(Ionicons.font);

  if (!fontsLoaded) {
    return null;
  }

  return (
    <QueryClientProvider client={queryClient}>
      <LocationProvider>
        <StatusBar style="dark" />
        <Slot />
      </LocationProvider>
    </QueryClientProvider>
  );
}
