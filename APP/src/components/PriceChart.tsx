import { View, Text, StyleSheet, Dimensions } from "react-native";
import { LineChart } from "react-native-chart-kit";
import type { GlobalPricePointDto } from "../types/api";

interface Props {
  data: GlobalPricePointDto[];
  fuelType: string;
}

export function PriceChart({ data, fuelType }: Props) {
  if (data.length === 0) {
    return (
      <View style={styles.empty}>
        <Text style={styles.emptyText}>Aucune donnee pour {fuelType}</Text>
      </View>
    );
  }

  const labels = data.map((p) =>
    new Date(p.date).toLocaleDateString("fr-FR", { day: "2-digit", month: "2-digit" })
  );
  // Show max ~6 labels to avoid overlap
  const step = Math.max(1, Math.floor(labels.length / 6));
  const displayLabels = labels.map((l, i) => (i % step === 0 ? l : ""));

  const chartData = {
    labels: displayLabels,
    datasets: [
      {
        data: data.map((p) => p.averagePrice),
        color: () => "#2563eb",
        strokeWidth: 2,
      },
      {
        data: data.map((p) => p.minPrice),
        color: () => "#16a34a",
        strokeWidth: 1,
      },
      {
        data: data.map((p) => p.maxPrice),
        color: () => "#dc2626",
        strokeWidth: 1,
      },
    ],
    legend: ["Moyenne", "Min", "Max"],
  };

  const screenWidth = Dimensions.get("window").width - 32;

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Evolution {fuelType} (EUR/L)</Text>
      <LineChart
        data={chartData}
        width={screenWidth}
        height={220}
        yAxisSuffix=" EUR"
        chartConfig={{
          backgroundColor: "#fff",
          backgroundGradientFrom: "#fff",
          backgroundGradientTo: "#fff",
          decimalPlaces: 3,
          color: (opacity = 1) => `rgba(37, 99, 235, ${opacity})`,
          labelColor: () => "#666",
          propsForDots: { r: "2" },
        }}
        bezier
        style={styles.chart}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    marginVertical: 12,
  },
  title: {
    fontSize: 16,
    fontWeight: "600",
    marginBottom: 8,
    textAlign: "center",
  },
  chart: {
    borderRadius: 12,
  },
  empty: {
    padding: 24,
    alignItems: "center",
  },
  emptyText: {
    color: "#888",
    fontSize: 14,
  },
});
