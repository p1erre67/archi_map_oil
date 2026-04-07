import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";
import type { GlobalPricePointDto } from "../types/api";

interface PriceChartProps {
  data: GlobalPricePointDto[];
}

export function PriceChart({ data }: PriceChartProps) {
  const formatted = data.map((d) => ({
    ...d,
    date: new Date(d.date).toLocaleDateString("fr-FR"),
  }));

  return (
    <ResponsiveContainer width="100%" height={400}>
      <LineChart data={formatted}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="date" />
        <YAxis domain={["auto", "auto"]} unit="€" />
        <Tooltip />
        <Legend />
        <Line
          type="monotone"
          dataKey="averagePrice"
          stroke="#2563eb"
          name="Prix moyen"
          dot={false}
        />
        <Line
          type="monotone"
          dataKey="minPrice"
          stroke="#16a34a"
          name="Min"
          dot={false}
          strokeDasharray="5 5"
        />
        <Line
          type="monotone"
          dataKey="maxPrice"
          stroke="#dc2626"
          name="Max"
          dot={false}
          strokeDasharray="5 5"
        />
      </LineChart>
    </ResponsiveContainer>
  );
}
