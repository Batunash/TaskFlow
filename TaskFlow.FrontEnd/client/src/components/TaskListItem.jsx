import { useNavigate } from "react-router-dom";
import { Calendar, ArrowRight, AlertCircle, CheckCircle2, Clock } from "lucide-react";

export default function TaskListItem({ task, projectName }) {
  const navigate = useNavigate();
  const getPriorityBadge = (p) => {
    switch (p) {
      case 2: return <span className="flex items-center gap-1 text-xs font-medium text-red-400 bg-red-400/10 px-2 py-1 rounded"><AlertCircle size={12} /> High</span>;
      case 1: return <span className="flex items-center gap-1 text-xs font-medium text-blue-400 bg-blue-400/10 px-2 py-1 rounded"><Clock size={12} /> Medium</span>;
      default: return <span className="flex items-center gap-1 text-xs font-medium text-gray-400 bg-gray-400/10 px-2 py-1 rounded"><CheckCircle2 size={12} /> Low</span>;
    }
  };

  return (
    <div className="group flex flex-col md:flex-row md:items-center justify-between gap-4 bg-[#1F2937] p-4 rounded-xl border border-gray-700 hover:border-blue-500/50 transition-all hover:shadow-lg hover:shadow-blue-900/10">
      <div className="flex-1">
        <div className="flex items-center gap-3 mb-2">
          {getPriorityBadge(task.priority)}
          <span className="text-xs text-gray-500 bg-gray-800 px-2 py-1 rounded border border-gray-700">
            #{task.id}
          </span>
          {projectName && (
             <span className="text-xs text-indigo-300 bg-indigo-900/30 px-2 py-1 rounded border border-indigo-500/20">
               📂 {projectName}
             </span>
          )}
        </div>
        <h4 className="text-lg font-semibold text-gray-200 group-hover:text-blue-400 transition-colors">
          {task.title}
        </h4>
        <p className="text-sm text-gray-400 line-clamp-1 mt-1">
          {task.description || "No description provided."}
        </p>
      </div>
      <div className="flex items-center justify-between md:justify-end gap-6 min-w-[200px] border-t md:border-t-0 border-gray-700 pt-3 md:pt-0">
        <div className="flex items-center gap-2 text-xs text-gray-500">
          <Calendar size={14} />
          <span>{new Date(task.createdOn || Date.now()).toLocaleDateString()}</span>
        </div>
        
        <button 
          onClick={() => navigate(`/projects/${task.projectId}`)}
          className="flex items-center gap-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded-lg transition-colors"
        >
          Go to Board <ArrowRight size={16} />
        </button>
      </div>
    </div>
  );
}