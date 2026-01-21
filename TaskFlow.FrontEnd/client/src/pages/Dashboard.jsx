import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { FolderKanban, CheckSquare, Users, Activity, Building2, Plus, ArrowRight, LogOut, Check } from "lucide-react";
import userService from "../services/userService";
import projectService from "../services/projectService";
import taskService from "../services/taskService";
import organizationService from "../services/organizationService";

export default function Dashboard() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState(null);
  const [stats, setStats] = useState({
    projectCount: 0,
    taskCount: 0,
    userRole: ""
  });
  const [orgForm, setOrgForm] = useState({ name: "", description: "" });
  const [creatingOrg, setCreatingOrg] = useState(false);
  const [invitations, setInvitations] = useState([]);
  const [acceptingInvite, setAcceptingInvite] = useState(false);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const userData = await userService.getMe();
      if (!userData) throw new Error("User data could not be fetched");
      setUser(userData);

      if (userData.organizationId) {
        const [projectsData, tasksData] = await Promise.all([
          projectService.getAll(),
          taskService.getAll({ assignedUserId: userData.userId })
        ]);

        const totalTasks = tasksData?.totalCount 
                           ?? (tasksData?.items?.length) 
                           ?? (Array.isArray(tasksData) ? tasksData.length : 0);

        const totalProjects = Array.isArray(projectsData) ? projectsData.length : 0;

        setStats({
          projectCount: totalProjects,
          taskCount: totalTasks,
          userRole: userData.role || "Member"
        });
      } else {
        try {
            const invites = await organizationService.getInvitations();
            setInvitations(Array.isArray(invites) ? invites : []);
        } catch (invError) {
            setInvitations([]);
        }
      }
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateOrganization = async (e) => {
    e.preventDefault();
    setCreatingOrg(true);
    try {
      const response = await organizationService.create(orgForm);
      const token = response.accessToken || response.AccessToken;

      if (token) {
        localStorage.setItem("token", token);
        const currentUser = JSON.parse(localStorage.getItem("user") || "{}");
        localStorage.setItem("user", JSON.stringify({
            ...currentUser,
            organizationId: response.organization?.id || response.Organization?.id
        }));
        window.location.reload(); 
      } else {
        handleLogout();
      }
    } catch (error) {
      alert(error.response?.data?.message || "Failed to create organization.");
    } finally {
      setCreatingOrg(false);
    }
  };

  const handleAcceptInvite = async (orgId) => {
    if(!window.confirm("Do you want to join this organization?")) return;
    setAcceptingInvite(true);
    try {
        await organizationService.acceptInvitation(orgId);
        handleLogout(); 
    } catch (error) {
        alert(error.response?.data?.message || "Failed to accept invitation.");
        setAcceptingInvite(false);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    window.location.href = "/login";
  };

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center bg-[#111827] text-gray-400">
        <Activity className="animate-spin mr-2" /> Loading...
      </div>
    );
  }

  if (!user?.organizationId) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center p-4 text-white">
        <div className="max-w-4xl w-full bg-[#1F2937] border border-gray-700 rounded-2xl shadow-2xl p-8 md:p-12 relative overflow-hidden">
          <div className="absolute top-0 right-0 w-64 h-64 bg-blue-600/10 rounded-full blur-[80px] -mr-16 -mt-16 pointer-events-none"/>
          <div className="relative z-10">
            <div className="w-16 h-16 bg-blue-900/30 rounded-2xl flex items-center justify-center mb-6 border border-blue-500/20">
              <Building2 className="w-8 h-8 text-blue-400" />
            </div>
            <h1 className="text-3xl font-bold mb-3">Welcome, {user?.userName || "User"}!</h1>
            <p className="text-gray-400 mb-8 text-lg">
              You are not a member of any organization yet. Create a workspace or join an existing one.
            </p>
            
            <div className="grid md:grid-cols-2 gap-12">
              <div className="space-y-6">
                <h3 className="text-xl font-semibold text-white flex items-center gap-2">
                  <Plus className="w-5 h-5 text-blue-500" />
                  Create Workspace
                </h3>                
                <form onSubmit={handleCreateOrganization} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-400 mb-1">Organization Name</label>
                    <input 
                      type="text" 
                      required
                      className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all placeholder-gray-600"
                      placeholder="e.g. Acme Corp"
                      value={orgForm.name}
                      onChange={(e) => setOrgForm({...orgForm, name: e.target.value})}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-400 mb-1">Description (Optional)</label>
                    <textarea 
                      className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all h-24 resize-none placeholder-gray-600"
                      placeholder="What is this team about?"
                      value={orgForm.description}
                      onChange={(e) => setOrgForm({...orgForm, description: e.target.value})}
                    />
                  </div>
                  <button 
                    type="submit"
                    disabled={creatingOrg}
                    className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2.5 px-4 rounded-lg transition-colors flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {creatingOrg ? "Creating..." : "Create Organization"}
                    {!creatingOrg && <ArrowRight size={18} />}
                  </button>
                </form>
              </div>
              <div className="border-l border-gray-700 pl-8 flex flex-col justify-start">
                <div className="mb-6">
                    <div className="flex items-center gap-3 mb-4">
                        <div className="w-10 h-10 bg-purple-900/30 rounded-lg flex items-center justify-center border border-purple-500/20">
                           <Users className="w-5 h-5 text-purple-400" />
                        </div>
                        <h3 className="text-xl font-semibold text-white">Pending Invitations</h3>
                    </div>

                    {invitations.length > 0 ? (
                        <div className="space-y-3 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
                            <p className="text-sm text-gray-400 mb-2">You have been invited to join:</p>
                            {invitations.map((invite) => (
                                <div key={invite.organizationId} className="bg-[#111827] p-4 rounded-lg border border-gray-600 hover:border-gray-500 transition-colors flex flex-col gap-3 group">
                                    <div className="flex justify-between items-start">
                                        <div>
                                            <p className="font-bold text-white text-lg">{invite.organizationName || "Unknown Org"}</p>
                                            <p className="text-xs text-gray-500">Invited by: <span className="text-gray-300">{invite.inviterName || "Admin"}</span></p>
                                        </div>
                                        <span className="text-[10px] bg-yellow-900/30 text-yellow-500 px-2 py-1 rounded border border-yellow-500/20">Pending</span>
                                    </div>
                                    <button 
                                        onClick={() => handleAcceptInvite(invite.organizationId)}
                                        disabled={acceptingInvite}
                                        className="w-full bg-green-600 hover:bg-green-700 text-white text-sm py-2 rounded flex items-center justify-center gap-2 transition-colors font-medium shadow-lg shadow-green-900/20"
                                    >
                                        <Check size={16} /> 
                                        {acceptingInvite ? "Joining..." : "Accept & Join"}
                                    </button>
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="bg-[#111827]/50 border border-dashed border-gray-700 rounded-lg p-6 text-center">
                            <p className="text-gray-500 text-sm leading-relaxed mb-2">
                                You don't have any pending invitations.
                            </p>
                            <p className="text-xs text-gray-600">
                                Ask your team administrator to invite you via your username: <br/>
                                <span className="text-blue-400 font-mono mt-1 block text-sm">{user?.userName}</span>
                            </p>
                        </div>
                    )}
                </div>

                <div className="pt-6 border-t border-gray-700 mt-auto">
                  <button onClick={handleLogout} className="text-gray-500 hover:text-white text-sm flex items-center gap-2 transition-colors w-full justify-center hover:bg-gray-800 py-2 rounded">
                    <LogOut size={16} /> Logout from session
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="text-white">
      <header className="mb-8 border-b border-gray-800 pb-6">
        <h2 className="text-3xl font-bold text-gray-100">Dashboard</h2>
        <p className="text-gray-400 mt-2 flex items-center gap-2">
          Welcome back, <span className="text-blue-400 font-medium capitalize">{user?.userName}</span>
          <span className="w-1 h-1 bg-gray-600 rounded-full"></span>
          <span className="text-xs bg-gray-800 text-gray-400 px-2 py-1 rounded border border-gray-700">
             {stats.userRole}
          </span>
        </p>
      </header>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-[#1F2937] p-6 rounded-xl border border-gray-700 hover:border-gray-600 transition-colors shadow-sm group">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-gray-400 text-sm font-medium uppercase tracking-wider">Total Projects</h3>
            <div className="p-2 bg-blue-500/10 rounded-lg group-hover:bg-blue-500/20 transition-colors">
              <FolderKanban className="w-5 h-5 text-blue-400" />
            </div>
          </div>
          <p className="text-3xl font-bold text-gray-100">{stats.projectCount}</p>
          <div className="mt-2 text-xs text-gray-500">Active projects</div>
        </div>        
        <div className="bg-[#1F2937] p-6 rounded-xl border border-gray-700 hover:border-gray-600 transition-colors shadow-sm group">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-gray-400 text-sm font-medium uppercase tracking-wider">Total Tasks</h3>
            <div className="p-2 bg-purple-500/10 rounded-lg group-hover:bg-purple-500/20 transition-colors">
              <CheckSquare className="w-5 h-5 text-purple-400" />
            </div>
          </div>
          <p className="text-3xl font-bold text-gray-100">{stats.taskCount}</p>
          <div className="mt-2 text-xs text-gray-500">Tasks in all projects</div>
        </div>
        <div className="bg-[#1F2937] p-6 rounded-xl border border-gray-700 hover:border-gray-600 transition-colors shadow-sm group">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-gray-400 text-sm font-medium uppercase tracking-wider">Workspace</h3>
            <div className="p-2 bg-green-500/10 rounded-lg group-hover:bg-green-500/20 transition-colors">
              <Building2 className="w-5 h-5 text-green-400" />
            </div>
          </div>
          <p className="text-lg font-bold text-gray-100 truncate">Active</p>
          <div className="mt-2 text-xs text-gray-500">Organization ID: {user?.organizationId}</div>
        </div>
      </div>

      {stats.projectCount === 0 && (
        <div className="mt-12 text-center p-12 bg-[#1F2937]/50 rounded-xl border border-dashed border-gray-700">
          <FolderKanban className="w-12 h-12 text-gray-600 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-300">No projects yet</h3>
          <p className="text-gray-500 mt-2 max-w-sm mx-auto mb-6">
            Get started by creating your first project in this workspace.
          </p>
          <button 
             onClick={() => navigate("/projects")}
             className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors"
          >
            Create Project
          </button>
        </div>
      )}
    </div>
  );
}