import { Outlet, Navigate } from "react-router-dom";
import Sidebar from "../components/Sidebar";

export default function MainLayout() {
  const token = localStorage.getItem("token");

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  return (
    <div className="flex min-h-screen bg-[#111827]">
      <Sidebar />      
      <main className="flex-1 p-8 overflow-y-auto">
        <Outlet />
      </main>
    </div>
  );
}