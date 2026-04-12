import { View, Text, StyleSheet } from "react-native";
import type { StationPriceDto } from "../types/api";

interface Props {
  station: StationPriceDto;
}

export function StationCard({ station }: Props) {
  return (
    <View style={styles.card}>
      <View style={styles.header}>
        <Text style={styles.name}>{station.stationName}</Text>
        {station.brandName ? (
          <Text style={styles.brand}>{station.brandName}</Text>
        ) : null}
      </View>

      <Text style={styles.address}>
        {station.address}, {station.postalCode} {station.city}
      </Text>

      <View style={styles.pricesContainer}>
        {station.fuelPrices.map((fuel) => (
          <View key={fuel.fuelType} style={styles.priceRow}>
            <Text style={styles.fuelType}>{fuel.fuelType}</Text>
            <Text style={styles.price}>{fuel.pricePerLiter.toFixed(3)} EUR/L</Text>
          </View>
        ))}
      </View>

      <Text style={styles.updated}>
        MAJ : {new Date(station.lastUpdated).toLocaleDateString("fr-FR")}
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: "#fff",
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  header: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 4,
  },
  name: {
    fontSize: 16,
    fontWeight: "600",
    flex: 1,
  },
  brand: {
    fontSize: 12,
    color: "#666",
    backgroundColor: "#f0f0f0",
    paddingHorizontal: 8,
    paddingVertical: 2,
    borderRadius: 4,
    overflow: "hidden",
  },
  address: {
    fontSize: 13,
    color: "#888",
    marginBottom: 12,
  },
  pricesContainer: {
    gap: 6,
  },
  priceRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  fuelType: {
    fontSize: 14,
    color: "#333",
  },
  price: {
    fontSize: 15,
    fontWeight: "700",
    color: "#2563eb",
  },
  updated: {
    fontSize: 11,
    color: "#aaa",
    marginTop: 10,
    textAlign: "right",
  },
});
