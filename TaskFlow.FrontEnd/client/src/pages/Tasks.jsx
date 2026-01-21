import { useEffect, useState } from "react";
import { CheckSquare, Filter } from "lucide-react";
import taskService from "../services/taskService";
import projectService from "../services/projectService";
import userService from "../services/userService";
import TaskListItem from "../components/TaskListItem";

export default function Tasks() {
  const [tasks, setTasks] = useState([]);
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filterPriority, setFilterPriority] = useState("all");

  useEffect(() => {
    fetchMyTasks();
  }, []);

  const fetchMyTasks = async () => {
    try {
      const currentUser = await userService.getMe();
      const [tasksData, projectsData] = await Promise.all([
        taskService.getAll({ assignedUserId: currentUser.id }),
        projectService.getAll()
      ]);

      const rawTasks = tasksData.items || (Array.isArray(tasksData) ? tasksData : []);
      const rawProjects = Array.isArray(projectsData) ? projectsData : [];

      setTasks(rawTasks);
      setProjects(rawProjects);
    } catch (error) {
      console.error("Tasks fetch error:", error);
    } finally {
      setLoading(false);
    }
  };
  const getProjectName = (projectId) => {
    return projects.find(p => p.id === projectId)?.name || "Unknown Project";
  };
  const filteredTasks = tasks.filter(task => {
    if (filterPriority === "all") return true;
    return task.priority === parseInt(filterPriority);
  });

  if (loading) return <div className="text-center mt-20 text-gray-400">Loading your tasks...</div>;

  return (
    <div className="h-full flex flex-col text-white">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8 border-b border-gray-800 pb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-100 flex items-center gap-3">
            <CheckSquare className="text-green-400" /> My Tasks
          </h1>
          <p className="text-gray-400 mt-1">
            You have <span className="text-white font-bold">{tasks.length}</span> tasks assigned to you.
          </p>
        </div>
        <div className="flex items-center gap-3 bg-[#1F2937] px-4 py-2 rounded-lg border border-gray-700">
          <Filter size={18} className="text-gray-500" />
          <select 
            value={filterPriority}
            onChange={(e) => setFilterPriority(e.target.value)}
            className="bg-transparent text-sm text-white outline-none cursor-pointer"
          >
            <option value="all">All Priorities</option>
            <option value="2">High Priority</option>
            <option value="1">Medium Priority</option>
            <option value="0">Low Priority</option>
          </select>
        </div>
      </div>
      <div className="space-y-4 pb-8">
        {filteredTasks.length > 0 ? (
          filteredTasks.map(task => (
            <TaskListItem 
              key={task.id} 
              task={task} 
              projectName={getProjectName(task.projectId)} 
            />
          ))
        ) : (
          <div className="text-center py-20 bg-[#1F2937]/30 rounded-xl border border-dashed border-gray-800">
             <CheckSquare size={48} className="mx-auto text-gray-600 mb-4" />
             <h3 className="text-xl font-medium text-gray-300">All caught up!</h3>
             <p className="text-gray-500 mt-2">You have no tasks matching this filter.</p>
          </div>
        )}
      </div>
    </div>
  );
}