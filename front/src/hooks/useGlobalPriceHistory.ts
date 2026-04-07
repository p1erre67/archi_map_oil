import { useQuery } from "@tanstack/react-query";
import { fetchApi } from "../api/client";
import type { GlobalPricePointDto } from "../types/api";

export function useGlobalPriceHistory(
  fuelType: string,
  from: string,
  to: string
) {
  return useQuery({
    queryKey: ["global-price-history", fuelType, from, to],
    queryFn: () =>
      fetchApi<GlobalPricePointDto[]>(
        `/history/global?fuelType=${fuelType}&from=${from}&to=${to}`
      ),
    enabled: fuelType !== "" && from !== "" && to !== "",
  });
}
