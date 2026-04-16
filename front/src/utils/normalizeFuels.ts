import type { FuelPriceDto } from "../types/api";

/**
 * Fusionne SP95 et SP95-E10 en une seule entree "SP95" :
 * garde le prix le plus recemment mis a jour.
 */
export function normalizeFuels(fuels: FuelPriceDto[]): FuelPriceDto[] {
  const result: FuelPriceDto[] = [];
  let sp95: FuelPriceDto | null = null;

  for (const f of fuels) {
    if (f.fuelType === "SP95" || f.fuelType === "SP95-E10") {
      if (!sp95 || new Date(f.updatedAt) > new Date(sp95.updatedAt)) {
        sp95 = { ...f, fuelType: "SP95" };
      }
    } else {
      result.push(f);
    }
  }

  if (sp95) result.push(sp95);
  return result;
}
