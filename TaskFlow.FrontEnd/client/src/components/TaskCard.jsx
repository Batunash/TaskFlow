import { Pencil, Trash2 } from "lucide-react";

export default function TaskCard({ task, orgMembers, states, onEdit, onDelete, onStatusChange }) {
  const assignedMember = orgMembers.find(m => m.id === task.assignedUserId);

  return (
    <div className="bg-[#1F2937] p-4 rounded-lg border border-gray-700 hover:border-blue-500/50 shadow-sm group relative">
      <div className="flex justify-between items-start mb-2">
        <span className={`text-xs px-2 py-0.5 rounded ${
          task.priority === 2 ? 'bg-red-900/30 text-red-400' : 
          task.priority === 1 ? 'bg-blue-900/30 text-blue-400' : 
          'bg-gray-700 text-gray-300'
        }`}>
          {task.priority === 2 ? 'High' : task.priority === 1 ? 'Medium' : 'Low'}
        </span>
        <div className="flex gap-2">
          <button onClick={() => onEdit(task)} className="text-gray-600 hover:text-blue-400 transition-colors">
            <Pencil size={14} />
          </button>
          <button onClick={() => onDelete(task.id)} className="text-gray-600 hover:text-red-400 transition-colors">
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
          onChange={(e) => onStatusChange(task.id, e.target.value)}
        >
          {states.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
        </select>
      </div>
    </div>
  );
}