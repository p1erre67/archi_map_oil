import { useState, useEffect } from "react";

export interface City {
  nom: string;
  code: string;
  latitude: number;
  longitude: number;
}

interface ApiCommune {
  nom: string;
  code: string;
  centre: {
    type: string;
    coordinates: [number, number]; // [lon, lat]
  };
}

export function useCitySearch(query: string) {
  const [cities, setCities] = useState<City[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (query.length < 2) {
      setCities([]);
      return;
    }

    const controller = new AbortController();
    const timeout = setTimeout(async () => {
      setIsLoading(true);
      try {
        const res = await fetch(
          `https://geo.api.gouv.fr/communes?nom=${encodeURIComponent(query)}&fields=centre&limit=5`,
          { signal: controller.signal }
        );
        const data: ApiCommune[] = await res.json();
        setCities(
          data
            .filter((c) => c.centre)
            .map((c) => ({
              nom: c.nom,
              code: c.code,
              latitude: c.centre.coordinates[1],
              longitude: c.centre.coordinates[0],
            }))
        );
      } catch (err) {
        if (!(err instanceof DOMException && err.name === "AbortError")) {
          setCities([]);
        }
      } finally {
        setIsLoading(false);
      }
    }, 300);

    return () => {
      clearTimeout(timeout);
      controller.abort();
    };
  }, [query]);

  return { cities, isLoading };
}
