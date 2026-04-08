import { useState, useRef, useEffect } from "react";
import { useCitySearch, type City } from "../hooks/useCitySearch";

interface Props {
  onSelect: (city: City) => void;
  placeholder?: string;
}

export function CityAutocomplete({ onSelect, placeholder = "Rechercher une ville..." }: Props) {
  const [inputValue, setInputValue] = useState("");
  const [isOpen, setIsOpen] = useState(false);
  const [selectedCity, setSelectedCity] = useState<City | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const { cities, isLoading } = useCitySearch(selectedCity ? "" : inputValue);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  function handleInputChange(value: string) {
    setInputValue(value);
    setSelectedCity(null);
    setIsOpen(value.length >= 2);
  }

  function handleSelect(city: City) {
    setInputValue(city.nom);
    setSelectedCity(city);
    setIsOpen(false);
    onSelect(city);
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (e.key === "Escape") {
      setIsOpen(false);
    }
  }

  return (
    <div className="city-autocomplete" ref={containerRef}>
      <input
        type="text"
        value={inputValue}
        onChange={(e) => handleInputChange(e.target.value)}
        onFocus={() => inputValue.length >= 2 && !selectedCity && setIsOpen(true)}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
      />
      {isOpen && (
        <ul className="city-dropdown">
          {isLoading && <li className="city-dropdown-info">Recherche...</li>}
          {!isLoading && cities.length === 0 && inputValue.length >= 2 && (
            <li className="city-dropdown-info">Aucune ville trouvee</li>
          )}
          {cities.map((city) => (
            <li
              key={city.code}
              className="city-dropdown-item"
              onMouseDown={() => handleSelect(city)}
            >
              {city.nom} ({city.code.slice(0, 2)})
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
