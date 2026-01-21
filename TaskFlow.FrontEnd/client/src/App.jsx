import { Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import MainLayout from './layouts/MainLayout';
import Projects from './pages/Projects';
import ProjectDetail from './pages/ProjectDetail';
function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/" element={<MainLayout />}>
        <Route index element={<Dashboard />} /> 
        <Route path="dashboard" element={<Dashboard />} />
        <Route path="projects" element={<Projects />} /> 
        <Route path="projects/:id" element={<ProjectDetail />} />
      </Route>
    </Routes>
  );
}

export default App;