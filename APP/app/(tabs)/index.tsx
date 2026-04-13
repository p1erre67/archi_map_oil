import { useState } from "react";
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Alert,
} from "react-native";
import { useNearbyStations } from "../../src/hooks/useNearbyStations";
import { useSyncPrices } from "../../src/hooks/useSyncPrices";
import { StationCard } from "../../src/components/StationCard";
import { CityPickerModal } from "../../src/components/CityPickerModal";
import { useLocation } from "../../src/hooks/useLocation";
import type { City } from "../../src/hooks/useCitySearch";

export default function StationsScreen() {
  const deviceLocation = useLocation();
  const [selectedCity, setSelectedCity] = useState<City | null>(null);
  const [radiusKm, setRadiusKm] = useState(4);
  const [pickerVisible, setPickerVisible] = useState(false);

  const activeLat = selectedCity?.latitude ?? deviceLocation.latitude;
  const activeLng = selectedCity?.longitude ?? deviceLocation.longitude;
  const locationReady = selectedCity !== null || deviceLocation.ready;

  const { data: stations, isLoading, error, refetch } = useNearbyStations(
    activeLat,
    activeLng,
    radiusKm
  );

  const sync = useSyncPrices();

  function handleSync() {
    if (!locationReady) return;
    sync.mutate(
      { latitude: activeLat, longitude: activeLng, radiusKm },
      {
        onSuccess: (data) => Alert.alert("Sync", data.message),
        onError: (err) => Alert.alert("Erreur", err.message),
      }
    );
  }

  if (!locationReady) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" color="#2563eb" />
        <Text style={styles.loadingText}>Localisation en cours...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.locationBar}>
        <View style={styles.locationInfo}>
          <Text style={styles.locationLabel}>
            {selectedCity ? selectedCity.nom : "Ma position"}
          </Text>
          {selectedCity && (
            <TouchableOpacity onPress={() => setSelectedCity(null)}>
              <Text style={styles.resetBtn}>Reinitialiser</Text>
            </TouchableOpacity>
          )}
        </View>
        <TouchableOpacity
          style={styles.pickerBtn}
          onPress={() => setPickerVisible(true)}
        >
          <Text style={styles.pickerBtnText}>+ Ville</Text>
        </TouchableOpacity>
      </View>

      <View style={styles.toolbar}>
        <View style={styles.radiusSelector}>
          {[4, 8].map((r) => (
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

      <CityPickerModal
        visible={pickerVisible}
        onClose={() => setPickerVisible(false)}
        onSelect={setSelectedCity}
      />
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
  locationBar: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: 12,
    backgroundColor: "#fff",
    borderBottomWidth: 1,
    borderBottomColor: "#e2e8f0",
  },
  locationInfo: {
    flex: 1,
    flexDirection: "row",
    alignItems: "center",
    gap: 10,
  },
  locationLabel: {
    fontSize: 15,
    fontWeight: "600",
    color: "#0f172a",
  },
  resetBtn: {
    fontSize: 12,
    color: "#64748b",
    textDecorationLine: "underline",
  },
  pickerBtn: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 6,
    backgroundColor: "#eff6ff",
    borderWidth: 1,
    borderColor: "#bfdbfe",
  },
  pickerBtnText: {
    color: "#2563eb",
    fontSize: 13,
    fontWeight: "600",
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
