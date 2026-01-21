import { X, Users } from "lucide-react";

export default function MembersModal({ 
  isOpen, 
  onClose, 
  project, 
  orgMembers, 
  selectedMemberId, 
  setSelectedMemberId, 
  onAddMember, 
  onRemoveMember 
}) {
  if (!isOpen) return null;

  const currentMemberIds = project?.members?.map(m => m.id) || [];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div className="bg-[#1F2937] border border-gray-700 rounded-xl w-full max-w-lg shadow-2xl p-6 relative">
        <button onClick={onClose} className="absolute right-4 top-4 text-gray-400 hover:text-white">
          <X size={20} />
        </button>
        <h3 className="text-xl font-bold mb-6 flex items-center gap-2">
          <Users size={20} className="text-blue-400"/> Project Members
        </h3>
        <div className="mb-6">
          <h4 className="text-sm text-gray-400 mb-3 uppercase font-semibold">Current Team</h4>
          <div className="space-y-2 max-h-40 overflow-y-auto pr-2">
            {project?.members?.map(member => (
              <div key={member.id} className="flex justify-between items-center bg-[#111827] p-3 rounded-lg border border-gray-700">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-full bg-blue-900/50 flex items-center justify-center text-blue-200 text-xs font-bold border border-blue-500/30">
                    {(member.userName || member.username)?.[0]?.toUpperCase() || "U"}
                  </div>
                  <div>
                    <p className="text-sm font-medium text-gray-200">{member.userName || member.username}</p>
                    <p className="text-xs text-gray-500">{member.email}</p>
                  </div>
                </div>
                <button 
                  onClick={() => onRemoveMember(member.id)} 
                  className="text-gray-500 hover:text-red-400 p-1 rounded transition-colors"
                  title="Remove Member"
                >
                  <X size={16} />
                </button>
              </div>
            ))}
            {(!project?.members || project.members.length === 0) && (
              <p className="text-gray-500 text-sm italic">No members found.</p>
            )}
          </div>
        </div>
        <div className="pt-6 border-t border-gray-700">
          <h4 className="text-sm text-gray-400 mb-3 uppercase font-semibold">Add New Member</h4>
          <div className="flex gap-2">
            <select 
              className="flex-1 bg-[#111827] border border-gray-600 rounded-lg px-4 py-2 text-white outline-none focus:border-blue-500 text-sm"
              value={selectedMemberId}
              onChange={(e) => setSelectedMemberId(e.target.value)}
            >
              <option value="">-- Select from Organization --</option>
              {orgMembers
                .filter(orgMember => !currentMemberIds.includes(orgMember.id))
                .map(m => (
                  <option key={m.id} value={m.id}>{m.username || m.userName}</option>
              ))}
            </select>
            <button 
              onClick={onAddMember}
              disabled={!selectedMemberId}
              className="bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-white px-4 py-2 rounded-lg font-medium text-sm transition-colors"
            >
              Add
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}