import { X } from "lucide-react";

export default function ProjectSettingsModal({ 
  isOpen, 
  onClose, 
  projectForm, 
  setProjectForm, 
  onUpdate, 
  onDelete 
}) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
        <button onClick={onClose} className="absolute right-4 top-4 text-gray-400 hover:text-white">
          <X size={20} />
        </button>
        <h3 className="text-xl font-bold mb-6">Project Settings</h3>
        
        <form onSubmit={onUpdate} className="space-y-4">
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
                onClick={onDelete}
                className="w-full border border-red-900/50 bg-red-900/20 hover:bg-red-900/40 text-red-400 py-2 rounded-lg font-medium text-sm transition-colors"
            >
                Delete Project
            </button>
        </div>
      </div>
    </div>
  );
}