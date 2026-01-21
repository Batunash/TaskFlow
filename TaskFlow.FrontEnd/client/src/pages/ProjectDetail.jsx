import { useEffect, useState, useMemo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Plus, ArrowLeft, Settings, Users, Edit3 } from "lucide-react"; 
import projectService from "../services/projectService";
import taskService from "../services/taskService";
import workflowService from "../services/workflowService";
import organizationService from "../services/organizationService";
import userService from "../services/userService";
import ProjectFilters from "../components/ProjectFilters";
import KanbanColumn from "../components/KanbanColumn";
import CreateTaskModal from "../components/CreateTaskModal";
import EditTaskModal from "../components/EditTaskModal";
import MembersModal from "../components/MembersModal";
import StateModal from "../components/StateModal";
import ProjectSettingsModal from "../components/ProjectSettingsModal";

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
  const [isStateModalOpen, setIsStateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isMemberModalOpen, setIsMemberModalOpen] = useState(false);
  const [isProjectSettingsOpen, setIsProjectSettingsOpen] = useState(false);
  const [newTask, setNewTask] = useState({ title: "", description: "", priority: 1 });
  const [newStateForm, setNewStateForm] = useState({
    name: "", previousStateId: "", isInitial: false, isFinal: false, allowedRoles: ["Admin", "Member"] 
  });
  const [editingTask, setEditingTask] = useState(null);
  const [selectedMemberId, setSelectedMemberId] = useState("");  
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
                    onClick={() => setIsMemberModalOpen(true)}
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
        
        <ProjectFilters 
            filters={filters} 
            setFilters={setFilters} 
            orgMembers={orgMembers} 
        />
      </div>

      <div className="flex-1 overflow-x-auto">
        <div className="flex gap-6 h-full min-w-max pb-4 px-2">
          {states.map((state) => (
             <KanbanColumn 
                key={state.id}
                state={state}
                tasks={filteredTasks.filter(t => t.workflowStateId === state.id)}
                orgMembers={orgMembers}
                states={states}
                onDeleteState={handleDeleteState}
                onEditTask={openEditModal}
                onDeleteTask={handleDeleteTask}
                onStatusChange={handleStatusChange}
             />
          ))}
          {states.length === 0 && (
            <div className="text-gray-500 m-auto text-center">
              <p>This project has no workflow yet.</p>
              <p className="text-sm opacity-70">Click "Add Column" to define your process.</p>
            </div>
          )}
        </div>
      </div>
      <CreateTaskModal 
        isOpen={isTaskModalOpen}
        onClose={() => setIsTaskModalOpen(false)}
        newTask={newTask}
        setNewTask={setNewTask}
        onSubmit={handleCreateTask}
      />

      <EditTaskModal 
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        editingTask={editingTask}
        setEditingTask={setEditingTask}
        orgMembers={orgMembers}
        onUpdate={handleUpdateTask}
      />

      <MembersModal 
        isOpen={isMemberModalOpen}
        onClose={() => setIsMemberModalOpen(false)}
        project={project}
        orgMembers={orgMembers}
        selectedMemberId={selectedMemberId}
        setSelectedMemberId={setSelectedMemberId}
        onAddMember={handleAddMember}
        onRemoveMember={handleRemoveMember}
      />

      <StateModal 
        isOpen={isStateModalOpen}
        onClose={() => setIsStateModalOpen(false)}
        states={states}
        newStateForm={newStateForm}
        setNewStateForm={setNewStateForm}
        onSubmit={handleAddState}
      />

      <ProjectSettingsModal 
        isOpen={isProjectSettingsOpen}
        onClose={() => setIsProjectSettingsOpen(false)}
        projectForm={projectForm}
        setProjectForm={setProjectForm}
        onUpdate={handleUpdateProject}
        onDelete={handleDeleteProject}
      />

    </div>
  );
}