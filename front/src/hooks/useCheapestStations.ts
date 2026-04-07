import { useQuery } from "@tanstack/react-query";
import { fetchApi } from "../api/client";
import type { StationPriceDto } from "../types/api";

export function useCheapestStations(fuelType: string, limit: number = 10) {
  return useQuery({
    queryKey: ["cheapest-stations", fuelType, limit],
    queryFn: () =>
      fetchApi<StationPriceDto[]>(
        `/prices/cheapest?fuelType=${fuelType}&limit=${limit}`
      ),
    enabled: fuelType !== "",
  });
}
