import { NavLink, Navigate, Route, Routes } from "react-router-dom";
import { ProjectReportPage } from "./pages/project-report/ProjectReportPage";
import { TimeEntriesPage } from "./pages/time-entries/TimeEntriesPage";

export function App() {
  return (
    <main className="app">
      <nav className="nav">
        <NavLink to="/time-entries">Табель</NavLink>
        <NavLink to="/reports/projects">Отчёт по проектам</NavLink>
      </nav>

      <Routes>
        <Route path="/time-entries" element={<TimeEntriesPage />} />
        <Route path="/reports/projects" element={<ProjectReportPage />} />
        <Route path="*" element={<Navigate to="/time-entries" replace />} />
      </Routes>
    </main>
  );
}
