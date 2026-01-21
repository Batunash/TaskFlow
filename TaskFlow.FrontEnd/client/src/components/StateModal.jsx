import { X } from "lucide-react";

export default function StateModal({ 
  isOpen, 
  onClose, 
  states, 
  newStateForm, 
  setNewStateForm, 
  onSubmit 
}) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-md shadow-2xl p-6 relative">
        <button onClick={onClose} className="absolute right-4 top-4 text-gray-400 hover:text-white">
          <X size={20} />
        </button>
        <h3 className="text-xl font-bold mb-6">Add New Column</h3>
        <form onSubmit={onSubmit} className="space-y-4">
          <div>
            <label className="block text-sm text-gray-400 mb-1">Column Name</label>
            <input 
              type="text" required placeholder="e.g. Backlog"
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
                <span className="text-sm text-gray-300">Is Initial?</span>
             </label>
             <label className="flex items-center gap-2 cursor-pointer">
                <input 
                  type="checkbox" 
                  className="rounded border-gray-600 bg-[#111827] text-blue-600 w-4 h-4"
                  checked={newStateForm.isFinal} 
                  onChange={(e) => setNewStateForm({...newStateForm, isFinal: e.target.checked})} 
                />
                <span className="text-sm text-gray-300">Is Final?</span>
             </label>
          </div>
          <div>
            <label className="block text-sm text-gray-400 mb-1">Transition from...</label>
            <select 
              className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500"
              value={newStateForm.previousStateId} 
              onChange={(e) => setNewStateForm({...newStateForm, previousStateId: e.target.value})}
            >
              <option value="">-- Independent --</option>
              {states.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
          </div>
          <button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-medium mt-4">
            Create Column
          </button>
        </form>
      </div>
    </div>
  );
}