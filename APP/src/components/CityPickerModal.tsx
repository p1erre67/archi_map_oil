import { useState } from "react";
import {
  Modal,
  View,
  Text,
  TextInput,
  TouchableOpacity,
  FlatList,
  StyleSheet,
} from "react-native";
import { useCitySearch, type City } from "../hooks/useCitySearch";

interface Props {
  visible: boolean;
  onClose: () => void;
  onSelect: (city: City) => void;
}

export function CityPickerModal({ visible, onClose, onSelect }: Props) {
  const [query, setQuery] = useState("");
  const { cities, isLoading } = useCitySearch(query);

  function handleSelect(city: City) {
    onSelect(city);
    setQuery("");
    onClose();
  }

  return (
    <Modal
      visible={visible}
      animationType="slide"
      transparent
      onRequestClose={onClose}
    >
      <View style={styles.overlay}>
        <View style={styles.container}>
          <View style={styles.header}>
            <Text style={styles.title}>Choisir une ville</Text>
            <TouchableOpacity onPress={onClose}>
              <Text style={styles.closeBtn}>Fermer</Text>
            </TouchableOpacity>
          </View>

          <TextInput
            style={styles.input}
            placeholder="Rechercher une ville..."
            value={query}
            onChangeText={setQuery}
            autoFocus
          />

          {isLoading && <Text style={styles.info}>Recherche...</Text>}

          {!isLoading && query.length >= 2 && cities.length === 0 && (
            <Text style={styles.info}>Aucune ville trouvee</Text>
          )}

          <FlatList
            data={cities}
            keyExtractor={(item) => item.code}
            renderItem={({ item }) => (
              <TouchableOpacity
                style={styles.cityItem}
                onPress={() => handleSelect(item)}
              >
                <Text style={styles.cityName}>{item.nom}</Text>
                <Text style={styles.cityCode}>({item.code.slice(0, 2)})</Text>
              </TouchableOpacity>
            )}
          />
        </View>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.4)",
    justifyContent: "flex-end",
  },
  container: {
    backgroundColor: "#fff",
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
    padding: 16,
    maxHeight: "80%",
    minHeight: "50%",
  },
  header: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 12,
  },
  title: {
    fontSize: 18,
    fontWeight: "600",
  },
  closeBtn: {
    color: "#2563eb",
    fontSize: 15,
    fontWeight: "500",
  },
  input: {
    borderWidth: 1,
    borderColor: "#d1d5db",
    borderRadius: 8,
    padding: 12,
    fontSize: 15,
    marginBottom: 12,
  },
  info: {
    color: "#6b7280",
    fontSize: 14,
    padding: 8,
    textAlign: "center",
  },
  cityItem: {
    flexDirection: "row",
    padding: 14,
    borderBottomWidth: 1,
    borderBottomColor: "#f3f4f6",
    alignItems: "center",
    gap: 6,
  },
  cityName: {
    fontSize: 15,
    flex: 1,
  },
  cityCode: {
    fontSize: 13,
    color: "#94a3b8",
  },
});
