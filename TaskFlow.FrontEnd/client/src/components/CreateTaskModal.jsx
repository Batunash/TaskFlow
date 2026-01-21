import { X } from "lucide-react";

export default function CreateTaskModal({ isOpen, onClose, onSubmit, newTask, setNewTask }) {
  if (!isOpen) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
        <button onClick={onClose} className="absolute right-4 top-4 text-gray-400 hover:text-white">
          <X size={20} />
        </button>
        <h3 className="text-xl font-bold mb-6">Add New Task</h3>
        <form onSubmit={onSubmit} className="space-y-4">
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
  );
}