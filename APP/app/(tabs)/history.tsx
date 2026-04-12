import { useState } from "react";
import {
  View,
  Text,
  TouchableOpacity,
  ScrollView,
  StyleSheet,
  ActivityIndicator,
} from "react-native";
import { useGlobalPriceHistory } from "../../src/hooks/useGlobalPriceHistory";
import { PriceChart } from "../../src/components/PriceChart";

const FUEL_TYPES = ["Gazole", "SP95", "SP98", "E10", "E85", "GPLc"];

export default function HistoryScreen() {
  const [selectedFuel, setSelectedFuel] = useState("Gazole");

  // 30 derniers jours par defaut
  const to = new Date().toISOString().split("T")[0];
  const from = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)
    .toISOString()
    .split("T")[0];

  const { data, isLoading, error } = useGlobalPriceHistory(selectedFuel, from, to);

  return (
    <ScrollView style={styles.container}>
      <Text style={styles.title}>Evolution des prix</Text>

      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.fuelSelector}
      >
        {FUEL_TYPES.map((fuel) => (
          <TouchableOpacity
            key={fuel}
            style={[styles.fuelBtn, selectedFuel === fuel && styles.fuelBtnActive]}
            onPress={() => setSelectedFuel(fuel)}
          >
            <Text
              style={[
                styles.fuelBtnText,
                selectedFuel === fuel && styles.fuelBtnTextActive,
              ]}
            >
              {fuel}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      <Text style={styles.period}>
        Du {from} au {to}
      </Text>

      {isLoading && (
        <View style={styles.centered}>
          <ActivityIndicator size="large" color="#2563eb" />
        </View>
      )}

      {error && (
        <Text style={styles.errorText}>Erreur : {error.message}</Text>
      )}

      {data && <PriceChart data={data} fuelType={selectedFuel} />}

      {data && data.length > 0 && (
        <View style={styles.statsContainer}>
          <Text style={styles.statsTitle}>Statistiques</Text>
          <View style={styles.statsRow}>
            <StatBox
              label="Min"
              value={`${Math.min(...data.map((d) => d.minPrice)).toFixed(3)} EUR`}
              color="#16a34a"
            />
            <StatBox
              label="Moyenne"
              value={`${(data.reduce((s, d) => s + d.averagePrice, 0) / data.length).toFixed(3)} EUR`}
              color="#2563eb"
            />
            <StatBox
              label="Max"
              value={`${Math.max(...data.map((d) => d.maxPrice)).toFixed(3)} EUR`}
              color="#dc2626"
            />
          </View>
        </View>
      )}
    </ScrollView>
  );
}

function StatBox({
  label,
  value,
  color,
}: {
  label: string;
  value: string;
  color: string;
}) {
  return (
    <View style={styles.statBox}>
      <Text style={styles.statLabel}>{label}</Text>
      <Text style={[styles.statValue, { color }]}>{value}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#f8fafc",
    padding: 16,
  },
  title: {
    fontSize: 20,
    fontWeight: "700",
    marginBottom: 12,
  },
  fuelSelector: {
    gap: 8,
    paddingBottom: 8,
  },
  fuelBtn: {
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 20,
    backgroundColor: "#f1f5f9",
  },
  fuelBtnActive: {
    backgroundColor: "#2563eb",
  },
  fuelBtnText: {
    fontSize: 14,
    color: "#64748b",
    fontWeight: "500",
  },
  fuelBtnTextActive: {
    color: "#fff",
  },
  period: {
    fontSize: 12,
    color: "#94a3b8",
    marginTop: 8,
    marginBottom: 4,
  },
  centered: {
    padding: 48,
    alignItems: "center",
  },
  errorText: {
    color: "#dc2626",
    textAlign: "center",
    marginTop: 24,
  },
  statsContainer: {
    marginTop: 16,
    marginBottom: 32,
  },
  statsTitle: {
    fontSize: 16,
    fontWeight: "600",
    marginBottom: 8,
  },
  statsRow: {
    flexDirection: "row",
    gap: 12,
  },
  statBox: {
    flex: 1,
    backgroundColor: "#fff",
    borderRadius: 12,
    padding: 12,
    alignItems: "center",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
    elevation: 1,
  },
  statLabel: {
    fontSize: 12,
    color: "#94a3b8",
    marginBottom: 4,
  },
  statValue: {
    fontSize: 16,
    fontWeight: "700",
  },
});
