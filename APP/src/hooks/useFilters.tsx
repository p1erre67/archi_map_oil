import { createContext, useContext, useState, type ReactNode } from "react";
import type { City } from "./useCitySearch";
import { useLocation } from "./useLocation";

interface ManualCoords {
  lat: number;
  lng: number;
}

interface FiltersState {
  selectedCity: City | null;
  setSelectedCity: (city: City | null) => void;
  manualCoords: ManualCoords | null;
  setManualCoords: (coords: ManualCoords | null) => void;
  radiusKm: number;
  setRadiusKm: (radius: number) => void;
  fuelType: string;
  setFuelType: (fuel: string) => void;
  activeLat: number;
  activeLng: number;
  locationReady: boolean;
  locationLabel: string;
}

const FiltersContext = createContext<FiltersState | null>(null);

export function FiltersProvider({ children }: { children: ReactNode }) {
  const deviceLocation = useLocation();
  const [selectedCity, setSelectedCityRaw] = useState<City | null>(null);
  const [manualCoords, setManualCoords] = useState<ManualCoords | null>(null);
  const [radiusKm, setRadiusKm] = useState(8);
  const [fuelType, setFuelType] = useState("Tous");

  // Quand on choisit une ville, on reset les coords manuelles
  function setSelectedCity(city: City | null) {
    setSelectedCityRaw(city);
    if (city) setManualCoords(null);
  }

  // Priorite : ville > coords carte > geoloc device
  const activeLat = selectedCity?.latitude ?? manualCoords?.lat ?? deviceLocation.latitude;
  const activeLng = selectedCity?.longitude ?? manualCoords?.lng ?? deviceLocation.longitude;
  const locationReady = selectedCity !== null || manualCoords !== null || deviceLocation.ready;

  const locationLabel = selectedCity
    ? selectedCity.nom
    : manualCoords
      ? "Position carte"
      : "Ma position";

  return (
    <FiltersContext.Provider
      value={{
        selectedCity,
        setSelectedCity,
        manualCoords,
        setManualCoords,
        radiusKm,
        setRadiusKm,
        fuelType,
        setFuelType,
        activeLat,
        activeLng,
        locationReady,
        locationLabel,
      }}
    >
      {children}
    </FiltersContext.Provider>
  );
}

export function useFilters() {
  const ctx = useContext(FiltersContext);
  if (!ctx) throw new Error("useFilters must be used within FiltersProvider");
  return ctx;
}
