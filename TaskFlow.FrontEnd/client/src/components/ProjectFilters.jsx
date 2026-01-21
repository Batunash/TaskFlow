import { Search, Filter, X } from "lucide-react";

export default function ProjectFilters({ filters, setFilters, orgMembers }) {
  const handleClear = () => setFilters({ keyword: "", assignedUserId: "", priority: "all" });
  const hasActiveFilters = filters.keyword || filters.assignedUserId || filters.priority !== "all";

  return (
    <div className="flex flex-wrap items-center gap-3 bg-[#1F2937] p-3 rounded-xl border border-gray-700">
      <div className="flex items-center gap-2 text-gray-400 mr-2">
        <Filter size={16} />
        <span className="text-sm font-medium">Filters:</span>
      </div>
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={14} />
        <input
          type="text"
          placeholder="Search task..."
          className="bg-[#111827] border border-gray-600 rounded-lg py-1.5 pl-9 pr-3 text-sm text-gray-200 outline-none focus:border-blue-500 w-48 transition-all"
          value={filters.keyword}
          onChange={(e) => setFilters({ ...filters, keyword: e.target.value })}
        />
      </div>

      <select
        className="bg-[#111827] border border-gray-600 rounded-lg py-1.5 px-3 text-sm text-gray-200 outline-none focus:border-blue-500 cursor-pointer"
        value={filters.assignedUserId}
        onChange={(e) => setFilters({ ...filters, assignedUserId: e.target.value })}
      >
        <option value="">All Assignees</option>
        {orgMembers.map(m => (
          <option key={m.id} value={m.id}>{m.username || m.userName}</option>
        ))}
      </select>

      <select
        className="bg-[#111827] border border-gray-600 rounded-lg py-1.5 px-3 text-sm text-gray-200 outline-none focus:border-blue-500 cursor-pointer"
        value={filters.priority}
        onChange={(e) => setFilters({ ...filters, priority: e.target.value })}
      >
        <option value="all">All Priorities</option>
        <option value="2">High Priority</option>
        <option value="1">Medium Priority</option>
        <option value="0">Low Priority</option>
      </select>
      
      {hasActiveFilters && (
        <button
          onClick={handleClear}
          className="ml-auto text-xs text-red-400 hover:text-red-300 flex items-center gap-1"
        >
          <X size={12} /> Clear
        </button>
      )}
    </div>
  );
}