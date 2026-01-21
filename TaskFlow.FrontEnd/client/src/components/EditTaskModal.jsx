import { X } from "lucide-react";

export default function EditTaskModal({ 
  isOpen, 
  onClose, 
  editingTask, 
  setEditingTask, 
  orgMembers, 
  onUpdate 
}) {
  if (!isOpen || !editingTask) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
        <button onClick={onClose} className="absolute right-4 top-4 text-gray-400 hover:text-white">
          <X size={20} />
        </button>
        <h3 className="text-xl font-bold mb-6">Edit Task</h3>
        <form onSubmit={onUpdate} className="space-y-4">
          <div>
            <label className="block text-sm text-gray-400 mb-1">Title</label>
            <input 
              type="text" required 
              className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
              value={editingTask.title} 
              onChange={(e) => setEditingTask({...editingTask, title: e.target.value})} 
            />
          </div>
          <div>
            <label className="block text-sm text-gray-400 mb-1">Description</label>
            <textarea 
              className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 h-24 resize-none"
              value={editingTask.description} 
              onChange={(e) => setEditingTask({...editingTask, description: e.target.value})} 
            />
          </div>
          
          <div className="flex gap-4">
              <div className="flex-1">
                <label className="block text-sm text-gray-400 mb-1">Priority</label>
                <select 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={editingTask.priority} 
                  onChange={(e) => setEditingTask({...editingTask, priority: e.target.value})}
                >
                  <option value="0">Low</option>
                  <option value="1">Medium</option>
                  <option value="2">High</option>
                </select>
              </div>

              <div className="flex-1">
                <label className="block text-sm text-gray-400 mb-1">Assign To</label>
                <select 
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
                  value={editingTask.assignedUserId} 
                  onChange={(e) => setEditingTask({...editingTask, assignedUserId: e.target.value})}
                >
                  <option value="">-- Unassigned --</option>
                  {orgMembers.map(member => (
                    <option key={member.id} value={member.id}>{member.username || member.userName}</option>
                  ))}
                </select>
              </div>
          </div>
           <div className="flex gap-3 mt-6">
              <button 
                type="button" 
                onClick={onClose} 
                className="flex-1 bg-gray-700 hover:bg-gray-600 text-white py-2 rounded-lg font-medium"
              >
                Cancel
              </button>
              <button 
                type="submit" 
                className="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-medium"
              >
                Save Changes
              </button>
          </div>
        </form>
      </div>
    </div>
  );
}