import { BrowserRouter, Routes, Route, NavLink } from "react-router-dom";
import { DashboardPage } from "./pages/DashboardPage";
import { StationsPage } from "./pages/StationsPage";
import "./App.css";

export function App() {
  return (
    <BrowserRouter>
      <nav className="navbar">
        <span className="navbar-brand">PriceWatch</span>
        <NavLink to="/">Dashboard</NavLink>
        <NavLink to="/stations">Stations</NavLink>
      </nav>

      <main>
        <Routes>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/stations" element={<StationsPage />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}
