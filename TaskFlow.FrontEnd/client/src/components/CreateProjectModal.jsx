import { X, FolderPlus, Loader2 } from "lucide-react";

export default function CreateProjectModal({ isOpen, onClose, onSubmit, loading }) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
        <button 
          onClick={onClose} 
          disabled={loading}
          className="absolute right-4 top-4 text-gray-400 hover:text-white disabled:opacity-50"
        >
          <X size={20} />
        </button>
        
        <h3 className="text-xl font-bold mb-6 flex items-center gap-2 text-white">
          <FolderPlus className="text-blue-500" /> New Project
        </h3>
        
        <form onSubmit={onSubmit} className="space-y-4">
          <div>
            <label className="block text-sm text-gray-400 mb-1">Project Name</label>
            <input 
              name="name"
              type="text" 
              required 
              disabled={loading}
              className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 disabled:opacity-50 transition-colors"
              placeholder="e.g. Mobile App Redesign"
            />
          </div>
          <div>
            <label className="block text-sm text-gray-400 mb-1">Description</label>
            <textarea 
              name="description"
              disabled={loading}
              className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 h-24 resize-none disabled:opacity-50 transition-colors"
              placeholder="Briefly describe the goals..."
            />
          </div>
          
          <button 
            type="submit" 
            disabled={loading}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-medium flex justify-center items-center gap-2 disabled:bg-blue-800 disabled:cursor-not-allowed transition-all shadow-lg shadow-blue-900/20"
          >
            {loading ? <Loader2 size={18} className="animate-spin" /> : "Create Project"}
          </button>
        </form>
      </div>
    </div>
  );
}