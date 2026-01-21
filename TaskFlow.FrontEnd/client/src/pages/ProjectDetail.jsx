import { useEffect, useState, useMemo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Plus, ArrowLeft, Settings, Trash2, X, Pencil, Users, Edit3, Search, Filter } from "lucide-react"; 
import projectService from "../services/projectService";
import taskService from "../services/taskService";
import workflowService from "../services/workflowService";
import organizationService from "../services/organizationService";
import userService from "../services/userService";

export default function ProjectDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [project, setProject] = useState(null);
  const [states, setStates] = useState([]);
  const [tasks, setTasks] = useState([]);
  const [orgMembers, setOrgMembers] = useState([]); 
  const [loading, setLoading] = useState(true);
  
  const [filters, setFilters] = useState({
    keyword: "",
    assignedUserId: "",
    priority: "all"
  });

  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);
  const [newTask, setNewTask] = useState({ title: "", description: "", priority: 1 });
  const [isStateModalOpen, setIsStateModalOpen] = useState(false);
  const [newStateForm, setNewStateForm] = useState({
    name: "", previousStateId: "", isInitial: false, isFinal: false, allowedRoles: ["Admin", "Member"] 
  });
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [isMemberModalOpen, setIsMemberModalOpen] = useState(false);
  const [selectedMemberId, setSelectedMemberId] = useState("");  
  const [isProjectSettingsOpen, setIsProjectSettingsOpen] = useState(false);
  const [projectForm, setProjectForm] = useState({ name: "", description: "" });

  useEffect(() => {
    fetchInitialData();
  }, [id]);

  useEffect(() => {
    if (!loading) {
      fetchTasks();
    }
  }, [filters.assignedUserId]);

  const fetchInitialData = async () => {
    try {
      const currentUser = await userService.getMe();
      
      const [projectData, statesData, orgMembersData] = await Promise.all([
        projectService.getById(id),
        workflowService.getProjectStates(id),
        organizationService.getMembers(currentUser.organizationId)
      ]);

      setProject(projectData);
      setStates(statesData || []);
      setOrgMembers(orgMembersData || []); 
      await fetchTasks();
      
    } catch (error) {
      console.error("Failed to load project data:", error);
    } finally {
      setLoading(false);
    }
  };

  const fetchTasks = async () => {
    try {
      const query = {
        projectId: parseInt(id),
        pageSize: 49, 
      };

      if (filters.assignedUserId) {
        query.assignedUserId = parseInt(filters.assignedUserId);
      }
      const result = await taskService.getAll(query);      
      const rawTasks = result.items || result.Items || (Array.isArray(result) ? result : []);
      const mappedTasks = rawTasks.map(task => ({
        ...task,
        workflowStateId: task.stateId || task.StateId || task.workflowStateId,
        description: task.description || task.Description || "", 
        priority: task.priority !== undefined ? task.priority : 1 
      }));

      setTasks(mappedTasks);
    } catch (error) {
      console.error("Failed to fetch tasks:", error);
    }
  };

  const filteredTasks = useMemo(() => {
    return tasks.filter(task => {
      const matchesKeyword = filters.keyword === "" || 
        task.title.toLowerCase().includes(filters.keyword.toLowerCase()) ||
        task.description.toLowerCase().includes(filters.keyword.toLowerCase());
      const matchesPriority = filters.priority === "all" || 
        task.priority === parseInt(filters.priority);

      return matchesKeyword && matchesPriority;
    });
  }, [tasks, filters.keyword, filters.priority]);

  const handleCreateTask = async (e) => {
    e.preventDefault();
    try {
      const initialState = states.find(s => s.isInitial);
      if (!initialState) {
        alert("No 'Initial State' defined. Please add a column and mark it as 'Initial State'.");
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
      fetchTasks();
    } catch (error) {
      console.error(error);
      alert("Failed to create task.");
    }
  };

  const handleDeleteTask = async (taskId) => {
    if(!window.confirm("Are you sure you want to delete this task?")) return;
    try {
        await taskService.delete(taskId);
        fetchTasks();
    } catch (error) {
        console.error("Delete error:", error);
        alert("Failed to delete task.");
    }
  };

  const openEditModal = (task) => {
    setEditingTask({
      id: task.id,
      title: task.title,
      description: task.description,
      priority: task.priority,
      assignedUserId: task.assignedUserId || ""
    });
    setIsEditModalOpen(true);
  };

  const handleUpdateTask = async (e) => {
    e.preventDefault();
    try {
      if (!editingTask) return;
      
      await taskService.update(editingTask.id, {
        title: editingTask.title,
        description: editingTask.description,
        priority: parseInt(editingTask.priority)
      });

      const originalTask = tasks.find(t => t.id === editingTask.id);
      if (editingTask.assignedUserId && editingTask.assignedUserId != originalTask.assignedUserId) {
          await taskService.assign(editingTask.id, parseInt(editingTask.assignedUserId));
      }

      await fetchTasks();
      setIsEditModalOpen(false);
      setEditingTask(null);
    } catch (error) {
      console.error("Update/Assign error:", error);
      alert("Failed to update task.");
    }
  };
  
  const handleStatusChange = async (taskId, newStateId) => {
      try {
          await taskService.changeStatus(taskId, newStateId);
          fetchTasks();
      } catch (error) {
          console.error("Status change error:", error);
          alert("Failed to change status.");
      }
  };

  const handleDeleteState = async (stateId) => {
    const stateToDelete = states.find(s => s.id === stateId);
    if (stateToDelete?.isInitial) {
        alert("The 'Initial State' cannot be deleted.");
        return;
    }

    const tasksInColumn = tasks.filter(t => t.workflowStateId === stateId);
    let confirmMessage = tasksInColumn.length > 0
        ? `WARNING: This column contains ${tasksInColumn.length} tasks! Deleting it will PERMANENTLY delete these tasks. Are you sure?`
        : "Are you sure you want to delete this column?";

    if(!window.confirm(confirmMessage)) return;

    try {
        if (tasksInColumn.length > 0) {
            await Promise.all(tasksInColumn.map(task => taskService.delete(task.id)));
        }
        await workflowService.removeState(id, stateId); 
        const updatedStates = await workflowService.getProjectStates(id);
        setStates(updatedStates || []);
        fetchTasks();
    } catch (error) {
        console.error("State delete error:", error);
        alert("Failed to delete column.");
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
      
      const updatedStates = await workflowService.getProjectStates(id);
      setStates(updatedStates || []);
      setIsStateModalOpen(false);
      setNewStateForm({ 
        name: "", previousStateId: "", isInitial: false, isFinal: false, allowedRoles: ["Admin", "Member"] 
      });
    } catch (error) {
      console.error(error);
      alert("Failed to add column.");
    }
  };

  const openMemberModal = async () => {
      setIsMemberModalOpen(true);
  };

  const handleAddMember = async () => {
    if (!selectedMemberId) return;
    try {
        await projectService.addMember(id, selectedMemberId, "Member");
        const pData = await projectService.getById(id);
        setProject(pData);
        setSelectedMemberId("");
        alert("Member added successfully!");
    } catch (error) {
        console.error("Add member error:", error);
        alert("Failed to add member.");
    }
  };

  const handleRemoveMember = async (userId) => {
    if(!window.confirm("Are you sure you want to remove this member?")) return;
    try {
        await projectService.removeMember(id, userId);
        const pData = await projectService.getById(id);
        setProject(pData);
    } catch (error) {
        console.error("Remove member error:", error);
        alert("Failed to remove member.");
    }
  };

  const openProjectSettings = () => {
    setProjectForm({
        name: project.name,
        description: project.description
    });
    setIsProjectSettingsOpen(true);
  };

  const handleUpdateProject = async (e) => {
    e.preventDefault();
    try {
        await projectService.update(id, projectForm);
        setProject({ ...project, ...projectForm });
        setIsProjectSettingsOpen(false);
        alert("Project updated successfully!");
    } catch (error) {
        console.error("Update project error:", error);
        alert("Failed to update project.");
    }
  };

  const handleDeleteProject = async () => {
    const confirmName = window.prompt(`To delete this project, type its name "${project.name}":`);
    if (confirmName !== project.name) {
        if(confirmName !== null) alert("Project name didn't match. Deletion cancelled.");
        return;
    }
    try {
        await projectService.delete(id);
        alert("Project deleted.");
        navigate("/projects");
    } catch (error) {
        console.error("Delete project error:", error);
        alert("Failed to delete project.");
    }
  };

  if (loading) return <div className="text-white text-center mt-20">Loading...</div>;

  return (
    <div className="h-full flex flex-col text-white">
      <div className="flex flex-col gap-4 mb-6 border-b border-gray-700 pb-4">
        <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
            <button onClick={() => navigate("/projects")} className="p-2 hover:bg-gray-800 rounded-lg">
                <ArrowLeft size={20} className="text-gray-400" />
            </button>
            <div>
                <div className="flex items-center gap-3">
                    <h1 className="text-2xl font-bold">{project?.name}</h1>
                    <button 
                        onClick={openProjectSettings}
                        className="text-gray-500 hover:text-white transition-colors"
                        title="Project Settings"
                    >
                        <Edit3 size={18} />
                    </button>
                </div>
                <p className="text-gray-400 text-sm">{project?.description}</p>
            </div>
            </div>
            
            <div className="flex gap-3">
                <button 
                    onClick={openMemberModal}
                    className="bg-gray-700 hover:bg-gray-600 text-white px-4 py-2 rounded-lg flex items-center gap-2 text-sm font-medium border border-gray-600 transition-colors"
                >
                    <Users size={18} /> Members
                </button>
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
        
        <div className="flex flex-wrap items-center gap-3 bg-[#1F2937] p-3 rounded-xl border border-gray-700">
            <div className="flex items-center gap-2 text-gray-400 mr-2">
                <Filter size={16} />
                <span className="text-sm font-medium">Filters:</span>
            </div>
            <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={14} />
                <input 
                    type="text" 
                    placeholder="Search task..." 
                    className="bg-[#111827] border border-gray-600 rounded-lg py-1.5 pl-9 pr-3 text-sm text-gray-200 outline-none focus:border-blue-500 w-48 transition-all"
                    value={filters.keyword}
                    onChange={(e) => setFilters({...filters, keyword: e.target.value})}
                />
            </div>

            <select 
                className="bg-[#111827] border border-gray-600 rounded-lg py-1.5 px-3 text-sm text-gray-200 outline-none focus:border-blue-500 cursor-pointer"
                value={filters.assignedUserId}
                onChange={(e) => setFilters({...filters, assignedUserId: e.target.value})}
            >
                <option value="">All Assignees</option>
                {orgMembers.map(m => (
                    <option key={m.id} value={m.id}>{m.username || m.userName}</option>
                ))}
            </select>
            
            <select 
                className="bg-[#111827] border border-gray-600 rounded-lg py-1.5 px-3 text-sm text-gray-200 outline-none focus:border-blue-500 cursor-pointer"
                value={filters.priority}
                onChange={(e) => setFilters({...filters, priority: e.target.value})}
            >
                <option value="all">All Priorities</option>
                <option value="2">High Priority</option>
                <option value="1">Medium Priority</option>
                <option value="0">Low Priority</option>
            </select>
            {(filters.keyword || filters.assignedUserId || filters.priority !== "all") && (
                <button 
                    onClick={() => setFilters({ keyword: "", assignedUserId: "", priority: "all" })}
                    className="ml-auto text-xs text-red-400 hover:text-red-300 flex items-center gap-1"
                >
                    <X size={12} /> Clear
                </button>
            )}
        </div>
      </div>

      <div className="flex-1 overflow-x-auto">
        <div className="flex gap-6 h-full min-w-max pb-4 px-2">
          {states.map((state) => {
            const columnTasks = filteredTasks.filter(t => t.workflowStateId === state.id);
            return (
                <div key={state.id} className="w-80 flex-shrink-0 flex flex-col group/column">
                <div className="flex items-center justify-between mb-3 px-1">
                    <h3 className="font-semibold text-gray-300 flex items-center gap-2">
                    <span className={`w-3 h-3 rounded-full ${state.isFinal ? 'bg-green-500' : state.isInitial ? 'bg-blue-500' : 'bg-yellow-500'}`}></span>
                    {state.name}
                    </h3>
                    <div className="flex items-center gap-2">
                        <span className="text-xs bg-gray-800 text-gray-500 px-2 py-1 rounded-full border border-gray-700">
                        {columnTasks.length}
                        </span>
                        <button 
                            onClick={() => handleDeleteState(state.id)}
                            className="text-gray-600 hover:text-red-500 opacity-0 group-hover/column:opacity-100 transition-opacity"
                            title="Delete Column"
                        >
                            <Trash2 size={16} />
                        </button>
                    </div>
                </div>
                
                <div className="bg-[#1F2937]/50 rounded-xl p-3 flex-1 border border-gray-800/50 min-h-[200px] space-y-3">
                    {columnTasks.map(task => {
                        const assignedMember = orgMembers.find(m => m.id === task.assignedUserId);
                        return (
                            <div key={task.id} className="bg-[#1F2937] p-4 rounded-lg border border-gray-700 hover:border-blue-500/50 shadow-sm group relative">
                            <div className="flex justify-between items-start mb-2">
                                <span className={`text-xs px-2 py-0.5 rounded ${task.priority === 2 ? 'bg-red-900/30 text-red-400' : task.priority === 1 ? 'bg-blue-900/30 text-blue-400' : 'bg-gray-700 text-gray-300'}`}>
                                {task.priority === 2 ? 'High' : task.priority === 1 ? 'Medium' : 'Low'}
                                </span>
                                <div className="flex gap-2">
                                    <button onClick={() => openEditModal(task)} className="text-gray-600 hover:text-blue-400 transition-colors">
                                        <Pencil size={14} />
                                    </button>
                                    <button onClick={() => handleDeleteTask(task.id)} className="text-gray-600 hover:text-red-400 transition-colors">
                                        <Trash2 size={14} />
                                    </button>
                                </div>
                            </div>
                            
                            <h4 className="font-medium text-gray-200 mb-1">{task.title}</h4>
                            <p className="text-xs text-gray-500 mb-3 line-clamp-2">{task.description}</p>
                            
                            {assignedMember && (
                                <div className="mb-2">
                                    <span className="text-[10px] bg-indigo-900/40 text-indigo-300 px-1.5 py-0.5 rounded border border-indigo-500/20">
                                    👤 {assignedMember.username || assignedMember.userName || "Assigned"}
                                    </span>
                                </div>
                            )}

                            <div className="pt-3 border-t border-gray-700/50 flex items-center justify-between gap-2">
                                <span className="text-xs text-gray-500">#{task.id}</span>
                                <select 
                                    className="bg-[#111827] text-xs text-gray-300 border border-gray-600 rounded px-1 py-1 outline-none focus:border-blue-500 max-w-[120px]"
                                    value={task.workflowStateId}
                                    onChange={(e) => handleStatusChange(task.id, e.target.value)}
                                >
                                    {states.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                                </select>
                            </div>
                            </div>
                        );
                    })}
                    {columnTasks.length === 0 && (
                        <div className="h-full flex items-center justify-center opacity-30">
                            <div className="text-center">
                                <p className="text-xs">No tasks</p>
                            </div>
                        </div>
                    )}
                </div>
                </div>
            );
          })}
          {states.length === 0 && (
            <div className="text-gray-500 m-auto text-center">
              <p>This project has no workflow yet.</p>
              <p className="text-sm opacity-70">Click "Add Column" to define your process.</p>
            </div>
          )}
        </div>
      </div>

      {isMemberModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
           <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-lg shadow-2xl p-6 relative">
             <button onClick={() => setIsMemberModalOpen(false)} className="absolute right-4 top-4 text-gray-400 hover:text-white">
                 <X size={20} />
             </button>
             <h3 className="text-xl font-bold mb-6 flex items-center gap-2">
                 <Users size={20} className="text-blue-400"/> Project Members
             </h3>
             
              <div className="mb-6">
                <h4 className="text-sm text-gray-400 mb-3 uppercase font-semibold">Current Team</h4>
                <div className="space-y-2 max-h-40 overflow-y-auto pr-2">
                    {project?.members?.map(member => (
                       <div key={member.id} className="flex justify-between items-center bg-[#111827] p-3 rounded-lg border border-gray-700">
                           <div className="flex items-center gap-3">
                               <div className="w-8 h-8 rounded-full bg-blue-900/50 flex items-center justify-center text-blue-200 text-xs font-bold border border-blue-500/30">
                                   {(member.userName || member.username)?.[0]?.toUpperCase() || "U"}
                               </div>
                               <div>
                                   <p className="text-sm font-medium text-gray-200">{member.userName || member.username}</p>
                                   <p className="text-xs text-gray-500">{member.email}</p>
                               </div>
                           </div>
                           <button onClick={() => handleRemoveMember(member.id)} className="text-gray-500 hover:text-red-400 p-1 rounded transition-colors">
                               <X size={16} />
                           </button>
                       </div>
                    ))}
                    {(!project?.members || project.members.length === 0) && (
                        <p className="text-gray-500 text-sm italic">
                            No members found.
                        </p>
                    )}
                </div>
            </div>
            <div className="pt-6 border-t border-gray-700">
                <h4 className="text-sm text-gray-400 mb-3 uppercase font-semibold">Add New Member</h4>
                <div className="flex gap-2">
                    <select 
                        className="flex-1 bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 text-sm"
                        value={selectedMemberId}
                        onChange={(e) => setSelectedMemberId(e.target.value)}
                    >
                        <option value="">-- Select from Organization --</option>
                        {orgMembers
                            .filter(orgMember => !project?.members?.some(pm => pm.id === orgMember.id))
                            .map(m => (
                                <option key={m.id} value={m.id}>{m.username || m.userName}</option>
                        ))}
                    </select>
                    <button 
                        onClick={handleAddMember}
                        disabled={!selectedMemberId}
                        className="bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-white px-4 py-2 rounded-lg font-medium text-sm transition-colors"
                    >
                        Add
                    </button>
                </div>
            </div>
           </div>
        </div>
      )}

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
                <input type="text" required placeholder="e.g. Backlog"
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newStateForm.name} onChange={(e) => setNewStateForm({...newStateForm, name: e.target.value})}
                />
              </div>
              <div className="flex gap-6">
                 <label className="flex items-center gap-2 cursor-pointer">
                    <input type="checkbox" className="rounded border-gray-600 bg-[#111827] text-blue-600 w-4 h-4"
                      checked={newStateForm.isInitial} onChange={(e) => setNewStateForm({...newStateForm, isInitial: e.target.checked})} />
                    <span className="text-sm text-gray-300">Is Initial?</span>
                 </label>
                 <label className="flex items-center gap-2 cursor-pointer">
                    <input type="checkbox" className="rounded border-gray-600 bg-[#111827] text-blue-600 w-4 h-4"
                      checked={newStateForm.isFinal} onChange={(e) => setNewStateForm({...newStateForm, isFinal: e.target.checked})} />
                    <span className="text-sm text-gray-300">Is Final?</span>
                 </label>
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Transition from...</label>
                <select className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newStateForm.previousStateId} onChange={(e) => setNewStateForm({...newStateForm, previousStateId: e.target.value})}>
                  <option value="">-- Independent --</option>
                  {states.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                </select>
              </div>
              <button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-medium mt-4">Create Column</button>
            </form>
          </div>
        </div>
      )}

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
                <input type="text" required className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newTask.title} onChange={(e) => setNewTask({...newTask, title: e.target.value})} />
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Description</label>
                <textarea className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 h-24 resize-none"
                  value={newTask.description} onChange={(e) => setNewTask({...newTask, description: e.target.value})} />
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Priority</label>
                <select className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={newTask.priority} onChange={(e) => setNewTask({...newTask, priority: e.target.value})}>
                  <option value="0">Low</option>
                  <option value="1">Medium</option>
                  <option value="2">High</option>
                </select>
              </div>
              <button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-medium">Create Task</button>
            </form>
          </div>
        </div>
      )}

      {isEditModalOpen && editingTask && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
          <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
            <button onClick={() => setIsEditModalOpen(false)} className="absolute right-4 top-4 text-gray-400 hover:text-white">
                <X size={20} />
            </button>
            <h3 className="text-xl font-bold mb-6">Edit Task</h3>
            <form onSubmit={handleUpdateTask} className="space-y-4">
              <div>
                <label className="block text-sm text-gray-400 mb-1">Title</label>
                <input type="text" required className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={editingTask.title} onChange={(e) => setEditingTask({...editingTask, title: e.target.value})} />
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Description</label>
                <textarea className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 h-24 resize-none"
                  value={editingTask.description} onChange={(e) => setEditingTask({...editingTask, description: e.target.value})} />
              </div>
              
              <div className="flex gap-4">
                  <div className="flex-1">
                    <label className="block text-sm text-gray-400 mb-1">Priority</label>
                    <select className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                      value={editingTask.priority} onChange={(e) => setEditingTask({...editingTask, priority: e.target.value})}>
                      <option value="0">Low</option>
                      <option value="1">Medium</option>
                      <option value="2">High</option>
                    </select>
                  </div>

                  <div className="flex-1">
                    <label className="block text-sm text-gray-400 mb-1">Assign To</label>
                    <select className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                      value={editingTask.assignedUserId} onChange={(e) => setEditingTask({...editingTask, assignedUserId: e.target.value})}>
                      <option value="">-- Unassigned --</option>
                      {orgMembers.map(member => (
                        <option key={member.id} value={member.id}>{member.username || member.userName || `User ${member.id}`}</option>
                      ))}
                    </select>
                  </div>
              </div>
               <div className="flex gap-3 mt-6">
                  <button type="button" onClick={() => setIsEditModalOpen(false)} className="flex-1 bg-gray-700 hover:bg-gray-600 text-white py-2 rounded-lg font-medium">Cancel</button>
                  <button type="submit" className="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-medium">Save Changes</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {isProjectSettingsOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
          <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
            <button onClick={() => setIsProjectSettingsOpen(false)} className="absolute right-4 top-4 text-gray-400 hover:text-white">
                <X size={20} />
            </button>
            <h3 className="text-xl font-bold mb-6">Project Settings</h3>
            
            <form onSubmit={handleUpdateProject} className="space-y-4">
              <div>
                <label className="block text-sm text-gray-400 mb-1">Project Name</label>
                <input 
                  type="text" required
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={projectForm.name}
                  onChange={(e) => setProjectForm({...projectForm, name: e.target.value})}
                />
              </div>
              <div>
                <label className="block text-sm text-gray-400 mb-1">Description</label>
                <textarea 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 h-20 resize-none"
                  value={projectForm.description}
                  onChange={(e) => setProjectForm({...projectForm, description: e.target.value})}
                />
              </div>
              
              <button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-medium">
                Save Changes
              </button>
            </form>
            <div className="mt-8 pt-6 border-t border-gray-700">
                <h4 className="text-red-400 font-bold text-sm mb-2 uppercase">Danger Zone</h4>
                <p className="text-xs text-gray-500 mb-3">
                    Once you delete a project, there is no going back. All tasks and columns will be lost.
                </p>
                <button 
                    onClick={handleDeleteProject}
                    className="w-full border border-red-900/50 bg-red-900/20 hover:bg-red-900/40 text-red-400 py-2 rounded-lg font-medium text-sm transition-colors"
                >
                    Delete Project
                </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}