import { useMutation, useQueryClient } from "@tanstack/react-query";
import { postApi } from "../api/client";

interface SyncRequest {
  latitude: number;
  longitude: number;
  radiusKm: number;
}

export function useSyncPrices() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: SyncRequest) =>
      postApi<{ message: string }>("/prices/sync", request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["nearby-stations"] });
    },
  });
}
