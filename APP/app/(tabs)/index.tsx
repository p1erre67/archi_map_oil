import { useEffect, useState } from "react";
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Alert,
} from "react-native";
import * as Location from "expo-location";
import { useNearbyStations } from "../../src/hooks/useNearbyStations";
import { useSyncPrices } from "../../src/hooks/useSyncPrices";
import { StationCard } from "../../src/components/StationCard";

export default function StationsScreen() {
  const [location, setLocation] = useState<{ lat: number; lng: number } | null>(null);
  const [radiusKm, setRadiusKm] = useState(5);

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
      const loc = await Location.getCurrentPositionAsync({});
      setLocation({ lat: loc.coords.latitude, lng: loc.coords.longitude });
    })();
  }, []);

  const { data: stations, isLoading, error, refetch } = useNearbyStations(
    location?.lat ?? 0,
    location?.lng ?? 0,
    radiusKm
  );

  const sync = useSyncPrices();

  function handleSync() {
    if (!location) return;
    sync.mutate(
      { latitude: location.lat, longitude: location.lng, radiusKm },
      {
        onSuccess: (data) => Alert.alert("Sync", data.message),
        onError: (err) => Alert.alert("Erreur", err.message),
      }
    );
  }

  if (!location) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" color="#2563eb" />
        <Text style={styles.loadingText}>Localisation en cours...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.toolbar}>
        <View style={styles.radiusSelector}>
          {[5, 10, 20].map((r) => (
            <TouchableOpacity
              key={r}
              style={[styles.radiusBtn, radiusKm === r && styles.radiusBtnActive]}
              onPress={() => setRadiusKm(r)}
            >
              <Text
                style={[styles.radiusBtnText, radiusKm === r && styles.radiusBtnTextActive]}
              >
                {r} km
              </Text>
            </TouchableOpacity>
          ))}
        </View>

        <TouchableOpacity
          style={styles.syncBtn}
          onPress={handleSync}
          disabled={sync.isPending}
        >
          <Text style={styles.syncBtnText}>
            {sync.isPending ? "Sync..." : "Synchroniser"}
          </Text>
        </TouchableOpacity>
      </View>

      {isLoading && (
        <View style={styles.centered}>
          <ActivityIndicator size="large" color="#2563eb" />
        </View>
      )}

      {error && (
        <View style={styles.centered}>
          <Text style={styles.errorText}>Erreur : {error.message}</Text>
          <TouchableOpacity onPress={() => refetch()}>
            <Text style={styles.retryText}>Reessayer</Text>
          </TouchableOpacity>
        </View>
      )}

      {stations && (
        <FlatList
          data={stations}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => <StationCard station={item} />}
          contentContainerStyle={styles.list}
          ListEmptyComponent={
            <Text style={styles.emptyText}>
              Aucune station trouvee. Essayez de synchroniser.
            </Text>
          }
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#f8fafc",
  },
  centered: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 24,
  },
  loadingText: {
    marginTop: 12,
    color: "#666",
    fontSize: 14,
  },
  toolbar: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: 12,
    backgroundColor: "#fff",
    borderBottomWidth: 1,
    borderBottomColor: "#e2e8f0",
  },
  radiusSelector: {
    flexDirection: "row",
    gap: 8,
  },
  radiusBtn: {
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: 16,
    backgroundColor: "#f1f5f9",
  },
  radiusBtnActive: {
    backgroundColor: "#2563eb",
  },
  radiusBtnText: {
    fontSize: 13,
    color: "#64748b",
    fontWeight: "500",
  },
  radiusBtnTextActive: {
    color: "#fff",
  },
  syncBtn: {
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 8,
    backgroundColor: "#2563eb",
  },
  syncBtnText: {
    color: "#fff",
    fontWeight: "600",
    fontSize: 13,
  },
  list: {
    padding: 12,
  },
  errorText: {
    color: "#dc2626",
    fontSize: 14,
    marginBottom: 8,
  },
  retryText: {
    color: "#2563eb",
    fontSize: 14,
    fontWeight: "600",
  },
  emptyText: {
    textAlign: "center",
    color: "#94a3b8",
    fontSize: 14,
    marginTop: 48,
  },
});
