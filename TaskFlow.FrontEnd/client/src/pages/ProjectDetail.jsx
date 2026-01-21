import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Plus, ArrowLeft, MoreHorizontal, Settings } from "lucide-react";
import projectService from "../services/projectService";
import taskService from "../services/taskService";
import workflowService from "../services/workflowService";

export default function ProjectDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [project, setProject] = useState(null);
  const [states, setStates] = useState([]);
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);
  const [newTask, setNewTask] = useState({ title: "", description: "", priority: 1 });
  const [isStateModalOpen, setIsStateModalOpen] = useState(false);
  const [newStateForm, setNewStateForm] = useState({
    name: "",
    previousStateId: "", 
    allowedRoles: ["Admin", "Member"] 
  });

  useEffect(() => {
    fetchProjectData();
  }, [id]);

  const fetchProjectData = async () => {
    try {
      const [projectData, statesData, tasksData] = await Promise.all([
        projectService.getById(id),
        workflowService.getProjectStates(id),
        taskService.getAll({ projectId: id })
      ]);

      setProject(projectData);
      setStates(statesData || []);
      setTasks(tasksData || []);
    } catch (error) {
      console.error("Veri yüklenemedi:", error);
    } finally {
      setLoading(false);
    }
  };
  const handleCreateTask = async (e) => {
    e.preventDefault();
    try {
      await taskService.create({
        ...newTask,
        projectId: id,
        priority: parseInt(newTask.priority)
      });
      setIsTaskModalOpen(false);
      setNewTask({ title: "", description: "", priority: 1 });
      fetchProjectData();
    } catch (error) {
      console.error(error);
      alert("Görev oluşturulamadı.");
    }
  };
  const handleAddState = async (e) => {
    e.preventDefault();
    
    if(!newStateForm.name) return alert("Lütfen bir isim girin.");

    try {
      const createdState = await workflowService.addState(id, {
        name: newStateForm.name,
        isInitial: false,
        isFinal: false 
      });
      console.log("State oluşturuldu:", createdState);
      if (newStateForm.previousStateId) {
        await workflowService.addTransition(id, {
          fromStateId: parseInt(newStateForm.previousStateId), 
          toStateId: createdState.id, 
          allowedRoles: newStateForm.allowedRoles
        });
        console.log("Transition oluşturuldu.");
      }
      await fetchProjectData();
      setIsStateModalOpen(false);
      setNewStateForm({ name: "", previousStateId: "", allowedRoles: ["Admin", "Member"] });

    } catch (error) {
      console.error(error);
      alert("Sütun eklenirken hata oluştu.");
    }
  };

  const handleRoleChange = (role) => {
    setNewStateForm(prev => {
      const roles = prev.allowedRoles.includes(role)
        ? prev.allowedRoles.filter(r => r !== role)
        : [...prev.allowedRoles, role];
      return { ...prev, allowedRoles: roles };
    });
  };

  if (loading) return <div className="text-white text-center mt-20">Yükleniyor...</div>;

  return (
    <div className="h-full flex flex-col text-white">
      <div className="flex items-center justify-between mb-6 border-b border-gray-700 pb-4">
        <div className="flex items-center gap-4">
          <button onClick={() => navigate("/projects")} className="p-2 hover:bg-gray-800 rounded-lg">
            <ArrowLeft size={20} className="text-gray-400" />
          </button>
          <div>
            <h1 className="text-2xl font-bold">{project?.name}</h1>
            <p className="text-gray-400 text-sm">{project?.description}</p>
          </div>
        </div>
        
        <div className="flex gap-3">
          <button 
            onClick={() => setIsStateModalOpen(true)}
            className="bg-gray-700 hover:bg-gray-600 text-white px-4 py-2 rounded-lg flex items-center gap-2 text-sm font-medium border border-gray-600 transition-colors"
          >
            <Settings size={18} /> Add Column
          </button>
          <button 
            onClick={() => setIsTaskModalOpen(true)}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center gap-2 text-sm font-medium transition-colors"
          >
            <Plus size={18} /> New Task
          </button>
        </div>
      </div>
      <div className="flex-1 overflow-x-auto">
        <div className="flex gap-6 h-full min-w-max pb-4 px-2">
          {states.map((state) => (
            <div key={state.id} className="w-80 flex-shrink-0 flex flex-col">
              <div className="flex items-center justify-between mb-3 px-1">
                <h3 className="font-semibold text-gray-300 flex items-center gap-2">
                  <span className={`w-3 h-3 rounded-full ${state.isFinal ? 'bg-green-500' : state.isInitial ? 'bg-blue-500' : 'bg-yellow-500'}`}></span>
                  {state.name}
                </h3>
                <span className="text-xs bg-gray-800 text-gray-500 px-2 py-1 rounded-full border border-gray-700">
                  {tasks.filter(t => t.stateId === state.id).length}
                </span>
              </div>
              <div className="bg-[#1F2937]/50 rounded-xl p-3 flex-1 border border-gray-800/50 min-h-[200px]">
                <div className="space-y-3">
                  {tasks.filter(t => t.stateId === state.id).map(task => (
                    <div key={task.id} className="bg-[#1F2937] p-4 rounded-lg border border-gray-700 hover:border-blue-500/50 cursor-pointer shadow-sm group">
                      <div className="flex justify-between items-start mb-2">
                        <span className={`text-xs px-2 py-0.5 rounded ${task.priority === 2 ? 'bg-red-900/30 text-red-400' : 'bg-blue-900/30 text-blue-400'}`}>
                          {task.priority === 2 ? 'High' : task.priority === 1 ? 'Medium' : 'Low'}
                        </span>
                      </div>
                      <h4 className="font-medium text-gray-200 mb-1">{task.title}</h4>
                      <div className="mt-3 pt-3 border-t border-gray-700/50 flex items-center justify-between text-xs text-gray-500">
                         <span>#{task.id}</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          ))}
          {states.length === 0 && (
            <div className="text-gray-500 m-auto text-center">
              <p>Henüz bir aşama (sütun) yok.</p>
              <p className="text-sm opacity-70">Sağ üstteki "Add Column" ile başlayın.</p>
            </div>
          )}
        </div>
      </div>
      {isStateModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
          <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
            <button onClick={() => setIsStateModalOpen(false)} className="absolute right-4 top-4 text-gray-400 hover:text-white">X</button>
            <h3 className="text-xl font-bold mb-6">Yeni Sütun Ekle</h3>
            
            <form onSubmit={handleAddState} className="space-y-4">
              <div>
                <label className="block text-sm text-gray-400 mb-1">Sütun Adı</label>
                <input 
                  type="text" required
                  placeholder="Örn: Review, QA, Test"
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newStateForm.name}
                  onChange={(e) => setNewStateForm({...newStateForm, name: e.target.value})}
                />
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Hangi Aşamadan Sonra?</label>
                <select 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newStateForm.previousStateId}
                  onChange={(e) => setNewStateForm({...newStateForm, previousStateId: e.target.value})}
                >
                  <option value="">-- Başlangıç (Bağımsız) --</option>
                  {states.map(s => (
                    <option key={s.id} value={s.id}>{s.name}</option>
                  ))}
                </select>
                <p className="text-xs text-gray-500 mt-1">Seçtiğiniz sütundan yeni oluşturacağınız sütuna bir geçiş kuralı ekler.</p>
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-2">Kimler taşıyabilir?</label>
                <div className="flex gap-4">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input 
                      type="checkbox" 
                      checked={newStateForm.allowedRoles.includes("Admin")}
                      onChange={() => handleRoleChange("Admin")}
                      className="rounded border-gray-600 bg-[#111827] text-blue-600"
                    />
                    <span className="text-sm">Admin</span>
                  </label>
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input 
                      type="checkbox" 
                      checked={newStateForm.allowedRoles.includes("Member")}
                      onChange={() => handleRoleChange("Member")}
                      className="rounded border-gray-600 bg-[#111827] text-blue-600"
                    />
                    <span className="text-sm">Member</span>
                  </label>
                </div>
              </div>

              <button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-medium mt-4">
                Oluştur
              </button>
            </form>
          </div>
        </div>
      )}
      {isTaskModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
          <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
            <button onClick={() => setIsTaskModalOpen(false)} className="absolute right-4 top-4 text-gray-400 hover:text-white">X</button>
            <h3 className="text-xl font-bold mb-6">Yeni Görev Ekle</h3>
            <form onSubmit={handleCreateTask} className="space-y-4">
              <div>
                <label className="block text-sm text-gray-400 mb-1">Başlık</label>
                <input 
                  type="text" required
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newTask.title}
                  onChange={(e) => setNewTask({...newTask, title: e.target.value})}
                />
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Açıklama</label>
                <textarea 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 h-24 resize-none"
                  value={newTask.description}
                  onChange={(e) => setNewTask({...newTask, description: e.target.value})}
                />
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Öncelik</label>
                <select 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newTask.priority}
                  onChange={(e) => setNewTask({...newTask, priority: e.target.value})}
                >
                  <option value="0">Düşük</option>
                  <option value="1">Orta</option>
                  <option value="2">Yüksek</option>
                </select>
              </div>
              <button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-medium">Oluştur</button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}