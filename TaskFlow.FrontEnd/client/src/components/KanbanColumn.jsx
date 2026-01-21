import { Trash2 } from "lucide-react";
import TaskCard from "./TaskCard";

export default function KanbanColumn({ state, tasks, orgMembers, states, onDeleteState, onEditTask, onDeleteTask, onStatusChange }) {
  return (
    <div className="w-80 flex-shrink-0 flex flex-col group/column">
      <div className="flex items-center justify-between mb-3 px-1">
        <h3 className="font-semibold text-gray-300 flex items-center gap-2">
          <span className={`w-3 h-3 rounded-full ${state.isFinal ? 'bg-green-500' : state.isInitial ? 'bg-blue-500' : 'bg-yellow-500'}`}></span>
          {state.name}
        </h3>
        <div className="flex items-center gap-2">
          <span className="text-xs bg-gray-800 text-gray-500 px-2 py-1 rounded-full border border-gray-700">
            {tasks.length}
          </span>
          <button
            onClick={() => onDeleteState(state.id)}
            className="text-gray-600 hover:text-red-500 opacity-0 group-hover/column:opacity-100 transition-opacity"
            title="Delete Column"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </div>

      <div className="bg-[#1F2937]/50 rounded-xl p-3 flex-1 border border-gray-800/50 min-h-[200px] space-y-3">
        {tasks.map(task => (
          <TaskCard 
            key={task.id}
            task={task}
            orgMembers={orgMembers}
            states={states}
            onEdit={onEditTask}
            onDelete={onDeleteTask}
            onStatusChange={onStatusChange}
          />
        ))}
        {tasks.length === 0 && (
          <div className="h-full flex items-center justify-center opacity-30">
            <div className="text-center">
              <p className="text-xs">No tasks</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}