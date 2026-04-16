import { Tabs } from "expo-router";
import { Ionicons } from "@expo/vector-icons";
import { FiltersProvider } from "../../src/hooks/useFilters";

export default function TabsLayout() {
  return (
    <FiltersProvider>
      <Tabs
        screenOptions={{
          tabBarActiveTintColor: "#2563eb",
          tabBarInactiveTintColor: "#94a3b8",
          tabBarStyle: {
            backgroundColor: "#ffffff",
            borderTopWidth: 0.5,
            borderTopColor: "#e2e8f0",
            elevation: 0,
            height: 56,
            paddingBottom: 6,
          },
          headerStyle: {
            backgroundColor: "#0f172a",
            elevation: 0,
            shadowOpacity: 0,
          },
          headerTintColor: "#ffffff",
          headerTitleStyle: {
            fontWeight: "600",
            fontSize: 16,
            letterSpacing: 0.3,
          },
          headerTitleAlign: "center",
        }}
      >
        <Tabs.Screen
          name="index"
          options={{
            title: "Carte",
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="map" size={size} color={color} />
            ),
          }}
        />
        <Tabs.Screen
          name="stations"
          options={{
            title: "Stations",
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="location" size={size} color={color} />
            ),
          }}
        />
        <Tabs.Screen
          name="history"
          options={{
            title: "Evolution",
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="trending-up" size={size} color={color} />
            ),
          }}
        />
      </Tabs>
    </FiltersProvider>
  );
}
