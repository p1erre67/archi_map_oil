import { useQuery } from "@tanstack/react-query";
import { fetchApi } from "../api/client";
import type { GlobalPricePointDto } from "../types/api";

export function useGlobalPriceHistory(
  fuelType: string,
  from: string,
  to: string,
  stationIds?: string[]
) {
  const idsParam =
    stationIds && stationIds.length > 0
      ? `&stationIds=${stationIds.join(",")}`
      : "";

  return useQuery({
    queryKey: ["global-price-history", fuelType, from, to, stationIds],
    queryFn: () =>
      fetchApi<GlobalPricePointDto[]>(
        `/history/global?fuelType=${fuelType}&from=${from}&to=${to}${idsParam}`
      ),
    enabled: fuelType !== "" && from !== "" && to !== "",
  });
}
