import { FolderKanban, MoreVertical, Users } from "lucide-react";
import { Link } from "react-router-dom";
export default function ProjectCard({ project }) {
  return (
    <Link to={`/projects/${project.id}`} className="block">
        <div className="bg-[#1F2937] border border-gray-700 rounded-xl p-5 hover:border-blue-500/50 transition-all group relative">
        <div className="flex justify-between items-start mb-4">
            <div className="p-2 bg-blue-900/20 rounded-lg text-blue-400 group-hover:bg-blue-600 group-hover:text-white transition-colors">
            <FolderKanban size={20} />
            </div>
            <button className="text-gray-500 hover:text-white transition-colors">
            <MoreVertical size={18} />
            </button>
        </div>
        
        <h3 className="text-lg font-semibold text-white mb-2 truncate pr-6">{project.name}</h3>
        <p className="text-gray-400 text-sm line-clamp-2 mb-4 h-10 break-words">
            {project.description || "No description provided."}
        </p>
        
        <div className="flex items-center justify-between pt-4 border-t border-gray-700/50">
            <div className="flex items-center text-xs text-gray-500 gap-1.5 bg-gray-800/50 px-2 py-1 rounded">
            <Users size={14} />
            <span>Project Team</span>
            </div>
        </div>
        </div>
    </Link>
  );
}