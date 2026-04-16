import { useEffect, useState, useCallback } from "react";
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Alert,
} from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useNearbyStations } from "../../src/hooks/useNearbyStations";
import { useSyncPrices } from "../../src/hooks/useSyncPrices";
import { useFilters } from "../../src/hooks/useFilters";
import { StationsMap } from "../../src/components/StationsMap";

const FUEL_TYPES = ["Tous", "Gazole", "SP95", "SP98", "E85"];

export default function MapScreen() {
  const {
    activeLat, activeLng, radiusKm, setRadiusKm,
    locationReady, fuelType, setFuelType, setManualCoords,
  } = useFilters();

  const [mapCenter, setMapCenter] = useState({ lat: activeLat, lng: activeLng });

  // Force le rayon a 8km quand on arrive sur la carte
  useEffect(() => {
    setRadiusKm(8);
  }, []);

  const { data: stations, isLoading } = useNearbyStations(
    activeLat,
    activeLng,
    radiusKm
  );

  const sync = useSyncPrices();

  const handleCenterChange = useCallback((lat: number, lng: number) => {
    setMapCenter({ lat, lng });
  }, []);

  function handleRefresh() {
    // Met a jour les filtres partages avec les coords du centre de la carte
    setManualCoords({ lat: mapCenter.lat, lng: mapCenter.lng });
    // Sync les stations depuis l'API gouv pour cette zone
    sync.mutate(
      { latitude: mapCenter.lat, longitude: mapCenter.lng, radiusKm },
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
      <View style={styles.toolbar}>
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          style={{ flexShrink: 1 }}
          contentContainerStyle={styles.fuelBar}
        >
          {FUEL_TYPES.map((ft) => (
            <TouchableOpacity
              key={ft}
              style={[styles.fuelChip, fuelType === ft && styles.fuelChipActive]}
              onPress={() => setFuelType(ft)}
            >
              <Text style={[styles.fuelChipText, fuelType === ft && styles.fuelChipTextActive]}>
                {ft}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>

        <TouchableOpacity
          style={[styles.refreshBtn, sync.isPending && styles.refreshBtnDisabled]}
          onPress={handleRefresh}
          disabled={sync.isPending}
        >
          <Ionicons
            name="refresh"
            size={18}
            color={sync.isPending ? "#94a3b8" : "#2563eb"}
          />
        </TouchableOpacity>
      </View>

      <StationsMap
        stations={stations ?? []}
        center={{ latitude: activeLat, longitude: activeLng }}
        selectedFuelType={fuelType}
        onCenterChange={handleCenterChange}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  toolbar: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#f8fafc",
    borderBottomWidth: 0.5,
    borderBottomColor: "#e2e8f0",
    paddingRight: 8,
    height: 36,
  },
  fuelBar: {
    flexDirection: "row",
    gap: 6,
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  fuelChip: {
    paddingHorizontal: 10,
    paddingVertical: 3,
    borderRadius: 12,
    backgroundColor: "#f1f5f9",
  },
  fuelChipActive: {
    backgroundColor: "#2563eb",
  },
  fuelChipText: {
    fontSize: 13,
    fontWeight: "500",
    color: "#64748b",
  },
  fuelChipTextActive: {
    color: "#ffffff",
  },
  refreshBtn: {
    padding: 6,
    borderRadius: 8,
    backgroundColor: "#eff6ff",
    borderWidth: 1,
    borderColor: "#bfdbfe",
  },
  refreshBtnDisabled: {
    backgroundColor: "#f1f5f9",
    borderColor: "#e2e8f0",
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
