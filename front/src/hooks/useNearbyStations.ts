import { useQuery } from "@tanstack/react-query";
import { fetchApi } from "../api/client";
import type { StationPriceDto } from "../types/api";

export function useNearbyStations(
  latitude: number,
  longitude: number,
  radiusKm: number
) {
  return useQuery({
    queryKey: ["nearby-stations", latitude, longitude, radiusKm],
    queryFn: () =>
      fetchApi<StationPriceDto[]>(
        `/prices/nearby?latitude=${latitude}&longitude=${longitude}&radiusKm=${radiusKm}`
      ),
    enabled: latitude !== 0 && longitude !== 0,
  });
}
