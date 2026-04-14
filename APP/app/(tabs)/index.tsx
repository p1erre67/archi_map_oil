import { View, Text, StyleSheet, ActivityIndicator } from "react-native";
import { useNearbyStations } from "../../src/hooks/useNearbyStations";
import { useLocation } from "../../src/hooks/useLocation";
import { StationsMap } from "../../src/components/StationsMap";

export default function MapScreen() {
  const location = useLocation();

  const { data: stations, isLoading } = useNearbyStations(
    location.latitude,
    location.longitude,
    10
  );

  if (!location.ready) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" color="#2563eb" />
        <Text style={styles.loadingText}>Localisation en cours...</Text>
      </View>
    );
  }

  if (isLoading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" color="#2563eb" />
        <Text style={styles.loadingText}>Chargement des stations...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <StationsMap
        stations={stations ?? []}
        center={{ latitude: location.latitude, longitude: location.longitude }}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  centered: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  loadingText: {
    marginTop: 12,
    color: "#666",
    fontSize: 14,
  },
});
