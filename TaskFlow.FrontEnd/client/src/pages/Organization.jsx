import { useEffect, useState } from "react";
import { Users, UserPlus, Plus, Building2 } from "lucide-react";
import organizationService from "../services/organizationService";
import userService from "../services/userService";

export default function Organization() {
  const [loading, setLoading] = useState(true);
  const [org, setOrg] = useState(null);
  const [members, setMembers] = useState([]);
  const [inviteUsername, setInviteUsername] = useState("");
  const [inviting, setInviting] = useState(false);

  useEffect(() => {
    fetchOrgData();
  }, []);

  const fetchOrgData = async () => {
    try {
      const userData = await userService.getMe();

      if (userData.organizationId) {
        // Fetch Org Details
        const orgData = await organizationService.getCurrent();
        setOrg(orgData);

        // Fetch Members
        const membersData = await organizationService.getMembers(userData.organizationId);
        setMembers(membersData || []);
      }
    } catch (error) {
      console.error("Failed to fetch organization data:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleInvite = async (e) => {
    e.preventDefault();
    if (!inviteUsername.trim()) return;

    setInviting(true);
    try {
      await organizationService.inviteUser(inviteUsername);
      alert("User invited successfully!");
      setInviteUsername("");
      fetchOrgData(); // Refresh list
    } catch (error) {
      console.error("Invite error:", error);
      alert(error.response?.data?.message || "Failed to invite user. Make sure the username exists.");
    } finally {
      setInviting(false);
    }
  };

  if (loading) return <div className="text-white text-center mt-20">Loading...</div>;

  if (!org) {
    return (
      <div className="text-white text-center mt-20">
        <h2 className="text-2xl font-bold mb-4">No Organization Found</h2>
        <p className="text-gray-400">Please create a workspace from the Dashboard.</p>
      </div>
    );
  }

  return (
    <div className="text-white max-w-5xl mx-auto">
      <div className="mb-8 border-b border-gray-700 pb-6">
        <div className="flex items-center gap-4 mb-2">
          <div className="p-3 bg-blue-600/20 rounded-xl">
            <Building2 className="w-8 h-8 text-blue-400" />
          </div>
          <div>
            <h1 className="text-3xl font-bold">{org.name}</h1>
            <p className="text-gray-400 text-sm">Organization Management</p>
          </div>
        </div>
        {org.description && (
            <p className="mt-4 text-gray-300 bg-[#1F2937] p-4 rounded-lg border border-gray-700">
                {org.description}
            </p>
        )}
      </div>

      <div className="grid md:grid-cols-2 gap-8">
        {/* Left Column: Members List */}
        <div className="bg-[#1F2937] rounded-xl border border-gray-700 p-6 flex flex-col">
          <h3 className="text-xl font-bold mb-6 flex items-center gap-2">
            <Users className="text-blue-400" size={20} />
            Team Members <span className="text-sm text-gray-500 font-normal">({members.length})</span>
          </h3>
          
          <div className="space-y-4 overflow-y-auto pr-2 custom-scrollbar flex-1 max-h-[500px]">
            {members.map((member) => (
              <div key={member.id} className="flex items-center justify-between bg-[#111827] p-4 rounded-lg border border-gray-700 hover:border-gray-600 transition-colors">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-600 to-purple-600 flex items-center justify-center font-bold text-white shadow-lg">
                    {member.username||member.userName?.[0]?.toUpperCase() || "U"}
                  </div>
                  <div>
                    <p className="font-medium text-gray-200">{member.userName || member.username}</p>
                  </div>
                </div>
                <span className="px-2 py-1 rounded text-xs bg-gray-800 border border-gray-700 text-gray-400">
                    Member
                </span>
              </div>
            ))}
            
            {members.length === 0 && (
                <div className="text-center py-10 text-gray-500 border border-dashed border-gray-700 rounded-lg">
                    No members found.
                </div>
            )}
          </div>
        </div>

        {/* Right Column: Invite Form */}
        <div>
          <div className="bg-[#1F2937] rounded-xl border border-gray-700 p-6 sticky top-6">
            <h3 className="text-xl font-bold mb-4 flex items-center gap-2">
              <UserPlus className="text-green-400" size={20} />
              Invite Member
            </h3>
            <p className="text-gray-400 text-sm mb-6 leading-relaxed">
              Enter the <b>username</b> of the person you want to add. They will be added to the organization immediately.
            </p>

            <form onSubmit={handleInvite} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-400 mb-1">Username</label>
                <input 
                  type="text" 
                  required
                  placeholder="e.g. johndoe"
                  className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-3 text-white focus:border-blue-500 outline-none transition-all placeholder-gray-600"
                  value={inviteUsername}
                  onChange={(e) => setInviteUsername(e.target.value)}
                />
              </div>
              <button 
                type="submit"
                disabled={inviting}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 rounded-lg transition-colors flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed shadow-lg shadow-blue-900/20"
              >
                {inviting ? "Inviting..." : "Add to Team"}
                {!inviting && <Plus size={18} />}
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}