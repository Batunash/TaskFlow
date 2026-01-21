import { useEffect, useState } from "react";
import { Plus, Search, Layers } from "lucide-react"; 
import projectService from "../services/projectService";
import workflowService from "../services/workflowService"; 
import ProjectCard from "../components/ProjectCard";
import CreateProjectModal from "../components/CreateProjectModal";

export default function Projects() {
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [createLoading, setCreateLoading] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    fetchProjects();
  }, []); 

  const fetchProjects = async () => {
    try {
      const data = await projectService.getAll();
      setProjects(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("Failed to load projects:", error);
      setProjects([]); 
    } finally {
      setLoading(false);
    }
  };

  const handleCreateProject = async (e) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    const newProjectData = {
        name: formData.get("name"),
        description: formData.get("description")
    };

    if (!newProjectData.name.trim()) return;
    
    setCreateLoading(true);
    try {
      const newProject = await projectService.create(newProjectData);
      if (newProject?.id) {
          try {
             await workflowService.create(newProject.id);
          } catch (wfError) {
             console.warn("Workflow auto-create failed, user can add manually.", wfError);
          }
      }
      
      await fetchProjects(); 
      setIsModalOpen(false);
    } catch (error) {
      console.error(error);
      alert("Failed to create project.");
    } finally {
      setCreateLoading(false);
    }
  };

  const handleDeleteProject = async (projectId) => {
      if(!window.confirm("Are you sure you want to delete this project? This action cannot be undone.")) return;

      try {
          setProjects(prev => prev.filter(p => p.id !== projectId));
          await projectService.delete(projectId);
      } catch (error) {
          console.error("Delete failed:", error);
          alert("Could not delete project.");
          fetchProjects(); 
      }
  };

  const filteredProjects = projects.filter(p => 
    p.name.toLowerCase().includes(searchTerm.toLowerCase()) || 
    (p.description && p.description.toLowerCase().includes(searchTerm.toLowerCase()))
  );

  if (loading) return <div className="text-center mt-20 text-gray-400">Loading projects...</div>;

  return (
    <div className="h-full flex flex-col text-white">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8 border-b border-gray-800 pb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-100 flex items-center gap-3">
            <Layers className="text-blue-500" /> Projects
          </h1>
          <p className="text-gray-400 mt-1">Manage your team's work and workflows.</p>
        </div>

        <div className="flex gap-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={18} />
            <input 
              type="text" 
              placeholder="Search projects..." 
              className="bg-[#1F2937] border border-gray-700 text-white pl-10 pr-4 py-2 rounded-lg outline-none focus:border-blue-500 w-full md:w-64 transition-all"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <button 
            onClick={() => setIsModalOpen(true)}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center gap-2 font-medium transition-colors shadow-lg shadow-blue-900/20"
          >
            <Plus size={20} /> New Project
          </button>
        </div>
      </div>
      {filteredProjects.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 pb-8">
          {filteredProjects.map((project) => (
            <ProjectCard 
              key={project.id} 
              project={project} 
              onDelete={handleDeleteProject}
            />
          ))}
        </div>
      ) : (
        <div className="flex-1 flex flex-col items-center justify-center text-center p-12 border-2 border-dashed border-gray-800 rounded-xl bg-[#1F2937]/30">
          <div className="bg-gray-800 p-4 rounded-full mb-4">
            <Layers size={48} className="text-gray-600" />
          </div>
          <h3 className="text-xl font-medium text-gray-300">No projects found</h3>
          <p className="text-gray-500 mt-2 max-w-sm">
            {searchTerm ? "Try adjusting your search terms." : "Get started by creating your first project to organize tasks."}
          </p>
          {!searchTerm && (
             <button 
                onClick={() => setIsModalOpen(true)}
                className="mt-6 text-blue-400 hover:text-blue-300 font-medium"
             >
                Create your first project &rarr;
             </button>
          )}
        </div>
      )}
      <CreateProjectModal 
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleCreateProject}
        loading={createLoading}
      />
    </div>
  );
}