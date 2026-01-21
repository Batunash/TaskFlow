import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Plus, ArrowLeft, Settings, Trash2, X } from "lucide-react"; 
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
  
  // Modals
  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);
  const [newTask, setNewTask] = useState({ title: "", description: "", priority: 1 });
  const [isStateModalOpen, setIsStateModalOpen] = useState(false);
  
  const [newStateForm, setNewStateForm] = useState({
    name: "",
    previousStateId: "", 
    isInitial: false,
    isFinal: false,
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
        taskService.getByProjectId(id)
      ]);

      setProject(projectData);
      setStates(statesData || []);

      // Mapping: Backend verisini Frontend'e uydur
      const mappedTasks = (tasksData || []).map(task => ({
        ...task,
        // Backend 'stateId' veya 'StateId' gönderebilir
        workflowStateId: task.stateId || task.StateId || task.workflowStateId,
        
        // Eksik alanlar için varsayılanlar
        description: task.description || task.Description || "", 
        priority: task.priority !== undefined ? task.priority : 1 
      }));

      setTasks(mappedTasks);
    } catch (error) {
      console.error("Failed to load data:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTask = async (e) => {
    e.preventDefault();
    try {
      const initialState = states.find(s => s.isInitial);
      
      if (!initialState) {
        alert("No 'Initial State' defined in this project. Please add a column and mark it as 'Initial State'.");
        return;
      }

      await taskService.create({
        ...newTask,
        projectId: parseInt(id),
        workflowStateId: initialState.id, 
        priority: parseInt(newTask.priority)
      });
      
      setIsTaskModalOpen(false);
      setNewTask({ title: "", description: "", priority: 1 });
      fetchProjectData();
    } catch (error) {
      console.error(error);
      alert("Failed to create task.");
    }
  };

  const handleDeleteTask = async (taskId) => {
    if(!window.confirm("Are you sure you want to delete this task?")) return;
    try {
        await taskService.delete(taskId);
        fetchProjectData();
    } catch (error) {
        console.error("Delete error:", error);
        alert("Failed to delete task.");
    }
  };

  // Sütun Silme Fonksiyonu
  const handleDeleteState = async (stateId) => {
    if(!window.confirm("Are you sure you want to delete this column? Tasks in it might be lost.")) return;
    try {
        await workflowService.removeState(id, stateId); 
        fetchProjectData();
    } catch (error) {
        console.error("State delete error:", error);
        alert("Failed to delete column. Ensure it's not being used properly.");
    }
  };

  const handleStatusChange = async (taskId, newStateId) => {
      try {
          // Servise TargetStateId olarak gidecek
          await taskService.changeStatus(taskId, newStateId);
          fetchProjectData();
      } catch (error) {
          console.error("Status change error:", error);
          alert("Failed to change status.");
      }
  };

  const handleAddState = async (e) => {
    e.preventDefault();
    if(!newStateForm.name) return alert("Please enter a name.");

    try {
      const createdState = await workflowService.addState(id, {
        name: newStateForm.name,
        isInitial: newStateForm.isInitial, 
        isFinal: newStateForm.isFinal     
      });

      if (newStateForm.previousStateId) {
        await workflowService.addTransition(id, {
          fromStateId: parseInt(newStateForm.previousStateId), 
          toStateId: createdState.id, 
          allowedRoles: newStateForm.allowedRoles
        });
      }
      
      await fetchProjectData();
      setIsStateModalOpen(false);
      setNewStateForm({ 
        name: "", 
        previousStateId: "", 
        isInitial: false, 
        isFinal: false,
        allowedRoles: ["Admin", "Member"] 
      });

    } catch (error) {
      console.error(error);
      alert("Failed to add column.");
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

  if (loading) return <div className="text-white text-center mt-20">Loading...</div>;

  return (
    <div className="h-full flex flex-col text-white">
      {/* Header */}
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

      {/* Board Area */}
      <div className="flex-1 overflow-x-auto">
        <div className="flex gap-6 h-full min-w-max pb-4 px-2">
          {states.map((state) => (
            <div key={state.id} className="w-80 flex-shrink-0 flex flex-col group/column">
              <div className="flex items-center justify-between mb-3 px-1">
                <h3 className="font-semibold text-gray-300 flex items-center gap-2">
                  <span className={`w-3 h-3 rounded-full ${state.isFinal ? 'bg-green-500' : state.isInitial ? 'bg-blue-500' : 'bg-yellow-500'}`}></span>
                  {state.name}
                </h3>
                <div className="flex items-center gap-2">
                    <span className="text-xs bg-gray-800 text-gray-500 px-2 py-1 rounded-full border border-gray-700">
                      {tasks.filter(t => t.workflowStateId === state.id).length}
                    </span>
                    {/* SÜTUN SİLME BUTONU */}
                    <button 
                        onClick={() => handleDeleteState(state.id)}
                        className="text-gray-600 hover:text-red-500 opacity-0 group-hover/column:opacity-100 transition-opacity"
                        title="Delete Column"
                    >
                        <Trash2 size={16} />
                    </button>
                </div>
              </div>
              
              {/* Task List */}
              <div className="bg-[#1F2937]/50 rounded-xl p-3 flex-1 border border-gray-800/50 min-h-[200px] space-y-3">
                {tasks.filter(t => t.workflowStateId === state.id).map(task => (
                    <div key={task.id} className="bg-[#1F2937] p-4 rounded-lg border border-gray-700 hover:border-blue-500/50 shadow-sm group relative">
                      <div className="flex justify-between items-start mb-2">
                        <span className={`text-xs px-2 py-0.5 rounded ${task.priority === 2 ? 'bg-red-900/30 text-red-400' : task.priority === 1 ? 'bg-blue-900/30 text-blue-400' : 'bg-gray-700 text-gray-300'}`}>
                          {task.priority === 2 ? 'High' : task.priority === 1 ? 'Medium' : 'Low'}
                        </span>
                        <button 
                            onClick={() => handleDeleteTask(task.id)}
                            className="text-gray-600 hover:text-red-400 transition-colors"
                        >
                            <Trash2 size={14} />
                        </button>
                      </div>
                      <h4 className="font-medium text-gray-200 mb-1">{task.title}</h4>
                      <p className="text-xs text-gray-500 mb-3 line-clamp-2">{task.description}</p>
                      
                      {/* Status Changer */}
                      <div className="pt-3 border-t border-gray-700/50 flex items-center justify-between gap-2">
                         <span className="text-xs text-gray-500">#{task.id}</span>
                         <select 
                            className="bg-[#111827] text-xs text-gray-300 border border-gray-600 rounded px-1 py-1 outline-none focus:border-blue-500 max-w-[120px]"
                            value={task.workflowStateId}
                            onChange={(e) => handleStatusChange(task.id, e.target.value)}
                         >
                             {states.map(s => (
                                 <option key={s.id} value={s.id}>{s.name}</option>
                             ))}
                         </select>
                      </div>
                    </div>
                  ))}
              </div>
            </div>
          ))}

          {states.length === 0 && (
            <div className="text-gray-500 m-auto text-center">
              <p>This project has no workflow yet.</p>
              <p className="text-sm opacity-70">Click "Add Column" to define your process.</p>
            </div>
          )}
        </div>
      </div>

      {/* Add Column Modal */}
      {isStateModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
          <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
            <button onClick={() => setIsStateModalOpen(false)} className="absolute right-4 top-4 text-gray-400 hover:text-white">
                <X size={20} />
            </button>
            <h3 className="text-xl font-bold mb-6">Add New Column</h3>
            
            <form onSubmit={handleAddState} className="space-y-4">
              <div>
                <label className="block text-sm text-gray-400 mb-1">Column Name</label>
                <input 
                  type="text" required
                  placeholder="e.g. Backlog, Testing"
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newStateForm.name}
                  onChange={(e) => setNewStateForm({...newStateForm, name: e.target.value})}
                />
              </div>

              <div className="flex gap-6">
                 <label className="flex items-center gap-2 cursor-pointer">
                    <input 
                      type="checkbox" 
                      className="rounded border-gray-600 bg-[#111827] text-blue-600 w-4 h-4"
                      checked={newStateForm.isInitial}
                      onChange={(e) => setNewStateForm({...newStateForm, isInitial: e.target.checked})}
                    />
                    <span className="text-sm text-gray-300">Is Initial State?</span>
                 </label>
                 <label className="flex items-center gap-2 cursor-pointer">
                    <input 
                      type="checkbox" 
                      className="rounded border-gray-600 bg-[#111827] text-blue-600 w-4 h-4"
                      checked={newStateForm.isFinal}
                      onChange={(e) => setNewStateForm({...newStateForm, isFinal: e.target.checked})}
                    />
                    <span className="text-sm text-gray-300">Is Final State?</span>
                 </label>
              </div>

              <div>
                <label className="block text-sm text-gray-400 mb-1">Transition from...</label>
                <select 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newStateForm.previousStateId}
                  onChange={(e) => setNewStateForm({...newStateForm, previousStateId: e.target.value})}
                >
                  <option value="">-- Independent / Start --</option>
                  {states.map(s => (
                    <option key={s.id} value={s.id}>{s.name}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm text-gray-400 mb-2">Who can move tasks here?</label>
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
                Create Column
              </button>
            </form>
          </div>
        </div>
      )}

      {/* Add Task Modal */}
      {isTaskModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
          <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
            <button onClick={() => setIsTaskModalOpen(false)} className="absolute right-4 top-4 text-gray-400 hover:text-white">
                <X size={20} />
            </button>
            <h3 className="text-xl font-bold mb-6">Add New Task</h3>
            <form onSubmit={handleCreateTask} className="space-y-4">
              <div>
                <label className="block text-sm text-gray-400 mb-1">Title</label>
                <input 
                  type="text" required
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newTask.title}
                  onChange={(e) => setNewTask({...newTask, title: e.target.value})}
                />
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Description</label>
                <textarea 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 h-24 resize-none"
                  value={newTask.description}
                  onChange={(e) => setNewTask({...newTask, description: e.target.value})}
                />
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Priority</label>
                <select 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newTask.priority}
                  onChange={(e) => setNewTask({...newTask, priority: e.target.value})}
                >
                  <option value="0">Low</option>
                  <option value="1">Medium</option>
                  <option value="2">High</option>
                </select>
              </div>
              <button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-medium">
                Create Task
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}