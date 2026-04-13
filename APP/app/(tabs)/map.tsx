import { useEffect, useState } from "react";
import { View, Text, StyleSheet, ActivityIndicator, Alert } from "react-native";
import * as Location from "expo-location";
import { useNearbyStations } from "../../src/hooks/useNearbyStations";
import { StationsMap } from "../../src/components/StationsMap";

export default function MapScreen() {
  const [location, setLocation] = useState<{ lat: number; lng: number } | null>(null);

  useEffect(() => {
    (async () => {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== "granted") {
        Alert.alert(
          "Permission refusee",
          "PriceWatch a besoin de votre position pour afficher la carte."
        );
        return;
      }
      const last = await Location.getLastKnownPositionAsync();
      if (last) {
        setLocation({ lat: last.coords.latitude, lng: last.coords.longitude });
      }
      const loc = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.Balanced,
        timeInterval: 5000,
      });
      setLocation({ lat: loc.coords.latitude, lng: loc.coords.longitude });
    })();
  }, []);

  const { data: stations, isLoading } = useNearbyStations(
    location?.lat ?? 0,
    location?.lng ?? 0,
    10
  );

  if (!location) {
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
        center={{ latitude: location.lat, longitude: location.lng }}
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
