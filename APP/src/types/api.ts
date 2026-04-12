// Miroir des DTOs C# du backend

export interface FuelPriceDto {
  fuelType: string;
  pricePerLiter: number;
  updatedAt: string;
}

export interface StationPriceDto {
  id: string;
  externalStationId: string;
  stationName: string;
  brandName: string;
  address: string;
  city: string;
  postalCode: string;
  latitude: number;
  longitude: number;
  lastUpdated: string;
  fuelPrices: FuelPriceDto[];
}

export interface GlobalPricePointDto {
  date: string;
  averagePrice: number;
  minPrice: number;
  maxPrice: number;
  stationCount: number;
}

export interface PriceRecordDto {
  id: string;
  externalStationId: string;
  stationName: string;
  city: string;
  fuelType: string;
  pricePerLiter: number;
  recordedAt: string;
}
