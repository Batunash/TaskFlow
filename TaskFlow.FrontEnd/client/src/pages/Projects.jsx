import { useEffect, useState } from "react";
import { Plus, Search, X, FolderKanban } from "lucide-react";
import { Link } from "react-router-dom"; 
import projectService from "../services/projectService";
import workflowService from "../services/workflowService"; 

export default function Projects() {
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [formData, setFormData] = useState({
    name: "",
    description: "",
  });

  useEffect(() => {
    fetchProjects();
  }, []);

  const fetchProjects = async () => {
    try {
      const data = await projectService.getAll();
      setProjects(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("Projeler yüklenemedi:", error);
      setProjects([]); 
    } finally {
      setLoading(false);
    }
  };
  const handleCreateProject = async (e) => {
    e.preventDefault();
    
    if (!formData.name.trim()) {
        alert("Proje adı boş olamaz!");
        return;
    }

    try {
      const newProject = await projectService.create(formData);
      console.log("Proje oluşturuldu:", newProject);

      if (newProject && newProject.id) {
        try {
          await workflowService.create(newProject.id);
          console.log("Workflow başarıyla oluşturuldu.");
          await workflowService.addState(newProject.id, { name: "To Do", isInitial: true, isFinal: false });
          await workflowService.addState(newProject.id, { name: "In Progress", isInitial: false, isFinal: false });
          await workflowService.addState(newProject.id, { name: "Done", isInitial: false, isFinal: true });

        } catch (wfError) {
          console.error("Workflow oluşturulurken hata:", wfError);
          alert("Proje oluşturuldu ancak iş akışı hazırlanamadı. Detay sayfasından 'Yapılandır' diyerek düzeltebilirsiniz.");
        }
      }
      await fetchProjects();
      setIsModalOpen(false);
      setFormData({ name: "", description: "" });
      
    } catch (error) {
      console.error(error);
      const serverMessage = error.response?.data?.errors 
                            ? Object.values(error.response.data.errors).flat().join(", ")
                            : "Proje oluşturulamadı.";
      alert(serverMessage);
    }
  };
  const filteredProjects = projects.filter(p => 
    p.name?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="text-white">
      <div className="flex flex-col md:flex-row justify-between items-center mb-8 gap-4">
        <div>
          <h2 className="text-3xl font-bold text-gray-100">Projects</h2>
          <p className="text-gray-400 mt-1">Manage and track your ongoing projects</p>
        </div>
        
        <button 
          onClick={() => setIsModalOpen(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 transition-colors shadow-lg shadow-blue-900/20"
        >
          <Plus size={20} /> New Project
        </button>
      </div>
      <div className="mb-8 relative max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={18} />
        <input 
          type="text" 
          placeholder="Search projects..." 
          className="w-full bg-[#1F2937] border border-gray-700 rounded-lg py-2 pl-10 pr-4 text-gray-300 focus:outline-none focus:border-blue-500 transition-colors"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>
      {loading ? (
        <div className="text-center py-20 text-gray-500">Loading projects...</div>
      ) : filteredProjects.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredProjects.map((project) => (
            <Link key={project.id} to={`/projects/${project.id}`} className="block group">
              <div className="bg-[#1F2937] border border-gray-700 rounded-xl p-5 hover:border-blue-500/50 transition-all relative h-full">
                <div className="flex justify-between items-start mb-4">
                  <div className="p-2 bg-blue-900/20 rounded-lg text-blue-400 group-hover:bg-blue-600 group-hover:text-white transition-colors">
                    <FolderKanban size={20} />
                  </div>
                </div>
                <h3 className="text-lg font-semibold text-white mb-2 truncate pr-6">{project.name}</h3>
                <p className="text-gray-400 text-sm line-clamp-2 mb-4 break-words">
                  {project.description || "No description provided."}
                </p>
              </div>
            </Link>
          ))}
        </div>
      ) : (
        <div className="text-center py-20 bg-[#1F2937]/50 rounded-xl border border-dashed border-gray-700">
          <FolderKanban className="w-12 h-12 text-gray-600 mx-auto mb-4" />
          <p className="text-gray-400 mb-4">No projects found.</p>
          <button 
            onClick={() => setIsModalOpen(true)}
            className="text-blue-400 hover:text-blue-300 font-medium"
          >
            Create your first project
          </button>
        </div>
      )}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative animate-in fade-in zoom-in duration-200">
            <button 
              onClick={() => setIsModalOpen(false)}
              className="absolute right-4 top-4 text-gray-400 hover:text-white"
            >
              <X size={20} />
            </button>

            <h3 className="text-xl font-bold mb-6">Create New Project</h3>
            
            <form onSubmit={handleCreateProject} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-400 mb-1">Project Name</label>
                <input 
                  type="text" 
                  required
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white focus:border-blue-500 outline-none"
                  placeholder="e.g. Website Redesign"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-400 mb-1">Description</label>
                <textarea 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white focus:border-blue-500 outline-none h-24 resize-none"
                  placeholder="Project details..."
                  value={formData.description}
                  onChange={(e) => setFormData({...formData, description: e.target.value})}
                />
              </div>

              <div className="pt-4 flex justify-end gap-3">
                <button 
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="px-4 py-2 text-gray-400 hover:text-white transition-colors"
                >
                  Cancel
                </button>
                <button 
                  type="submit"
                  className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-lg font-medium transition-colors"
                >
                  Create Project
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}